﻿using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Data.Entity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
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
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables();
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
            services.AddInstance<IConfiguration>(Configuration);
            services.AddInstance<IHashids>(new Hashids.net.Hashids(Configuration["IdHash"]));

            services.AddMvc().AddJsonOptions(x =>
            {
                x.SerializerSettings.ContractResolver =
                 new CamelCasePropertyNamesContractResolver();
            });
            services.AddEntityFramework()
                .AddNpgsql()
                .AddDbContext<PersistenceContext>(options =>
                {
                    options.UseNpgsql(Configuration["Database:ConnectionString"]);
                });


            services.AddCors(o => o.AddPolicy("AllowPanel", builder =>
            {
                builder.WithOrigins("http://panel.shikashi.me", "https://panel.shikashi.me")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.MinimumLevel = LogLevel.Warning;
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseCors("AllowPanel");

            app.UseIISPlatformHandler();

            app.UseStaticFiles();
            app.UseTokenAuthentication();

            app.UseMvc();
        }

        // Entry point for the application.
        public static void Main(string[] args) => WebApplication.Run<Startup>(args);
    }
}
