using System.Threading.Tasks;

namespace NetworkMonitor
{
	public interface IMailService
	{
		Task SendAsync( string body );
	}
}
