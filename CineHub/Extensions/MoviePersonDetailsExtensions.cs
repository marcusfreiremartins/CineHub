using CineHub.Models.ViewModels.Persons;

namespace CineHub.Extensions
{
    public static class MoviePersonDetailsExtensions
    {
        public static string GetActivityYears(this List<MoviePersonDetails> movies)
        {
            var moviesWithDates = movies.Where(m => m.Movie.ReleaseDate.HasValue).ToList();
            if (!moviesWithDates.Any()) return "N/A";

            var firstYear = moviesWithDates.Min(m => m.Movie.ReleaseDate!.Value.Year);
            var lastYear = moviesWithDates.Max(m => m.Movie.ReleaseDate!.Value.Year);

            return firstYear == lastYear ? firstYear.ToString() : $"{firstYear} - {lastYear}";
        }

        public static string GetFormattedAverageRating(this List<MoviePersonDetails> movies)
        {
            var uniqueMovies = movies.GroupBy(m => m.Movie.Id).Select(g => g.First()).ToList();
            var average = uniqueMovies.Where(m => m.Movie.VoteAverage > 0)
                                    .DefaultIfEmpty()
                                    .Average(m => m?.Movie.VoteAverage ?? 0);
            return average > 0 ? average.ToString("F1") : "N/A";
        }
    }
}