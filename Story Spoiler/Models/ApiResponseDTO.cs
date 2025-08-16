using System.Text.Json.Serialization;

namespace Story_Spoiler.Models
{
    public class ApiResponseDTO
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("storyId")]
        public string? StoryId { get; set; }
    }
}
