using System.Text.Json.Serialization;

namespace YoloTrainingLib.Models
{
    public class PredictBatchResponse
    {
        [JsonPropertyName("stats")]
        public BatchStats Stats { get; set; }

        [JsonPropertyName("results")]
        public List<ImageResult> Results { get; set; }
    }
}
