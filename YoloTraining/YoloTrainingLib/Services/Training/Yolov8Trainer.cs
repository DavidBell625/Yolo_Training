using System.Diagnostics;
using System.Text;
using System.Text.Json;
using YoloTrainingLib.Helpers;
using YoloTrainingLib.Models;

namespace YoloTrainingLib.Services.Training
{
    public class Yolov8Trainer : IYolov8Trainer
    {
        private readonly IPathBuilderBase _pathBuilder;
        private const string DockerImage = "aislr-yolov8-trainer";
        private const string ContainerScriptPath = "/app/PythonScripts/train.py";
        private const string ContainerDatasetPath = "/data/dataset";
        private const string ContainerOutputPath = "/app/runs";
        private const string ContainerScriptsPath = "/app/PythonScripts";

        public Yolov8Trainer(IPathBuilderBase pathBuilder)
        {
            _pathBuilder = pathBuilder;
        }

        public async Task RunTrainingAsync(
            string datasetDir,
            List<string> classes,
            string experimentName,
            int maxEpochs,
            int batchSize,
            double learningRate,
            bool resume,
            string? weightsPath,
            string category,
            string modelName,
            Action<TrainingProgressUpdate>? onProgress,
            Action<TrainingResult>? onComplete)
        {
            var dockerExe = "docker";
            var hostOutputPath = Path.Combine(_pathBuilder.BaseDataPath, "Runs", category, experimentName); 
            var hostScriptsPath = Path.Combine(AppContext.BaseDirectory, "PythonScripts");

            var args = new StringBuilder();
            args.Append("run --rm --gpus all -e DEBUG=1 ");

            // Mount dataset, output, and scripts
            args.Append($"-v \"{datasetDir.Replace("\\", "/")}:{ContainerDatasetPath}\" ");
            args.Append($"-v \"{hostOutputPath.Replace("\\", "/")}:{ContainerOutputPath}\" ");
            args.Append($"-v \"{hostScriptsPath.Replace("\\", "/")}:{ContainerScriptsPath}\" ");

            // Docker image + Python unbuffered
            args.Append($"{DockerImage} python3 -u {ContainerScriptPath} ");
            args.Append($"--dataset_dir {ContainerDatasetPath} ");
            args.Append($"--experiment_name \"{experimentName}\" ");
            args.Append($"--max_epochs {maxEpochs} ");
            args.Append($"--batch_size {batchSize} ");
            args.Append($"--learning_rate {learningRate} ");

            if (string.IsNullOrEmpty(weightsPath))
            {
                // Fresh training
                args.Append("--model_size n ");
            }
            else
            {
                // Fine-tuning
                var containerWeightsFile = $"/app/runs/{modelName}/exp/weights/best.pt";
                args.Append($"--weights {containerWeightsFile} ");
            }

            if (resume)
                args.Append("--resume ");

            var psi = new ProcessStartInfo
            {
                FileName = dockerExe,
                Arguments = args.ToString(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            List<TrainingProgressUpdate> progressUpdates = new();

            process.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrWhiteSpace(e.Data)) return;

                Console.WriteLine(e.Data); // flush immediately to console

                if (!e.Data.TrimStart().StartsWith("{")) return;

                try
                {
                    using var doc = JsonDocument.Parse(e.Data);
                    var root = doc.RootElement;

                    if (root.TryGetProperty("type", out var typeElem) && typeElem.GetString() == "progress")
                    {
                        var update = JsonSerializer.Deserialize<TrainingProgressUpdate>(e.Data);
                        if (update != null)
                        {
                            progressUpdates.Add(update);
                            onProgress?.Invoke(update);
                        }
                    }
                    else if (root.TryGetProperty("type", out var typeElem2) && typeElem2.GetString() == "result")
                    {
                        var result = JsonSerializer.Deserialize<TrainingResult>(e.Data);
                        if (result != null)
                        {
                            result.ProgressUpdates = progressUpdates;
                            onComplete?.Invoke(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[JSON ERROR] {ex.Message} | Raw Line: {e.Data}");
                }
            };

            var errorBuilder = new StringBuilder();
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    Console.WriteLine($"[PYTHON STDERR]: {e.Data}");
                    errorBuilder.AppendLine(e.Data);
                }
            };

            Console.WriteLine($"[INFO] Starting YOLOv8 training via Docker:");
            Console.WriteLine($"> {psi.FileName} {psi.Arguments}");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                Console.WriteLine($"[ERROR] Docker container exited with code {process.ExitCode}");

            var errorLog = errorBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(errorLog))
            {
                Console.WriteLine("[PYTHON ERROR LOG]");
                Console.WriteLine(errorLog);
            }
        }
    }
}
