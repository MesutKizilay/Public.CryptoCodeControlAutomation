using Core.CrossCuttingConcerns.Logging.Contants;
using Core.CrossCuttingConcerns.Logging.SeriLog.ConfigurationModels;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Core.CrossCuttingConcerns.Logging.SeriLog.Logger
{
    public class FileLogger : LoggerServiceBase
    {
        private readonly IConfiguration _configuration;

        public FileLogger(IConfiguration configuration)
        {
            _configuration = configuration;
            FileLogConfiguration logConfig = configuration.GetSection("SeriLogConfigurations:FileLogConfiguration").Get<FileLogConfiguration>() ?? throw new Exception(SerilogMessages.NullOptionsMessage);

            string logFilePath = string.Format(format: "{0}{1}", arg0: Directory.GetCurrentDirectory() + logConfig.FolderPath, arg1: ".txt");

            Logger = new LoggerConfiguration().WriteTo.File(path: logFilePath,
                                                            rollingInterval: RollingInterval.Day,
                                                            retainedFileCountLimit: null,
                                                            fileSizeLimitBytes: 5000000,
                                                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {NewLine}{Message}{NewLine}{Exception}")
                                                      .CreateLogger();
        }
    }
}