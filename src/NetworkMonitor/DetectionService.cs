using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NetworkMonitor
{
	public class DetectionService : BackgroundService
	{
		private readonly IConfiguration configuration;
		private readonly ILogger logger;
		private readonly IMailService mailService;

		public DetectionService( ILogger<DetectionService> logger, IConfiguration configuration, IMailService mailService )
		{
			this.logger = logger;
			this.configuration = configuration;
			this.mailService = mailService;
		}

		protected override async Task ExecuteAsync( CancellationToken cancellationToken )
		{
			var networkTargets = configuration.GetSection( "targets" ).Get<NetworkTarget[]>();
			var ping = new Ping();

			while( !cancellationToken.IsCancellationRequested )
			{
				foreach( NetworkTarget networkTarget in networkTargets )
				{
					NetworkState state;

					try
					{
						PingReply pingReply = await ping.SendPingAsync( networkTarget.NameOrAddress ).ConfigureAwait(false);

						state = pingReply?.Status == IPStatus.Success ? NetworkState.Up : NetworkState.Down;
						logger.LogInformation( $"{networkTarget.Description}: Current state is {state}" );
					}
					catch( Exception )
					{
						state = NetworkState.Down;
					}

					DetectionState detectionState = networkTarget.SaveState( state );

					if( detectionState > DetectionState.None )
					{
						await mailService.SendAsync( $"{networkTarget.Description}: {detectionState} detected!" ).ConfigureAwait( false );
					}
				}

				await Task.Delay( TimeSpan.FromSeconds( configuration.GetValue( "sleepTime", 10 ) ), cancellationToken ).ConfigureAwait( false );
			}
		}
	}
}
