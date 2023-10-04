using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Brio.Docs.Api.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Brio.Docs.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var mutexName = "Global\\Brio.Docs.Api";

            using var singleAppMutex = new Mutex(true, mutexName, out bool isNew);

            if (!isNew)
            {
                var currentProcess = Process.GetCurrentProcess();
                var processes = Process.GetProcessesByName(currentProcess.ProcessName);
                var needToExit = false;

                foreach (var process in processes.Where(x => x.Id != currentProcess.Id))
                {
                    if (!string.Equals(currentProcess.MainModule?.FileName, process.MainModule?.FileName))
                        process.Kill(true);
                    else
                        needToExit = true;
                }

                if (needToExit)
                {
                    Console.WriteLine("Brio.Docs.Api service instance is already running, exiting.");
                    return;
                }
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
            => Host.CreateDefaultBuilder(args)
               .UseSerilog(
                    (context, services, configuration) => configuration.DestructureByIgnoringSensitive()
                       .ReadFrom.Configuration(context.Configuration))
               .ConfigureWebHostDefaults(
                    webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                    });
    }
}
