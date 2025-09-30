using System.Text.Json.Serialization;

namespace YoloTrainingLib.Models
{
    public class BatchStats
    {
        [JsonPropertyName("total_images")]
        public int TotalImages { get; set; }

        [JsonPropertyName("total_detections")]
        public int TotalDetections { get; set; }

        [JsonPropertyName("detections_by_class")]
        public Dictionary<string, int> DetectionsByClass { get; set; }

        [JsonPropertyName("images_with_no_detections")]
        public int ImagesWithNoDetections { get; set; }

        [JsonPropertyName("per_image_stats")]
        public List<ImageStats> PerImageStats { get; set; }
    }
}
