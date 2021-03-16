using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using VeloTiming.Server.Services;
using VeloTiming.Server.Logic;
using VeloTiming.Server.Data;
using Microsoft.AspNetCore.ResponseCompression;
using System.Linq;

namespace VeloTiming.Server
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddDbContext<Data.RacesDbContext>(options =>
				options.UseSqlite(Configuration.GetConnectionString("DbContext")));

			services.AddSignalR();

			services.AddControllersWithViews();
			services.AddRazorPages();
			services.AddGrpc();
			services.AddResponseCompression(opts =>
			{
				opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
					new[] { "application/octet-stream" });
			});

			services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
			services.AddHostedService<QueuedHostedService>();

			services.AddSingleton<ITimeService, TimeService>();
			services.AddSingleton<IRaceLogic, RaceLogic>();

			services.AddScoped<IResultLogic, ResultLogic>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseResponseCompression();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseWebAssemblyDebugging();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseBlazorFrameworkFiles();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseGrpcWeb(new GrpcWebOptions { DefaultEnabled = true });

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
				endpoints.MapControllers();
				endpoints.MapGrpcService<RacesService>();
				endpoints.MapGrpcService<MainService>();
				endpoints.MapGrpcService<NumberService>();
				endpoints.MapGrpcService<RaceCategoryService>();
				endpoints.MapGrpcService<RidersService>();
				endpoints.MapGrpcService<StartsService>();
				endpoints.MapHub<Hubs.ResultHub>("resultHub");
				//endpoints.MapHub<RfidHub>("/rfidHub");
				endpoints.MapFallbackToFile("index.html");
			});

			// Init Singleton services
			app.ApplicationServices.GetService<IRaceLogic>();
			//app.ApplicationServices.GetService<IRfidListener>();
		}
	}
}
