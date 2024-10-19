using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Common.Logging;

public static class SeriLogger
{
	public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
		(context, configuration) =>
		{
			var elasticUri = context.Configuration.GetValue<string>("ElasticConfiguration:Uri");

			var indexFormat =
				$"applogs-{context.HostingEnvironment.ApplicationName?.ToLower().Replace(".", "-")}-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-logs-{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}";

			var elasticSearchOptions = new ElasticsearchSinkOptions(new Uri(elasticUri))
			{
				NumberOfReplicas = 2,
				NumberOfShards = 2,
				AutoRegisterTemplate = true,
				IndexFormat = indexFormat
			};

			configuration
				.Enrich.FromLogContext()
				.Enrich.WithMachineName()
				.WriteTo.Debug()
				.WriteTo.Console()
				.WriteTo.Elasticsearch(elasticSearchOptions)
				.Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
				.Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
				.ReadFrom.Configuration(context.Configuration);
		};
}
