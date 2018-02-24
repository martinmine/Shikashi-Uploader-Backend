using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Events;
using ShikashiAPI.AuthenticatorMiddleware;
using ShikashiAPI.Hashids.net;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;

namespace ShikashiAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.

            services.AddSingleton<IKeyService, KeyService>();
            services.AddSingleton<IUserService, UserService>();
            services.AddSingleton<IUploadService, UploadService>();
            services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IHashids>(new Hashids.net.Hashids(Configuration["IdHash"]));

            services.AddMvc().AddJsonOptions(x =>
            {
                x.SerializerSettings.ContractResolver =
                 new CamelCasePropertyNamesContractResolver();
            });

            services.AddDbContext<PersistenceContext>(options => options.UseNpgsql(Configuration["Database:ConnectionString"]));

            services.AddCors(o => o.AddPolicy("AllowPanel", builder =>
            {
                builder.WithOrigins("http://panel.shikashi.me", "https://panel.shikashi.me")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("AllowPanel");
            
            app.UseTokenAuthentication();

            app.UseMvc();
        }
    }
}
