using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

			builder.Services.AddSingleton<IRaceSvc, RaceSvc>();

			await builder.Build().RunAsync();
		}
	}
}
