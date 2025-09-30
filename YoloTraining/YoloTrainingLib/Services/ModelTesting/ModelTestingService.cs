using System.Text.Json;
using YoloTrainingLib.Models;
using YoloTrainingLib.Providers;

namespace YoloTrainingLib.Services.ModelTesting
{
    public class ModelTestingService : IModelTestingService
    {
        private readonly IHttpProviderFactory _factory;

        public ModelTestingService(IHttpProviderFactory factory)
        {
            _factory = factory;
        }

        public async Task<PredictBatchResponse> PredictBatchAsync(
            string modelFolder,
            IEnumerable<(string FileName, Stream FileStream)> files)
        {
            if (files == null || !files.Any())
                throw new ArgumentException("At least one file must be provided.", nameof(files));

            var fileStreams = new Dictionary<string, Stream>();

            try
            {
                foreach (var (fileName, stream) in files)
                {
                    fileStreams[fileName] = stream;
                }

                var uri = $"/predict_batch?model_folder={Uri.EscapeDataString(modelFolder)}";
                var fastApiProvider = _factory.GetProvider("FastApiClient");

                var response = await fastApiProvider.PostMultipartAsync<PredictBatchResponse>(uri, fileStreams);
                var resultString = JsonSerializer.Serialize(response);
                return response;
            }
            finally
            {
                foreach (var stream in fileStreams.Values)
                {
                    await stream.DisposeAsync();
                }
            }
        }
    }
}
