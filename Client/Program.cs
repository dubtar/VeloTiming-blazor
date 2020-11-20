using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Grpc.Net.Client;
using Grpc.Net.Client.Web;

namespace VeloTiming.Client
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");

			var baseAddress = new Uri(builder.HostEnvironment.BaseAddress);

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseAddress });

			builder.Services.AddSingleton(services =>
			{
				var handler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new HttpClientHandler());
				return GrpcChannel.ForAddress(baseAddress, new GrpcChannelOptions { HttpHandler = handler });
			});

			builder.Services.AddSingleton<Pages.Races.IRaceSvc, Pages.Races.RaceSvc>();

			await builder.Build().RunAsync();
		}
	}
}
