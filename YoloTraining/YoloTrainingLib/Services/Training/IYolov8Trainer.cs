using YoloTrainingLib.Models;

namespace YoloTrainingLib.Services.Training
{
    public interface IYolov8Trainer
    {
        Task RunTrainingAsync(
             string datasetDir,
             List<string> classes,
             string experimentName,
             int maxEpochs,
             int batchSize,
             double learningRate,
             bool resume,
             string? weightsPath,  // null for fresh training, path for fine-tune
             string category,
             string modelName,
             Action<TrainingProgressUpdate>? onProgress,
             Action<TrainingResult>? onComplete);
    }
}