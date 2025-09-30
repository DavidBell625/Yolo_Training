namespace YoloTrainingLib.Models
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    public class ImageStats
    {
        [JsonPropertyName("filename")]
        public string Filename { get; set; }

        [JsonPropertyName("size_bytes")]
        public long SizeBytes { get; set; }

        [JsonPropertyName("image_width")]
        public int ImageWidth { get; set; }

        [JsonPropertyName("image_height")]
        public int ImageHeight { get; set; }

        [JsonPropertyName("aspect_ratio")]
        public double AspectRatio { get; set; }

        [JsonPropertyName("total_pixels")]
        public long TotalPixels { get; set; }

        [JsonPropertyName("detections_count")]
        public int DetectionsCount { get; set; }

        [JsonPropertyName("detections_by_class")]
        public Dictionary<string, int> DetectionsByClass { get; set; }

        [JsonPropertyName("confidence_min")]
        public double? ConfidenceMin { get; set; }

        [JsonPropertyName("confidence_max")]
        public double? ConfidenceMax { get; set; }

        [JsonPropertyName("confidence_avg")]
        public double? ConfidenceAvg { get; set; }

        [JsonPropertyName("bbox_area_min")]
        public double? BboxAreaMin { get; set; }

        [JsonPropertyName("bbox_area_max")]
        public double? BboxAreaMax { get; set; }

        [JsonPropertyName("bbox_area_avg")]
        public double? BboxAreaAvg { get; set; }

        [JsonPropertyName("bbox_area_avg_pct")]
        public double? BboxAreaAvgPct { get; set; }

        [JsonPropertyName("detection_density_per_mp")]
        public double? DetectionDensityPerMp { get; set; }

        [JsonPropertyName("processing_time_sec")]
        public double ProcessingTimeSec { get; set; }
    }

}
