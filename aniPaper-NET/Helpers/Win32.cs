using System;
using System.Runtime.InteropServices;

namespace aniPaper_NET
{
    static class Win32
    {
        public const int SPI_SETDESKWALLPAPER = 0x14;
        public const int SPIF_UPDATEINIFILE = 0x01;
        public const int SPIF_SENDWININICHANGE = 0x02;
        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_NOACTIVATE = 0x08000000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        [Flags]
        public enum SendMessageTimeoutFlags : uint
        {
            SMTO_NORMAL = 0x0,
            SMTO_BLOCK = 0x1,
            SMTO_ABORTIFHUNG = 0x2,
            SMTO_NOTIMEOUTIFNOTHUNG = 0x8,
            SMTO_ERRORONEXIT = 0x20,
        }

        [DllImport("user32.dll")]
        public static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lp_class_name, string lp_window_name);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hwnd, uint msg, IntPtr w_param, IntPtr l_param, SendMessageTimeoutFlags fu_flags, uint u_timeout, out IntPtr lpdw_result);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetParent(IntPtr hwnd_child, IntPtr hwnd_new_parent);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        public static extern int SetWindowLong32(HandleRef hwnd, int n_index, int dw_new_long);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hwnd, int n_index, IntPtr dw_new_long);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(int u_action, int u_param, string lpv_param, int fu_win_ini);

        public static IntPtr SetWindowLongPtr(HandleRef hwnd, int n_index, IntPtr dw_new_long)
        {

            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hwnd, n_index, dw_new_long);
            else
                return new IntPtr(SetWindowLong32(hwnd, n_index, dw_new_long.ToInt32()));

        }
    }
}
