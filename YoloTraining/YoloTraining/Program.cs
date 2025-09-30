using Aislr_Food_Miner.Shared.Configuration;
using CommandLine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using YoloTrainingLib.Helpers;
using YoloTrainingLib.Models;
using YoloTrainingLib.Providers;
using YoloTrainingLib.Services.ModelTesting;
using YoloTrainingLib.Services.Training;

namespace YoloTraining
{
    internal class Program
    {
        private static IPathBuilderBase _pathBuilder;
        private static IYolov8Trainer _yoloTrainer;
        private static IModelTrainingService _modelTrainingService;
        private static IModelTestingService _modelTestingService;
        private static IServiceProvider _serviceProvider;


        static int Main(string[] args)
        {
            ConfigureServices();

            return Parser.Default.ParseArguments<TrainOptions, TestOptions>(args)
                .MapResult(
                    (TrainOptions opts) => RunTrain(opts),
                    (TestOptions opts) => RunTest(opts),
                    errs => 1);
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .Build();
            services.Configure<MinerSettings>(config.GetSection("Miner"));

            var minerSettings = config.GetSection("Miner").Get<MinerSettings>();

            // Add PathBuilder (base or full)
            services.AddSingleton<IPathBuilderBase>(new PathBuilderBase(minerSettings, new CommandLineBasePathProvider()));

            // Add YOLO Trainer & Services
            services.AddSingleton<IYolov8Trainer, Yolov8Trainer>();
            services.AddSingleton<IModelTrainingService, ModelTrainingService>();

            // Add HttpClientFactory and ModelTestingService
            services.AddHttpClient(); // registers IHttpClientFactory
            services.AddSingleton<IModelTestingService, ModelTestingService>();

            // Build provider
            _serviceProvider = services.BuildServiceProvider();

            // Resolve any services you need immediately
            _pathBuilder = _serviceProvider.GetRequiredService<IPathBuilderBase>();
            _yoloTrainer = _serviceProvider.GetRequiredService<IYolov8Trainer>();
            _modelTrainingService = _serviceProvider.GetRequiredService<IModelTrainingService>();
            _modelTestingService = _serviceProvider.GetRequiredService<IModelTestingService>();
        }

        private static int RunTrain(TrainOptions opts)
        {
            Console.WriteLine($"Training model {opts.ModelName} with dataset {opts.DatasetDir}");
            _modelTrainingService.RunTrainingAsync(
                datasetDir: opts.DatasetDir,
                classes: opts.Classes.ToList(),
                experimentName: opts.ExperimentName,
                maxEpochs: opts.MaxEpoch,
                batchSize: opts.BatchSize,
                learningRate: opts.LearningRate,
                resume: opts.Resume,
                category: opts.Category, // hardcoded for now
                modelName: opts.ModelName,
                onProgress: OnTrainingProgress,
                onComplete: result =>
                {
                    Console.WriteLine("Training complete!");
                }).GetAwaiter().GetResult();
            // call your training service here
            return 0;
        }

        private static int RunTest(TestOptions opts)
        {
            Console.WriteLine($"Testing model in {opts.ModelFolder} with files from {opts.TestFileFolder}");
            var files = Directory.GetFiles(opts.TestFileFolder)
                .Select(f => (FileName: Path.GetFileName(f), Stream: (Stream)File.OpenRead(f)))
                .ToList();

            try
            {
                _modelTestingService
                    .PredictBatchAsync(opts.ModelFolder, files)
                    .GetAwaiter()
                    .GetResult();
            }
            finally
            {
                foreach (var (_, stream) in files)
                {
                    stream.Dispose();
                }
            }

            return 0;
        }

        private static void OnTrainingProgress(TrainingProgressUpdate update)
        {
            Console.WriteLine($"Epoch: {update.Epoch}, " +
                              $"Loss: {update.TrainLoss:F4}, " +
                              $"ValidationLoss: {update.ValidationLoss:P2}, " +
                              $"Accuracy: {update.Accuracy:P2}, " +
                              $"Timestamp: {update.Timestamp:P2}, " +
                              $"Message: {update.Message:P2}");
        }
    }
}
