using System;
using System.Runtime.InteropServices;

namespace Brio.Docs.Launcher
{
    internal static class SafeNativeMethods
    {
        // Link: https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-showwindow
        [DllImport("user32.dll")]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}
