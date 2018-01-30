using System.Linq;

namespace NetworkMonitor
{
	public class NetworkTarget
	{
		private readonly NetworkState[] history = new NetworkState[3];
		private NetworkState lastState;
		private int loop;

		private bool IsDown => history.All( x => x == NetworkState.Down );

		private bool IsUp => history.All( x => x == NetworkState.Up );

		private bool WasDown => lastState == NetworkState.Down;

		private bool WasUp => lastState == NetworkState.Up;

		public string Description { get; set; }

		public bool HasActivity => history.Any( x => x == NetworkState.Up );

		public string NameOrAddress { get; set; }

		public DetectionState SaveState( NetworkState state )
		{
			var detectionState = DetectionState.None;

			history[loop] = state;
			loop += loop < history.Length - 1 ? 1 : -( history.Length - 1 );

			switch( state )
			{
				case NetworkState.Up:
					if( IsUp )
					{
						if( WasDown )
							detectionState = DetectionState.Startup;

						lastState = NetworkState.Up;
					}

					break;

				case NetworkState.Down:
					if( IsDown )
					{
						if( WasUp )
							detectionState = DetectionState.Shutdown;
						lastState = NetworkState.Down;
					}

					break;
			}

			return detectionState;
		}
	}
}
