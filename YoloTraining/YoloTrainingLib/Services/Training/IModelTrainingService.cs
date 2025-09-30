using YoloTrainingLib.Models;

namespace YoloTrainingLib.Services.Training
{
    public interface IModelTrainingService
    {
        Task RunTrainingAsync(
            string datasetDir, 
            List<string> classes, 
            string experimentName, 
            int maxEpochs, 
            int batchSize, 
            double learningRate, 
            bool resume,
            string category,
            string modelName,
            Action<TrainingProgressUpdate>? onProgress, 
            Action<TrainingResult>? onComplete);
    }
}