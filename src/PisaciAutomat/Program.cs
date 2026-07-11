using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciAutomat.Prikazy;
using PisaciStroj;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Globalization;
using System.IO;

namespace PisaciAutomat
{
    public enum TypDialogu
    {
        Ziadny,
        PotvrdUkoncenie,
        Subor
    }

    public class Program
    {
        private IVyhladavac _vyhladavac;
        private IPisaciStroj _editor;
        private VykreslovaciAutomat _vykreslovaciAutomat;

        //prikazovy riadok
        private PrikazPrePrikazovyRiadok? _commandForCmdLine;
        private PrikazovyAutomat _cmdLineEditor;
        private bool _cmdMode;

        //kurzor
        private NavigovaciPrikaz _navigovaciPrikaz;
        private ParametreVypisu _parametreVypisu;

        private bool _maZmenuVSubore;
        public bool Ukonci { get; private set; }

        private string _cestaKSuboru;

        private ParametreVyberu _parametreVyberu;
        private string _skopirovanyText;

        //formatovanie
        private ParametreZapisu _parametreZapisu;

        private string _hlaska;
        private string _chyba;
        private TypDialogu? _dialog;

        //vyhladavanie
        private ParametreVyhladavania _search;

        public Program(string cestaKSuboru)
        {
            _cestaKSuboru = cestaKSuboru;

            _editor = new PisaciStroj.Program(new VyhladavaciAutomat());
            _vykreslovaciAutomat = new VykreslovaciAutomat(NacitajLexGramatiku(), _editor);
            _cmdLineEditor = new PrikazovyAutomat();

            _navigovaciPrikaz = new NavigovaciPrikaz();
            _parametreVyberu = new ParametreVyberu();
            _parametreVypisu = new ParametreVypisu() 
            {
                OkrajVlavo = 5,
                OkrajHore = 2,
                OkrajDole = 2
            };
            _parametreZapisu = new ParametreZapisu();

            _search = new ParametreVyhladavania();
        }

        public void Prekresli()
        {
            var screen = _vykreslovaciAutomat.Precitaj(_parametreVypisu, _search, _parametreVyberu, _parametreZapisu);

            var kurzor = string.Format("Riadok: {0} Stlpec: {1}",
                _parametreVypisu.IndexRiadok, _parametreVypisu.IndexStlpec);

            var vyberTextu = string.Format("Vyber: {0}",
                _parametreVyberu.PocetZnakov.HasValue ? _parametreVyberu.PocetZnakov.ToString() : "-");

            var subor = string.Format("{0}{1}", _cestaKSuboru, _maZmenuVSubore ? "*" : "");

            var stavovyRiadok = string.Format("{0} | {1} | {2}", kurzor, vyberTextu, subor);

            _vykreslovaciAutomat.VykresliNaKonzolu(screen, stavovyRiadok, _parametreVypisu, _hlaska, _cmdMode);

            if (_dialog.HasValue)
            {
                Console.Write(VykreslovaciAutomat.NastavKurzor(2, _parametreVypisu.OkrajVlavo + 1));
            }

            _hlaska = null;
            _chyba = null;
        }

        public void SpracujVstup(ConsoleKeyInfo vstup)
        {
            if (_parametreVypisu.SirkaKonzoly == 0 && _parametreVypisu.Stlpec == 0 && _parametreVypisu.OffsetStlpec == 0)
            {
                _parametreVypisu.SirkaKonzoly = Console.BufferWidth;
                _parametreVypisu.VyskaKonzoly = Console.BufferHeight;
            }

            if (_cmdMode)
            {
                CommandLineMode();
            }
            else if (_dialog.HasValue )
            {
                if (_dialog.Value == TypDialogu.PotvrdUkoncenie)
                {
                    if (vstup.KeyChar == 'a')
                    {
                        Ukonci = true;
                    }
                }

                _dialog = null;
            }
            else if (_parametreVypisu.SirkaKonzoly != Console.BufferWidth || _parametreVypisu.VyskaKonzoly != Console.BufferHeight)
            {
                var riadok = _parametreVypisu.IndexRiadok;
                var stlpec = _parametreVypisu.IndexStlpec;

                if (_parametreVypisu.SirkaKonzoly != Console.BufferWidth)
                {
                    _parametreVypisu.SirkaKonzoly = Console.BufferWidth;
                    _parametreVypisu.OffsetStlpec = 0;
                    _parametreVypisu.Stlpec = 0;
                }

                if (_parametreVypisu.VyskaKonzoly != Console.BufferHeight)
                {
                    _parametreVypisu.VyskaKonzoly = Console.BufferHeight;
                    _parametreVypisu.OffsetRiadok = 0;
                    _parametreVypisu.Riadok = 0;
                }

                Kurzor.GoTo(riadok, stlpec, _parametreVypisu, _editor.Riadky());

                Hlaska("Zmena rozmerov okna, prosim znova.");
            }
            else if (Navigator.NavigovaciPrikaz(vstup, _navigovaciPrikaz))
            {
                if (!_navigovaciPrikaz.Vyber)
                {
                    _parametreVyberu = new ParametreVyberu();
                }

                Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _editor.Riadky(), _parametreVyberu);
            }
            else if (vstup.Key == ConsoleKey.Backspace)
            {
                _editor.ZmazText(_parametreVypisu);
                _maZmenuVSubore = true;
            }
            else if (vstup.Key == ConsoleKey.Delete)
            {
                Kurzor.PosunKurzorDoprava(_parametreVypisu, _editor.Riadky());
                _editor.ZmazText(_parametreVypisu);
                _maZmenuVSubore = true;
            }
            else if (vstup.Key == ConsoleKey.Enter)
            {
                _editor.NapisText(Environment.NewLine, _parametreVypisu, _parametreZapisu);
                _maZmenuVSubore = true;
            }
            else if ((vstup.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                if (vstup.Key == ConsoleKey.U)
                {
                    _editor.VratPoslednuOperaciu(_parametreVypisu);
                }
                else if (vstup.Key == ConsoleKey.R)
                {
                    _editor.ZopakujPoslednuOperaciu(_parametreVypisu);
                }
                else if (vstup.Key == ConsoleKey.K && Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _skopirovanyText = _editor.PrecitajText(
                        _parametreVyberu.Zaciatok.Value.Riadok, _parametreVyberu.Zaciatok.Value.Stlpec,
                        _parametreVyberu.Koniec.Value.Riadok, _parametreVyberu.Koniec.Value.Stlpec);

                    Clipboard.Clipboard.SkopirujDoClipboardu(_skopirovanyText);
                }
                else if (vstup.Key == ConsoleKey.M && Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _skopirovanyText = _editor.PrecitajText(
                        _parametreVyberu.Zaciatok.Value.Riadok, _parametreVyberu.Zaciatok.Value.Stlpec,
                        _parametreVyberu.Koniec.Value.Riadok, _parametreVyberu.Koniec.Value.Stlpec);

                    Clipboard.Clipboard.SkopirujDoClipboardu(_skopirovanyText);

                    _editor.ZmazText(_parametreVyberu.Zaciatok.Value.Stlpec, _parametreVyberu.Zaciatok.Value.Riadok,
                        _parametreVyberu.Koniec.Value.Stlpec, _parametreVyberu.Koniec.Value.Riadok, _parametreVypisu);

                    _parametreVyberu = new ParametreVyberu();

                    _maZmenuVSubore = true;
                }
                else if (vstup.Key == ConsoleKey.L)
                {
                    _skopirovanyText = Clipboard.Clipboard.PreciajZClipboardu();

                    if (!string.IsNullOrEmpty(_skopirovanyText))
                    {
                        _editor.NapisText(_skopirovanyText, _parametreVypisu);
                        _parametreVyberu = new ParametreVyberu();
                        _maZmenuVSubore = true;
                    }
                }
                else if (vstup.Key == ConsoleKey.F)
                {
                    VyhladajZvyraznenyText();
                }
                else if (vstup.Key == ConsoleKey.H && _maZmenuVSubore)
                {
                    UlozSubor();
                    _maZmenuVSubore = false;
                }
                else if (vstup.Key == ConsoleKey.Q)
                {
                    if (_maZmenuVSubore)
                    {
                        Hlaska("Neulozene zmeny v subore. Naozaj ukoncit? (a/n)");
                        _dialog = TypDialogu.PotvrdUkoncenie;
                    }
                    else
                    {
                        Ukonci = true;
                    }
                }
                else if (vstup.Key == ConsoleKey.W)
                {
                    CommandLineMode();
                }
            }
            else if (IsPrintable(vstup.KeyChar))
            {
                _editor.NapisZnak(vstup.KeyChar, _parametreVypisu);
                _maZmenuVSubore = true;
            }
        }

        private void VyhladajZvyraznenyText()
        {
            if (!Zvyraznovac.MaVybranyText(_parametreVyberu))
            {
                if (_parametreVypisu.IndexRiadok < _editor.Riadky()[_parametreVypisu.IndexRiadok].Length() - 1)
                {
                    _navigovaciPrikaz.Vyber = false;
                    _navigovaciPrikaz.Typ = TypNavigacie.SlovoDoprava;

                    Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _editor.Riadky(), _parametreVyberu);

                    _navigovaciPrikaz.Vyber = true;
                    _navigovaciPrikaz.Typ = TypNavigacie.SlovoDolava;
                    _parametreVyberu = new ParametreVyberu();

                    Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _editor.Riadky(), _parametreVyberu);
                }
            }

            if (Zvyraznovac.MaVybranyText(_parametreVyberu))
            {
                if(_parametreVyberu.Zaciatok.Value.Riadok == _parametreVyberu.Koniec.Value.Riadok)
                {
                    var zvyrazneneSlovo = Zvyraznovac.ZvyraznenyText(_parametreVyberu, _parametreVypisu.IndexRiadok, _editor.Riadky()[_parametreVypisu.IndexRiadok].Length());
                    var text = _editor.Riadky()[_parametreVypisu.IndexRiadok].Read(zvyrazneneSlovo.Value.Pozicia, zvyrazneneSlovo.Value.Dlzka);

                    _commandForCmdLine = new PrikazPrePrikazovyRiadok()
                    {
                        VyhladavanyText = text
                    };

                    Kurzor.GoTo(zvyrazneneSlovo.Value.Riadok, zvyrazneneSlovo.Value.Pozicia, _parametreVypisu, _editor.Riadky());

                    _parametreVyberu = new ParametreVyberu();

                    CommandLineMode();
                }
            }
        }

        private void CommandLineMode()
        {
            _cmdMode = true;

            var r = _cmdLineEditor.NacitajPrikaz(_commandForCmdLine);

            _commandForCmdLine = null;

            if (r.ZavriRiadok)
            {
                _cmdMode = false;
                _search.VyhladaneSlovo = null;
                return;
            }
            else
            {
                SpracujPrikaz(r.Prikaz);

                if (_cmdMode)
                {
                    CommandLineMode();
                }
            }
        }

        private void SpracujPrikaz(Prikaz prikaz)
        {
            ProcessorPrikazov.SpracujPrikaz(prikaz, _search, _parametreVypisu, _editor, ref _cmdMode, ref _maZmenuVSubore);

            Prekresli();
        }

        private void Hlaska(string hlaska)
        {
            _hlaska = VykreslovaciAutomat.Hlaska(hlaska);
        }

        private bool JeVytlacitelnyAsciiZnak(char keyChar)
        {
            return keyChar >= 32 && keyChar <= 127;
        }

        /// <summary>
        /// Checks if a Unicode character is printable.
        /// Printable means: not control, not format, not surrogate, not private use, not unassigned.
        /// zdroj: Copilot search
        /// </summary>
        public static bool IsPrintable(char c)
        {
            if(c == '\t')
            {
                return true;
            }

            // Exclude control characters
            if (char.IsControl(c))
                return false;

            // Get Unicode category
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

            // Exclude non-printable categories
            switch (category)
            {
                case UnicodeCategory.Control:
                case UnicodeCategory.Format:
                case UnicodeCategory.Surrogate:
                case UnicodeCategory.PrivateUse:
                case UnicodeCategory.OtherNotAssigned:
                    return false;
                default:
                    return true;
            }
        }

        private LexGramatika NacitajLexGramatiku()
        {
            try
            {
                var cesta = "Config/Lex/Jazyk.json";

                LexGramatika gramatika;

                using (var file = File.Open(cesta, FileMode.Open))
                {
                    using (var reader = new StreamReader(file))
                    {
                        var s = reader.ReadToEnd();

                        gramatika = (LexGramatika)JsonConvert.DeserializeObject(s, typeof(LexGramatika));
                    }
                }

                return gramatika;
            }
            catch(Exception ex)
            {
                return new LexGramatika();
            }
            
        }

        public void NacitajSuborAVykresli()
        {
            Console.Write(VykreslovaciAutomat.EraseScree());
            if (_cestaKSuboru == null)
            {
                Console.Write(VykreslovaciAutomat.VykresliHlasku("Zadaj prosim nazov alebo cestu k suboru.", _parametreVypisu.OkrajVlavo));
                Console.Write(VykreslovaciAutomat.NastavKurzor(2, _parametreVypisu.OkrajVlavo + 1));
                _cestaKSuboru = Console.ReadLine();
            }

            if (File.Exists(_cestaKSuboru))
            {
                using (var streamReader = new StreamReader(_cestaKSuboru))
                {
                    var text = streamReader.ReadToEnd();
                    if (text != null && text.Length > 0)
                    {
                        _editor.NapisTextZoSuboru(text);
                    }
                }
            }

            _parametreVypisu.SirkaKonzoly = Console.BufferWidth;
            _parametreVypisu.VyskaKonzoly = Console.BufferHeight;
            Prekresli();
        }

        private void UlozSubor()
        {
            if (!File.Exists(_cestaKSuboru))
            {
                using (var f = File.Create(_cestaKSuboru)) { };
            }

            var text = _editor.PrecitajText();

            using (var writer = new StreamWriter(_cestaKSuboru))
            {
                writer.Write(text);
            }
        }
    }
}
