using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;

namespace NetworkMonitor
{
	public static class Program
	{
		public static int Main( string[] args )
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override( "Microsoft", LogEventLevel.Information )
				.Enrich.FromLogContext()
				.WriteTo.Console()
				.CreateLogger();

			try
			{
				CreateHostBuilder( args ).Build().Run();

				return 0;
			}
			catch(Exception e)
			{
				Log.Fatal( e, "Host terminated unexpectedly" );

				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		private static IHostBuilder CreateHostBuilder( string[] args ) => Host
			.CreateDefaultBuilder( args )
			.ConfigureWebHostDefaults( builder => builder
				.UseStartup<Startup>()
				.UseSerilog() );
	}
}
