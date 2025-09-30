using System.Text.Json.Serialization;

namespace YoloTrainingLib.Models
{
    public class Detection
    {
        [JsonPropertyName("class")]
        public string ClassName { get; set; }

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }

        [JsonPropertyName("bbox")]
        public List<double> Bbox { get; set; } // [x1, y1, x2, y2]

        [JsonPropertyName("bbox_area")]
        public double BboxArea { get; set; }

        [JsonPropertyName("bbox_area_pct")]
        public double BboxAreaPct { get; set; }
    }
}
