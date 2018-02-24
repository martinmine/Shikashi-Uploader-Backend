using Microsoft.AspNetCore.Hosting;
using System.IO;
using Serilog;
using Serilog.Events;

namespace ShikashiAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Starting environment");

            var host = new WebHostBuilder()
                .UseKestrel(options => { options.Limits.MaxRequestBodySize = 1000000000; })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseStartup<Startup>()
                .UseSerilog()
                .Build();

            host.Run();
        }
    }
}
