using Core.CrossCuttingConcerns.Logging.Contants;
using Core.CrossCuttingConcerns.Logging.SeriLog.ConfigurationModels;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.MSSqlServer;

namespace Core.CrossCuttingConcerns.Logging.SeriLog.Logger
{
    public class MsSqlLogger : LoggerServiceBase
    {
        public MsSqlLogger(IConfiguration configuration)
        {
            MsSqlConfiguration logConfiguration = configuration.GetSection("SeriLogConfigurations:MsSqlConfiguration").Get<MsSqlConfiguration>()
                ?? throw new Exception(SerilogMessages.NullOptionsMessage);

            MSSqlServerSinkOptions sinkOptions = new MSSqlServerSinkOptions()
            {
                TableName = logConfiguration.TableName,
                AutoCreateSqlTable = logConfiguration.AutoCreateSqlTable,
            };

            ColumnOptions columnOptions = new ColumnOptions();

            global::Serilog.Core.Logger /*ILogger*/ seriLogConfig = new LoggerConfiguration().WriteTo.MSSqlServer(connectionString: configuration.GetConnectionString("MsSqlConnectionString"),// logConfiguration.ConnectionString,
                                                                                                                  sinkOptions: sinkOptions,
                                                                                                                  columnOptions: columnOptions)
                                                                                                     .CreateLogger();

            Logger = seriLogConfig;
        }
    }
}