using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace NetworkMonitor
{
	public class MailService : IMailService
	{
		private readonly IConfiguration configuration;
		private readonly ILogger logger;

		public MailService( IConfiguration configuration, ILogger<MailService> logger )
		{
			this.configuration = configuration;
			this.logger = logger;
		}

		public async Task SendAsync( string body )
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

			using( SmtpClient client = new SmtpClient() )
			{
				client.UseDefaultCredentials = false;
				client.Credentials = new NetworkCredential( user, pass );
				client.EnableSsl = ssl;
				client.Host = host;
				client.Port = port;

				MailMessage mailMessage = new MailMessage();

				mailMessage.From = new MailAddress( from );
				mailMessage.To.Add( to );
				mailMessage.Body = body;
				mailMessage.Subject = subject;

				try
				{
					await client.SendMailAsync( mailMessage ).ConfigureAwait( false );
				}
				catch( Exception e )
				{
					logger.LogError( e, e.Message );
				}
			}
		}
	}
}
