using System.Text.Json.Serialization;

namespace CineHub.Models.DTOs
{
    public class PersonDTO
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("biography")]
        public string? Biography { get; set; }

        [JsonPropertyName("profile_path")]
        public string? ProfilePath { get; set; }

        [JsonPropertyName("birthday")]
        public string? Birthday { get; set; }

        [JsonPropertyName("deathday")]
        public string? Deathday { get; set; }

        [JsonPropertyName("place_of_birth")]
        public string? PlaceOfBirth { get; set; }

        // For cast members
        [JsonPropertyName("character")]
        public string? Character { get; set; }

        [JsonPropertyName("order")]
        public int Order { get; set; }

        // For crew members
        [JsonPropertyName("job")]
        public string? Job { get; set; }

        [JsonPropertyName("department")]
        public string? Department { get; set; }
    }

    public class MovieCreditsResponse
    {
        [JsonPropertyName("cast")]
        public List<PersonDTO> Cast { get; set; } = new();

        [JsonPropertyName("crew")]
        public List<PersonDTO> Crew { get; set; } = new();
    }
}