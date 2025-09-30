namespace YoloTrainingLib.Models
{
    public class TrainingResult
    {
        public string TrainingSessionId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public List<ClassImageCount> ClassImageCounts { get; set; } = new();
        public float AverageImageWidth { get; set; }
        public float AverageImageHeight { get; set; }

        public int TotalEpochs { get; set; }
        public int BatchSize { get; set; }
        public float LearningRate { get; set; }

        public string ModelName { get; set; } = string.Empty;
        public string DatasetPath { get; set; } = string.Empty;

        public List<TrainingProgressUpdate> ProgressUpdates { get; set; } = new();
    }

}
