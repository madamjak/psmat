using PSMat.Windows;
using System;

namespace PSMat
{
    class Program
    {
        private static PisaciAutomat.Program _editor;

        public static void Main(string[] args)
        {
            if (PisaciAutomat.OperatingSystem.IsWindows())
            {
                WindowsConsole.NastavRawMode();
            }

            try
            {
                var cestaKSuboru = args != null && args.Length == 1 ? args[0] : null;

                _editor = new PisaciAutomat.Program(cestaKSuboru);
                _editor.NacitajSuborAVykresli();

                Console.TreatControlCAsInput = true;

                while (true)
                {
                    var vstup = Console.ReadKey(intercept: true);

                    _editor.SpracujVstup(vstup);

                    if (_editor.Ukonci)
                    {
                        break;
                    }

                    _editor.Prekresli();
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (PisaciAutomat.OperatingSystem.IsWindows())
                {
                    WindowsConsole.VypniRawMode();
                }

                Environment.Exit(0);
            }
        }
    }
}
