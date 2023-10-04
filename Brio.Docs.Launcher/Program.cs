using System;
using System.Diagnostics;
using System.Linq;
using CommandLine;

namespace Brio.Docs.Launcher
{
    internal class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var mutName = "Brio.Docs.Launcher";
            using var mutex = new System.Threading.Mutex(true, mutName, out bool createdNew);

            if (!createdNew)
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
                    Console.WriteLine("Brio.Docs.Launcher instance is already running, exiting.");
                    return;
                }
            }

            using var app = new App();
            var options = new AppOptions();

            new Parser(with => with.EnableDashDash = true)
                .ParseArguments<AppOptions>(args)
                .WithParsed(x => options = x);

            App.Options = options;
            app.InitializeComponent();
            app.Run();
        }
    }
}
