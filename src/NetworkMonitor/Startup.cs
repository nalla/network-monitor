using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkMonitor
{
	public class Startup
	{
		public void Configure( IApplicationBuilder app ) => app
			.UseHealthChecks( "/status" )
			.UseRouting()
			.UseEndpoints( routing => routing.MapControllers() );

		public void ConfigureServices( IServiceCollection services )
		{
			services
				.AddHostedService<DetectionService>()
				.AddTransient<IMailService, MailService>()
				.AddHealthChecks();

			services.AddMvc().SetCompatibilityVersion( CompatibilityVersion.Version_3_0 );
		}
	}
}
