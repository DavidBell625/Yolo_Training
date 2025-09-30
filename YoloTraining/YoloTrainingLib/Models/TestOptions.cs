using CommandLine;

namespace YoloTrainingLib.Models
{
    [Verb("test", HelpText = "Test a trained YOLO model.")]
    public class TestOptions
    {
        [Option("modelFolder", Required = true, HelpText = "Path to trained model folder.")]
        public string ModelFolder { get; set; }

        [Option("testFileFolder", Required = true, HelpText = "Path to test image folder.")]
        public string TestFileFolder { get; set; }
    }
}
