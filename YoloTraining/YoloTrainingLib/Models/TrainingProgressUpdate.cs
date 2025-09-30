namespace YoloTrainingLib.Models
{
    public class TrainingProgressUpdate
    {
        public int Epoch { get; set; }
        public float TrainLoss { get; set; }
        public float ValidationLoss { get; set; }
        public float? Accuracy { get; set; } // Optional, depending on metric availability
        public float? F1Score { get; set; }  // Optional
        public DateTime Timestamp { get; set; }

        public string? Message { get; set; }
        public string? Type { get; set; } // e.g., "log", "progress"
    }

}
