namespace YoloTrainingLib.Providers
{
    public class HttpProviderFactory : IHttpProviderFactory
    {
        private readonly IHttpClientFactory _clientFactory;

        public HttpProviderFactory(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IHttpProvider GetProvider(string key)
        {
            return new HttpProvider(_clientFactory, key);
        }
    }
}
