using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace NetworkMonitor
{
	public class DetectionService : BackgroundService
	{
		private IConfiguration Configuration { get; }

		private ILogger<DetectionService> Logger { get; }

		public DetectionService( ILogger<DetectionService> logger, IConfiguration configuration )
		{
			Logger = logger;
			Configuration = configuration;
		}

		protected override async Task ExecuteAsync( CancellationToken cancellationToken )
		{
			var networkTargets = Configuration.GetSection( "targets" ).Get<NetworkTarget[]>();
			var ping = new Ping();

			Logger.LogDebug( $"{nameof(DetectionService)} is starting." );

			cancellationToken.Register( () => Logger.LogDebug( "Background task is stopping." ) );

			while( !cancellationToken.IsCancellationRequested )
			{
				foreach( NetworkTarget networkTarget in networkTargets )
				{
					NetworkState state;

					try
					{
						PingReply pingReply = ping.Send( networkTarget.NameOrAddress );

						state = pingReply?.Status == IPStatus.Success ? NetworkState.Up : NetworkState.Down;
						Logger.LogDebug( $"{networkTarget.Description}: Current state is {state}" );
						Metrics.CreateGauge( "network_state", null, "host", "description" ).Labels( networkTarget.NameOrAddress, networkTarget.Description ).Set( state == NetworkState.Down ? 0 : 1 );
					}
					catch( Exception )
					{
						state = NetworkState.Down;
					}

					DetectionState detectionState = networkTarget.SaveState( state );

					if( detectionState > DetectionState.None )
						Logger.LogInformation( $"{networkTarget.Description}: {detectionState} detected!" );
				}

				await Sleep( networkTargets.Any( x => x.HasActivity ), cancellationToken );
			}

			Logger.LogDebug( "Background task is stopping." );
		}

		private async Task Sleep( bool anyNetworkTargetHasActivity, CancellationToken cancellationToken )
		{
			int activitySleepTime = Configuration.GetValue( "activitySleepTime", 10 );
			int idleSleepTime = Configuration.GetValue( "idleSleepTime", 60 );
			TimeSpan sleepTime = TimeSpan.FromSeconds( anyNetworkTargetHasActivity ? activitySleepTime : idleSleepTime );

			await Task.Delay( sleepTime, cancellationToken );
		}
	}
}
