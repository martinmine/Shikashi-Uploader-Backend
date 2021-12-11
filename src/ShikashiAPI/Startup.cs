using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using ShikashiAPI.AuthenticatorMiddleware;
using ShikashiAPI.Hashids.net;
using ShikashiAPI.Policies;
using ShikashiAPI.Services;

namespace ShikashiAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; set; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IKeyService, KeyService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IUploadService, UploadService>();
            services.AddSingleton<IAuthorizationHandler, UserAuthorizationHandler>();
            services.AddSingleton<IS3Service, S3Service>();
            services.AddSingleton(Configuration);
            services.AddSingleton<IHashids>(new Hashids.net.Hashids(Configuration["IdHash"]));

            services.AddControllers()
            .AddNewtonsoftJson(x =>
            {
                x.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            services.AddDbContext<PersistenceContext>(options => options.UseNpgsql(Configuration["Database:ConnectionString"]));

            services.AddCors(o => o.AddPolicy("AllowPanel", builder =>
            {
                builder.WithOrigins("http://panel.shikashi.me", "https://panel.shikashi.me")
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseCors("AllowPanel");
            
            app.UseTokenAuthentication();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
