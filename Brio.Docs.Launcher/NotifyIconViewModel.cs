using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using Brio.Docs.Launcher.Base;
using Brio.Docs.Launcher.Resources;

namespace Brio.Docs.Launcher
{
    public class NotifyIconViewModel : ObservableObject, IDisposable
    {
        private bool isDMRunning;
        private Process dmProcess;
        private bool isConsoleVisible = false;
        private bool isSwaggerVisible;

        public NotifyIconViewModel()
        {
            IsSwaggerVisible = App.Options.DevMode;
            ToggleConsoleCommand = new RelayCommand(ToggleConsoleVisibility);
            ExitApplicationCommand = new RelayCommand(ExitApplication);
            OpenSwaggerCommand = new RelayCommand(OpenSwagger);
            StartDmConsoleCommand = new RelayCommand(StartDocumentManagement);
            StartDocumentManagement();
        }

        public bool IsDMRunning
        {
            get => isDMRunning;
            set => SetProperty(ref isDMRunning, value);
        }

        public bool IsConsoleVisible
        {
            get => isConsoleVisible;
            set => SetProperty(ref isConsoleVisible, value);
        }

        public bool IsSwaggerVisible
        {
            get => isSwaggerVisible;
            set => SetProperty(ref isSwaggerVisible, value);
        }

        public RelayCommand ExitApplicationCommand { get; }

        public RelayCommand ToggleConsoleCommand { get; }

        public RelayCommand OpenSwaggerCommand { get; }

        public RelayCommand StartDmConsoleCommand { get; }

        public void Dispose()
        {
            if (dmProcess != null)
            {
                dmProcess.Kill();
                dmProcess.WaitForExit();
                dmProcess.Dispose();
            }
        }

        private static void Hide(IntPtr win) => SafeNativeMethods.ShowWindow(win, 0);

        private static void Show(IntPtr win) => SafeNativeMethods.ShowWindow(win, 1);

        private void OpenSwagger() => OpenUrl(Properties.Settings.Default.SwaggerPath);

        private void DmProcessDisposed(object sender, EventArgs e) => IsDMRunning = false;

        private void StartDocumentManagement()
        {
            dmProcess = Process.GetProcessesByName("Brio.Docs.Api").FirstOrDefault();
            if (dmProcess != null)
            {
                dmProcess.Kill();
                dmProcess.WaitForExit();
            }

            string path = App.Options.DMExecutable ?? Properties.Settings.Default.DMExecutablePath;
            var executablePath = Path.GetFullPath(path);
            var executableDir = Path.GetDirectoryName(executablePath);

            if (!File.Exists(executablePath))
            {
                MessageBox.Show(
                    string.Format(LocalizationResources.MessageFormat_File_not_found, path),
                    string.Empty,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            dmProcess = new Process();
            dmProcess.StartInfo.FileName = executablePath;
            dmProcess.StartInfo.CreateNoWindow = false;
            dmProcess.StartInfo.UseShellExecute = false;
            dmProcess.StartInfo.WorkingDirectory = executableDir;
            dmProcess.StartInfo.Arguments = App.Options.PassingArguments ?? string.Empty;
            dmProcess.EnableRaisingEvents = true;
            dmProcess.Exited += DmProcessDisposed;
            dmProcess.Start();

            IsDMRunning = true;

            // Waiting for the console window to open
            while (dmProcess.MainWindowHandle == IntPtr.Zero)
            {
            }

            if (!IsConsoleVisible)
                Hide(dmProcess.MainWindowHandle);
        }

        private void ToggleConsoleVisibility()
        {
            if (dmProcess == null)
            {
                MessageBox.Show(LocalizationResources.Message_Path_not_found);
                return;
            }

            IsConsoleVisible = !IsConsoleVisible;
            if (IsConsoleVisible)
                Show(dmProcess.MainWindowHandle);
            else
                Hide(dmProcess.MainWindowHandle);
        }

        private void ExitApplication() => Application.Current.Shutdown();

        private void OpenUrl(string url)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }
}
