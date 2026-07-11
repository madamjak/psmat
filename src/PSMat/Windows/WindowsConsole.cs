using System;
using System.Runtime.InteropServices;

namespace PSMat.Windows
{
    /// <summary>
    /// Bing Copilot
    /// https://learn.microsoft.com/en-us/windows/console/high-level-console-modes
    /// </summary>
    internal static class WindowsConsole
    {
        const int STD_INPUT_HANDLE = -10;
        const uint ENABLE_ECHO_INPUT = 0x0004;
        const uint ENABLE_LINE_INPUT = 0x0002;
        const uint ENABLE_PROCESSED_INPUT = 0x0001;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll")]
        static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

        [DllImport("kernel32.dll")]
        static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

        internal static void NastavRawMode()
        {
            var handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out uint mode);

            // Disable processed, echo, and line input for raw mode
            mode &= ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT | ENABLE_PROCESSED_INPUT);
            SetConsoleMode(handle, mode);
        }

        internal static void VypniRawMode()
        {
            var handle = GetStdHandle(STD_INPUT_HANDLE);
            GetConsoleMode(handle, out uint mode);

            // Enable processed, echo, and line input for cooked mode
            mode = ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT | ENABLE_PROCESSED_INPUT;
            SetConsoleMode(handle, mode);
        }
    }
}
