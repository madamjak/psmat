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
            try
            {
                NacitajAleboVytvorSubor(args);

                if (PisaciAutomat.OperatingSystem.IsWindows())
                {
                    WindowsConsole.NastavRawMode();
                    _rawMode = true;
                    Console.TreatControlCAsInput = true;
                }

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
            }
            catch (Exception ex)
            {
                _chyba = true;

                Console.Write(VykreslovaciAutomat.EraseScreen());
                
                var logger = ErrorLogger.GetInstance();
                logger.Log(new Chyba() { Ex = ex });
                var cesta = logger.UlozDoSuboru();
                var sprava =
                    string.Format("Neocakavana chyba, mozne nahlasit na {0}{1}{2} a pridat zaznam ulozeny v subore {3}{4}{5}",
                    StylovaciAutomat.AnsiStyl(StylovaciAutomat.StylTextu.Cyan), 
                    "https://github.com/madamjak/psmat/issues/", 
                    StylovaciAutomat.AnsiReset(),
                    StylovaciAutomat.AnsiStyl(StylovaciAutomat.StylTextu.Yellow), 
                    cesta,
                    StylovaciAutomat.AnsiReset());
                Console.Write(VykreslovaciAutomat.VykresliChybu2(sprava));
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

        private static void NacitajAleboVytvorSubor(string[] args)
        {
            var cestaKSuboru = args != null && args.Length == 1 ? args[0] : null;

            var editor = PisaciAutomat.Program.GetInstance();

            var success = editor.NacitajSubor(cestaKSuboru);
            if (!success)
            {
                while (true)
                {
                    success = editor.NacitajSubor(null);
                    if (success)
                    {
                        break;
                    }
                }
            }
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
