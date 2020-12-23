using BilibiliLiveRecordDownLoader.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ReactiveUI;
using Serilog;
using Serilog.Events;
using Splat;
using Splat.Microsoft.Extensions.DependencyInjection;

namespace BilibiliLiveRecordDownLoader.Services
{
	public static class DI
	{
		private readonly static SubjectMemorySink MemorySink = new(Constants.OutputTemplate);

		public static T GetService<T>()
		{
			return Locator.Current.GetService<T>();
		}

		public static void CreateLogger()
		{
			Log.Logger = new LoggerConfiguration()
#if DEBUG
				.MinimumLevel.Debug()
				.WriteTo.Async(c => c.Debug(outputTemplate: Constants.OutputTemplate))
#else
				.MinimumLevel.Information()
#endif
				.MinimumLevel.Override(@"Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Async(c => c.File(Constants.LogFile,
						outputTemplate: Constants.OutputTemplate,
						rollingInterval: RollingInterval.Day,
						fileSizeLimitBytes: Constants.MaxLogFileSize))
				.WriteTo.Async(c => c.Sink(MemorySink))
				.CreateLogger();
		}

		public static void Register()
		{
			var services = new ServiceCollection();

			services.UseMicrosoftDependencyResolver();
			Locator.CurrentMutable.InitializeSplat();
			Locator.CurrentMutable.InitializeReactiveUI(RegistrationNamespace.Wpf);

			ConfigureServices(services);
		}

		private static IServiceCollection ConfigureServices(IServiceCollection services)
		{
			services.AddViewModels()
					.AddViews()
					.AddDanmuClients()
					.AddConfig()
					.AddDynamicData()
					.AddFlvProcessor()
					.AddStartupService()
					.AddGlobalTaskQueue()
					.AddBilibiliApiClient()
					.AddHttpDownloader()
					.AddLogging(c => c.AddSerilog());

			services.TryAddSingleton(MemorySink);

			return services;
		}
	}
}