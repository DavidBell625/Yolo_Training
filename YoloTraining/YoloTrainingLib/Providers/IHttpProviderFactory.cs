namespace YoloTrainingLib.Providers
{
    public interface IHttpProviderFactory
    {
        IHttpProvider GetProvider(string key);
    }
}