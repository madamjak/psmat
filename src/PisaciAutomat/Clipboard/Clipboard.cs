using System;

namespace PisaciAutomat.Clipboard
{
    public static class Clipboard
    {
        public static string PreciajZClipboardu()
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            try
            {
                string clipboardText = WindowsClipboardHelper.GetClipboardText();

                return clipboardText;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static void SkopirujDoClipboardu(string text)
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            try
            {
                WindowsClipboardHelper.SetClipboardText(text);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
