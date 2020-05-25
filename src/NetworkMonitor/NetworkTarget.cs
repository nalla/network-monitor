using System.Linq;

namespace NetworkMonitor
{
	public class NetworkTarget
	{
		private readonly NetworkState[] history = new NetworkState[3];
		private NetworkState lastState;
		private int loop;

		public string Description { get; set; }

		public string NameOrAddress { get; set; }

		public DetectionState SaveState( NetworkState state )
		{
			var detectionState = DetectionState.None;

			history[loop] = state;
			loop += loop < history.Length - 1 ? 1 : -( history.Length - 1 );

			switch( state )
			{
				case NetworkState.Up:
					if( history.All( x => x == NetworkState.Up ) )
					{
						if( lastState == NetworkState.Down )
						{
							detectionState = DetectionState.Startup;
						}

						lastState = NetworkState.Up;
					}

					break;

				case NetworkState.Down:
					if( history.All( x => x == NetworkState.Down ) )
					{
						if( lastState == NetworkState.Up )
						{
							detectionState = DetectionState.Shutdown;
						}
						lastState = NetworkState.Down;
					}

					break;
			}

			return detectionState;
		}
	}
}
