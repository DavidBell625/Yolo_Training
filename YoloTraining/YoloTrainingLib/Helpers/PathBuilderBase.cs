using Aislr_Food_Miner.Shared.Configuration;
using YoloTrainingLib.Models;

namespace YoloTrainingLib.Helpers
{
    public class PathBuilderBase : IPathBuilderBase
    {
        private readonly MinerSettings _settings;

        public string BaseDataPath { get; set;}

        public PathBuilderBase(
            MinerSettings settings,
            IBasePathProvider basePathPovider
            )
        {
            _settings = settings;
            BaseDataPath = Path.Combine(basePathPovider.BasePath, "..", _settings.BaseDataPath);
        }

        public string GetCategoryPath(string category)
        {
            return Path.Combine(BaseDataPath, _settings.ImageBase, category);
        }

        public string Combine(params string[] parts)
        {
            var all = new List<string> { _settings.BaseDataPath };
            all.AddRange(parts);
            return Path.Combine(all.ToArray());
        }
    }
}
