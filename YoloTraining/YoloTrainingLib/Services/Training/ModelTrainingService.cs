using YoloTrainingLib.Models;

namespace YoloTrainingLib.Services.Training
{
    public class ModelTrainingService : IModelTrainingService
    {
        private readonly IYolov8Trainer _yoloTrainer;

        public ModelTrainingService(IYolov8Trainer yoloTrainer)
        {
            _yoloTrainer = yoloTrainer;
        }

        public Task RunTrainingAsync(
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
            Action<TrainingResult>? onComplete)
        {
            return _yoloTrainer.RunTrainingAsync(
                datasetDir: datasetDir,
                classes: classes,
                experimentName: experimentName,
                maxEpochs: maxEpochs,
                batchSize: batchSize,
                learningRate: learningRate,
                resume: resume,
                weightsPath: null, // fresh training, no pre-trained weights provided
                category: category,
                modelName: modelName, 
                onProgress: onProgress,
                onComplete: onComplete
            );
        }
    }
}
