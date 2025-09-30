using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoloTrainingLib.Helpers
{
    public class CommandLineBasePathProvider : IBasePathProvider
    {
        private readonly string _basePath;

        public CommandLineBasePathProvider(string? basePath = null)
        {
            // Default to where the executable is located if not provided
            _basePath = basePath ?? AppContext.BaseDirectory;
        }

        public string BasePath => _basePath;
    }
}
