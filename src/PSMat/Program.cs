using PisaciAutomat.Config;
using PisaciAutomat.Config.Locale;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Chyby;
using PSMat.Windows;
using System;
using System.Threading;

namespace PSMat
{
    class Program
    {
        private struct AkciaSEditorom
        {
            public ConsoleKeyInfo vstup { get; set; }
            public int NovaVyskaKonzoly { get; set; }
            public int NovaSirkaKonzoly { get; set; }
            public bool Resize { get; set; }
        }

        private static bool _rawMode;
        private static readonly object _lockObject = new object();

        private static bool _chyba;

        public static void Main(string[] args)
        {
            var consoleBgColor = Console.BackgroundColor;
            var consoleFgColor = Console.ForegroundColor;

            try
            {
                if (PisaciAutomat.OperatingSystem.IsWindows())
                {
                    WindowsConsole.NastavRawMode();
                    _rawMode = true;
                    Console.TreatControlCAsInput = true;
                }

                NacitajSubor(args);

                Thread resizeThread = new Thread(ResizeListener)
                {
                    IsBackground = true // Ends when main app ends
                };
                resizeThread.Start();

                var akciSeditorom = new AkciaSEditorom();
                while (true)
                {
                    if (_chyba)
                    {
                        throw new ApplicationException("Neocakavana chyba.");
                    }

                    akciSeditorom.vstup = Console.ReadKey(intercept: true);

                    if (!OperaciaSEditorom(akciSeditorom))
                    {
                        break;
                    }
                }

                ResetujPozadieKonzoly(consoleBgColor, consoleFgColor);
            }
            catch (Exception ex)
            {
                _chyba = true;

                Console.Write(VykreslovaciAutomat.EraseScreen());
                ResetujPozadieKonzoly(consoleBgColor, consoleFgColor);
                
                var logger = ErrorLogger.GetInstance();
                logger.Log(new Chyba() { Ex = ex });

                try
                {
                    var cesta = logger.UlozDoSuboru();
                    var cestaZalohy = PisaciAutomat.Program.GetInstance().UlozZalohu();

                    var sprava =
                        string.Format(Lokalizacia.Hlasky.NeocakavanaChyba,
                        Farby.AnsiStyl(Farby.StylTextu.Cyan),
                        "https://github.com/madamjak/psmat/issues/",
                        Farby.AnsiReset2(),
                        Farby.AnsiStyl(Farby.StylTextu.Yellow),
                        cesta,
                        Farby.AnsiReset2());
                    Console.WriteLine(VykreslovaciAutomat.VykresliChybu2(sprava));

                    if (cestaZalohy != null)
                    {
                        Console.WriteLine();
                        Console.Write(string.Format(Lokalizacia.Hlasky.ZalohaSuboru, Farby.AnsiStyl(Farby.StylTextu.Yellow),
                        cestaZalohy,
                        Farby.AnsiReset2()));
                    }
                }
                catch
                {
                    Console.WriteLine(VykreslovaciAutomat.VykresliChybu2("Error when displaying error! Localisation config file corrupted."));
                }
                
            }
            finally
            {
                if (PisaciAutomat.OperatingSystem.IsWindows())
                {
                    if (_rawMode)
                    {
                        WindowsConsole.VypniRawMode();
                    }
                }

                Environment.Exit(_chyba ? 1 : 0);
            }
        }

        private static void ResetujPozadieKonzoly(ConsoleColor consoleBgColor, ConsoleColor consoleFgColor)
        {
            Console.BackgroundColor = consoleBgColor;
            Console.ForegroundColor = consoleFgColor;
            Console.Clear();
        }

        private static bool OperaciaSEditorom(AkciaSEditorom akcia)
        {
            lock (_lockObject)
            {
                var editor = PisaciAutomat.Program.GetInstance();

                if (akcia.Resize)
                {
                    editor.Resize(akcia.NovaSirkaKonzoly, akcia.NovaVyskaKonzoly);
                    return true;
                }

                return editor.SpracujVstup(akcia.vstup);
            }
        }

        private static void NacitajSubor(string[] args)
        {
            var cesta = string.Empty;
            if (args != null && args.Length > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] != "-dm")
                    {
                        cesta = args[i];
                    }
                }
            }

            var editor = PisaciAutomat.Program.GetInstance();

            editor.NacitajSubor(cesta);
        }

        private static void ResizeListener()
        {
            try
            {
                var sirkaKonzoly = Console.WindowWidth;
                var vyskaKonzoly = Console.WindowHeight;

                var akcia = new AkciaSEditorom()
                {
                    Resize = true,
                    NovaSirkaKonzoly = sirkaKonzoly,
                    NovaVyskaKonzoly = vyskaKonzoly
                };

                OperaciaSEditorom(akcia);

                while (true)
                {
                    PisaciAutomat.Program editor = PisaciAutomat.Program.GetInstance();

                    int currentWidth = Console.WindowWidth;
                    int currentHeight = Console.WindowHeight;

                    // Detect change
                    if (currentWidth != editor.SirkaKonzoly || currentHeight != editor.VyskaKonzoly)
                    {

                        akcia.NovaVyskaKonzoly = currentHeight;
                        akcia.NovaSirkaKonzoly = currentWidth;

                        OperaciaSEditorom(akcia);
                    }

                    Thread.Sleep(100); // Small delay to reduce CPU usage
                }
            }catch(Exception ex)
            {
                ErrorLogger.GetInstance().Log(new Chyba()
                {
                    Ex = ex
                });

                _chyba = true;
            }
        }
    }
}
