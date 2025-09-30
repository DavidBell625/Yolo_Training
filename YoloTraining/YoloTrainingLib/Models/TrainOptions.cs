using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YoloTrainingLib.Models
{
    [Verb("train", HelpText = "Train a YOLO model.")]
    public class TrainOptions
    {
        [Option("datasetDir", Required = true, HelpText = "Path to dataset directory.")]
        public string DatasetDir { get; set; }

        [Option("classes", Required = true, Separator = ',', HelpText = "Comma-separated list of class names.")]
        public IEnumerable<string> Classes { get; set; }

        [Option("experimentName", Required = true, HelpText = "Experiment name.")]
        public string ExperimentName { get; set; }

        [Option("maxEpoch", Default = 100, HelpText = "Maximum number of epochs.")]
        public int MaxEpoch { get; set; }

        [Option("batchSize", Default = 16, HelpText = "Batch size.")]
        public int BatchSize { get; set; }

        [Option("learningRate", Default = 0.001, HelpText = "Learning rate.")]
        public double LearningRate { get; set; }

        [Option("resume", Default = false, HelpText = "Resume training from checkpoint.")]
        public bool Resume { get; set; }

        [Option("modelName", Required = true, HelpText = "Base model name (e.g., yolo_nas_s).")]
        public string ModelName { get; set; }

        [Option("category", Required = true, HelpText = "Category for training.")]
        public string Category { get; set; }
    }
}
