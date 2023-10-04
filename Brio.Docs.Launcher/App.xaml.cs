using System;
using System.Threading;
using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace Brio.Docs.Launcher
{
    /// <summary>
    /// Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application, IDisposable
    {
        private TaskbarIcon notifyIcon;
        private bool isDisposed;

        public static AppOptions Options { get; internal set; }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!string.IsNullOrEmpty(Options.LanguageTag))
            {
                try
                {
                    Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Options.LanguageTag);
                }
                catch (System.Globalization.CultureNotFoundException)
                {
                    System.Diagnostics.Debug.WriteLine($"Culture {Options.LanguageTag} is not found");
                }
            }

            // create the notifyicon (it's a resource declared in NotifyIconResources.xaml
            notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (notifyIcon != null)
                    {
                        notifyIcon.Dispose(); // the icon would clean up automatically, but this is cleaner
                        if (notifyIcon.DataContext is IDisposable disposable)
                            disposable.Dispose();
                    }
                }

                isDisposed = true;
            }
        }
    }
}
