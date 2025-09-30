using YoloTrainingLib.Models;

namespace YoloTrainingLib.Services.ModelTesting
{
    public interface IModelTestingService
    {
        Task<PredictBatchResponse> PredictBatchAsync(string modelFolder, IEnumerable<(string FileName, Stream FileStream)> files);
    }
}