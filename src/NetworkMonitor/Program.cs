using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.Email;

namespace NetworkMonitor
{
	public static class Program
	{
		public static void Main( string[] args )
		{
			BuildWebHost( args ).Run();
		}

		private static IWebHost BuildWebHost( string[] args )
		{
			IConfigurationRoot hostConfig = new ConfigurationBuilder()
				.SetBasePath( Directory.GetCurrentDirectory() )
				.AddJsonFile( "hosting.json", true )
				.Build();

			return WebHost.CreateDefaultBuilder( args )
				.UseConfiguration( hostConfig )
				.UseStartup<Startup>()
				.UseSerilog( ( hostingContext, loggerConfiguration ) =>
				{
					loggerConfiguration.ReadFrom.Configuration( hostingContext.Configuration );
					loggerConfiguration.WriteToEmail( hostingContext.Configuration );
				} )
				.Build();
		}

		private static void WriteToEmail( this LoggerConfiguration loggerConfiguration, IConfiguration configuration )
		{
			IConfigurationSection mailSettings = configuration.GetSection( "mail" );

			if( !mailSettings.GetValue<bool>( "enable" ) )
				return;

			string from = mailSettings.GetValue<string>( "from" );
			string to = mailSettings.GetValue<string>( "to" );
			string subject = mailSettings.GetValue<string>( "subject" );
			string host = mailSettings.GetValue<string>( "host" );
			int port = mailSettings.GetValue<int>( "port" );
			bool ssl = mailSettings.GetValue<bool>( "ssl" );
			string user = mailSettings.GetValue<string>( "user" );
			string pass = mailSettings.GetValue<string>( "pass" );

			loggerConfiguration.WriteTo.Email( new EmailConnectionInfo
			{
				FromEmail = from,
				ToEmail = to,
				MailServer = host,
				NetworkCredentials = new NetworkCredential( user, pass ),
				EmailSubject = subject,
				EnableSsl = ssl,
				Port = port
			} );
		}
	}
}
