using CineHub.Data;
using CineHub.Models;
using CineHub.Models.DTOs;
using CineHub.Models.Enum;
using CineHub.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CineHub.Services
{
    public class PersonService
    {
        private readonly ApplicationDbContext _context;
        private readonly TMDbService _tmdbService;

        public PersonService(ApplicationDbContext context, TMDbService tmdbService)
        {
            _context = context;
            _tmdbService = tmdbService;
        }

        // Retrieves the movie credits (cast and crew) for a given movie, attempting from API and falling back to database if API fails
        public async Task<List<(Person Person, PersonRole Role, string? Character)>> GetMovieCreditsAsync(Movie movie)
        {
            try
            {
                var (cast, crew) = await _tmdbService.GetMovieCreditsAsync(movie.TMDbId);
                var allPersonDetails = await GetAllPersonDetailsFromApiAsync(cast, crew);
                return await ProcessCreditsFromApiDataAsync(movie.Id, allPersonDetails, cast, crew);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching credits from API: {ex.Message}");
                return await GetMovieCreditsFromDatabaseAsync(movie.Id);
            }
        }

        // Fetches detailed person information from the API for all relevant cast and crew members
        private async Task<Dictionary<int, PersonDTO>> GetAllPersonDetailsFromApiAsync(
         List<PersonDTO> cast,
         List<PersonDTO> crew)
        {
            var allPersonIds = cast.Select(c => c.Id)
                .Union(crew.Where(c => ShouldIncludeCrewMember(c.Job)).Select(c => c.Id))
                .Distinct()
                .ToList();

            var personDetails = new Dictionary<int, PersonDTO>();
            var tasks = allPersonIds.Select(async id =>
            {
                try
                {
                    var details = await _tmdbService.GetPersonDetailsAsync(id);
                    if (details != null)
                    {
                        lock (personDetails)
                        {
                            personDetails[id] = details;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting person details for ID {id}: {ex.Message}");
                }
            });

            await Task.WhenAll(tasks);
            return personDetails;
        }

        private bool ShouldIncludeCrewMember(string? job)
        {
            if (string.IsNullOrWhiteSpace(job))
                return false;

            return PersonRoleExtensions.ShouldIncludeJob(job);
        }

        // Processes the API data for credits, updates or adds people in database, maps roles, and returns the structured credits list
        private async Task<List<(Person Person, PersonRole Role, string? Character)>> ProcessCreditsFromApiDataAsync(
            int movieId,
            Dictionary<int, PersonDTO> personDetails,
            List<PersonDTO> cast,
            List<PersonDTO> crew)
        {
            var allPersonIds = personDetails.Keys.ToList();
            var existingPeople = await _context.People
                .Where(p => allPersonIds.Contains(p.TMDbId))
                .ToDictionaryAsync(p => p.TMDbId, p => p);

            var newPeople = new List<Person>();
            var peopleToUpdate = new List<Person>();
            var personMap = new Dictionary<int, Person>();

            foreach (var (tmdbId, personDto) in personDetails)
            {
                Person person;

                if (existingPeople.TryGetValue(tmdbId, out var existingPerson))
                {
                    if ((DateTime.UtcNow - existingPerson.LastUpdated).TotalDays > 7)
                    {
                        UpdatePersonFromDto(existingPerson, personDto);
                        peopleToUpdate.Add(existingPerson);
                    }
                    person = existingPerson;
                }
                else
                {
                    person = new Person
                    {
                        TMDbId = personDto.Id,
                        Name = personDto.Name,
                        Biography = personDto.Biography,
                        ProfilePath = personDto.ProfilePath,
                        Birthday = ParseDate(personDto.Birthday),
                        Deathday = ParseDate(personDto.Deathday),
                        PlaceOfBirth = personDto.PlaceOfBirth,
                        LastUpdated = DateTime.UtcNow
                    };
                    newPeople.Add(person);
                }

                personMap[tmdbId] = person;
            }

            var credits = new List<(Person Person, PersonRole Role, string? Character)>();
            var moviePersonRelations = new List<MoviePerson>();

            // Process cast
            foreach (var castMember in cast.OrderBy(c => c.Order))
            {
                if (personMap.TryGetValue(castMember.Id, out var person))
                {
                    credits.Add((person, PersonRole.Actor, castMember.Character));
                    moviePersonRelations.Add(new MoviePerson
                    {
                        MovieId = movieId,
                        Person = person,
                        Role = PersonRole.Actor,
                        Character = castMember.Character,
                        Order = castMember.Order
                    });
                }
            }

            foreach (var crewMember in crew)
            {
                if (personMap.TryGetValue(crewMember.Id, out var person) &&
                    !string.IsNullOrEmpty(crewMember.Job))
                {
                    // Check if the job should be included
                    if (!PersonRoleExtensions.ShouldIncludeJob(crewMember.Job))
                        continue;

                    // Map the job to a PersonRole
                    var role = PersonRoleExtensions.MapFromJob(crewMember.Job);

                    // Avoid duplicates
                    var existingCredit = credits.FirstOrDefault(c =>
                        c.Person.TMDbId == person.TMDbId && c.Role == role);

                    if (existingCredit.Person == null)
                    {
                        // For CrewMember, keep the original job in Character
                        var character = role == PersonRole.CrewMember ? crewMember.Job : null;

                        credits.Add((person, role, character));
                        moviePersonRelations.Add(new MoviePerson
                        {
                            MovieId = movieId,
                            Person = person,
                            Role = role,
                            Character = character,
                            Order = crewMember.Order
                        });
                    }
                }
            }

            await SaveCreditsInBatchAsync(movieId, newPeople, peopleToUpdate, moviePersonRelations);

            // Use extension method to order by importance
            return credits.OrderBy(c => c.Role.GetImportance()).ToList();
        }

        // Saves new and updated people and their movie relations in a transactional batch to the database
        private async Task SaveCreditsInBatchAsync(
            int movieId,
            List<Person> newPeople,
            List<Person> peopleToUpdate,
            List<MoviePerson> moviePersonRelations)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (newPeople.Any())
                {
                    await _context.People.AddRangeAsync(newPeople);
                    await _context.SaveChangesAsync();
                }

                if (peopleToUpdate.Any())
                {
                    _context.People.UpdateRange(peopleToUpdate);
                    await _context.SaveChangesAsync();
                }

                var existingRelations = await _context.MoviePeople
                    .Where(mp => mp.MovieId == movieId)
                    .ToListAsync();

                if (existingRelations.Any())
                {
                    _context.MoviePeople.RemoveRange(existingRelations);
                    await _context.SaveChangesAsync();
                }

                if (moviePersonRelations.Any())
                {
                    var uniqueRelations = moviePersonRelations
                        .GroupBy(r => new { r.MovieId, r.Person.TMDbId, r.Role })
                        .Select(g => g.First())
                        .ToList();

                    foreach (var relation in uniqueRelations)
                    {
                        if (relation.Person.Id == 0)
                        {
                            var person = await _context.People
                                .FirstOrDefaultAsync(p => p.TMDbId == relation.Person.TMDbId);
                            if (person != null)
                            {
                                relation.PersonId = person.Id;
                                relation.Person = person;
                            }
                        }
                    }

                    await _context.MoviePeople.AddRangeAsync(uniqueRelations);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error saving credits: {ex.Message}");
                throw;
            }
        }

        // Retrieves movie credits from the database fallback
        private async Task<List<(Person Person, PersonRole Role, string? Character)>> GetMovieCreditsFromDatabaseAsync(int movieId)
        {
            var results = await _context.MoviePeople
                .Where(mp => mp.MovieId == movieId)
                .Include(mp => mp.Person)
                .OrderBy(mp => mp.Order)
                .Select(mp => new { mp.Person, mp.Role, mp.Character })
                .ToListAsync();

            return results.Select(r => (r.Person, r.Role, r.Character)).ToList();
        }

        // Updates a Person entity with data from a PersonDTO
        private void UpdatePersonFromDto(Person person, PersonDTO dto)
        {
            person.Name = dto.Name;
            person.Biography = dto.Biography;
            person.ProfilePath = dto.ProfilePath;
            person.Birthday = ParseDate(dto.Birthday);
            person.Deathday = ParseDate(dto.Deathday);
            person.PlaceOfBirth = dto.PlaceOfBirth;
            person.LastUpdated = DateTime.UtcNow;
        }

        // Parses a date string into a nullable DateTime
        private DateTime? ParseDate(string? dateStr)
        {
            if (string.IsNullOrEmpty(dateStr)) return null;
            return DateTime.TryParse(dateStr, out DateTime date) ? date : null;
        }
    }
}