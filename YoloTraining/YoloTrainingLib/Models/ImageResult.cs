using System.Text.Json.Serialization;

namespace YoloTrainingLib.Models
{
    public class ImageResult
    {
        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("detections")]
        public List<Detection> Detections { get; set; }

        [JsonPropertyName("annotated_image")]
        public string AnnotatedImage { get; set; } // base64 JPEG

        [JsonPropertyName("stats")]
        public ImageStats Stats { get; set; }
    }
}
