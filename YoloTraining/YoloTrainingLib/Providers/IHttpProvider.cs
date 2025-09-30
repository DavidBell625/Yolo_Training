namespace YoloTrainingLib.Providers
{
    public interface IHttpProvider
    {
        Task<T> GetAsync<T>(string uri);
        Task<TResponse> PostAsync<TRequest, TResponse>(string uri, TRequest content);
        Task<TResponse> PostMultipartAsync<TResponse>(string uri, Dictionary<string, Stream> files, Dictionary<string, string>? fields = null);
    }
}