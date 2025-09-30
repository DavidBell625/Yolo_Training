namespace Aislr_Food_Miner.Shared.Configuration
{
    public class MinerSettings
    {
        public string BaseDataPath { get; set; }
        public string ImageBase { get; set; }
        public string ImageDownloader { get; set; }
        public string ImageVerifierAccepted { get; set; }
        public string ImageVerifierRejected { get; set; }
        public string ImageCropping { get; set; }
        public string ImageAnnotationsBackground { get; set; }
        public string ImageAnnotationsAugmented { get; set; }
        public string ImageAugmented { get; set; }
        public string ImageBackgroundRemover { get; set; }
        public string PexelsApiKey { get; set; }
        public string BackgroundDownload { get; set; }
        public string ImageComposites { get; set; }
        public string YoloDataset { get; set; }
    }

}
