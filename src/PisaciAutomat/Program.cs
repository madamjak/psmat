using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciAutomat.Prikazy;
using PisaciStroj;
using PisaciStroj.Lexer;
using PisaciStroj.Lexer.Algoritmy;
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
        private static Program instance = null;
        private static readonly object lockObject = new object();
        private Program() 
        {
            Konstruktor();
        }
        public static Program GetInstance()
        {
            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = new Program();
                }
            }
            return instance;
        }

        //editor
        private IVyhladavac _vyhladavac;
        private static IPisaciStroj _editor;
        private VykreslovaciAutomat _vykreslovaciAutomat;

        //prikazovy riadok
        private PrikazPrePrikazovyRiadok? _commandForCmdLine;
        private PrikazovyAutomat _cmdLineEditor;
        private bool _cmdMode;

        //kurzor
        private NavigovaciPrikaz _navigovaciPrikaz;
        private static ParametreVypisu _parametreVypisu;
        private ParametreVyberu _parametreVyberu;
        private string _skopirovanyText;

        //formatovanie
        private ParametreZapisu _parametreZapisu;

        private string _cestaKSuboru = string.Empty;
        private bool _ukonci;
        private string _hlaska;
        private string _chyba;
        private TypDialogu? _dialog;

        //vyhladavanie
        private ParametreVyhladavania _search;

        private void Konstruktor()
        {
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

        public int SirkaKonzoly => _parametreVypisu.SirkaKonzoly;

        public int VyskaKonzoly => _parametreVypisu.VyskaKonzoly;

        public bool SpracujVstup(ConsoleKeyInfo vstup)
        {
            bool necitaj = false;

            if (_cmdMode)
            {
                CommandLineMode(vstup);
            }
            else if (_dialog.HasValue )
            {
                if (_dialog.Value == TypDialogu.PotvrdUkoncenie)
                {
                    if (vstup.KeyChar == 'a')
                    {
                        _ukonci = true;
                    }
                }

                _dialog = null;
            }
            else if (Navigator.NavigovaciPrikaz(vstup, _navigovaciPrikaz))
            {
                var malVybranyText = Zvyraznovac.MaVybranyText(_parametreVyberu);
                if (!_navigovaciPrikaz.Vyber)
                {
                    _parametreVyberu = new ParametreVyberu();
                }

                var indexRiadok = _parametreVypisu.IndexRiadok;
                var indexStlpec = _parametreVypisu.IndexStlpec;
                var offsetRiadok = _parametreVypisu.OffsetRiadok;
                var offsetStlpec = _parametreVypisu.OffsetStlpec;

                var bracketHighlighted = false;
                if (_parametreVypisu.IndexStlpec < _editor.Riadky()[_parametreVypisu.IndexRiadok].Length())
                {
                    bracketHighlighted = StackBracketMatching.Zatvorky.Contains(_editor.Riadky()[_parametreVypisu.IndexRiadok].CharAt(_parametreVypisu.IndexStlpec));
                }

                Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _editor.Riadky(), _parametreVyberu);

                var zmenaStranky = offsetRiadok != _parametreVypisu.OffsetRiadok || offsetStlpec != _parametreVypisu.OffsetStlpec;                
                var zmenaVyberuTextu = (malVybranyText && !Zvyraznovac.MaVybranyText(_parametreVyberu))
                    || (!malVybranyText && Zvyraznovac.MaVybranyText(_parametreVyberu))
                    || (Zvyraznovac.MaVybranyText(_parametreVyberu) 
                        && (indexRiadok != _parametreVypisu.IndexRiadok 
                        || indexStlpec != _parametreVypisu.IndexStlpec));

                var bracketHighlightedPo = false;
                if (_parametreVypisu.IndexStlpec < _editor.Riadky()[_parametreVypisu.IndexRiadok].Length())
                {
                    bracketHighlightedPo = StackBracketMatching.Zatvorky.Contains(_editor.Riadky()[_parametreVypisu.IndexRiadok].CharAt(_parametreVypisu.IndexStlpec));
                }
                
                var zmenaBracketHighlight = bracketHighlighted != bracketHighlightedPo;
                if (!zmenaStranky && !zmenaVyberuTextu && !zmenaBracketHighlight)
                {
                    necitaj = true;
                }
            }
            else if (vstup.Key == ConsoleKey.Backspace)
            {
                if (Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _editor.ZmazText(_parametreVyberu.Zaciatok.Value.Stlpec, _parametreVyberu.Zaciatok.Value.Riadok,
                    _parametreVyberu.Koniec.Value.Stlpec, _parametreVyberu.Koniec.Value.Riadok, _parametreVypisu);

                    _parametreVyberu = new ParametreVyberu();
                }
                else
                {
                    _editor.ZmazText(_parametreVypisu);
                }
            }
            else if (vstup.Key == ConsoleKey.Delete)
            {
                if (Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _editor.ZmazText(_parametreVyberu.Zaciatok.Value.Stlpec, _parametreVyberu.Zaciatok.Value.Riadok,
                    _parametreVyberu.Koniec.Value.Stlpec, _parametreVyberu.Koniec.Value.Riadok, _parametreVypisu);

                    _parametreVyberu = new ParametreVyberu();
                }
                else
                {
                    Kurzor.PosunKurzorDoprava(_parametreVypisu, _editor.Riadky());
                    _editor.ZmazText(_parametreVypisu);
                }
            }
            else if (vstup.Key == ConsoleKey.Enter)
            {
                var newLine = Environment.NewLine;
                if (_parametreZapisu != null && _parametreZapisu.Okraj > 0)
                {
                    newLine = Environment.NewLine + Indentation.SimpleAutoIndent(_parametreZapisu.Okraj);
                }

                _editor.NapisText(newLine, _parametreVypisu, _parametreZapisu);
            }
            else if ((vstup.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                if (vstup.Key == ConsoleKey.Z)
                {
                    _editor.VratPoslednuOperaciu(_parametreVypisu);
                }
                else if (vstup.Key == ConsoleKey.Y)
                {
                    _editor.ZopakujPoslednuOperaciu(_parametreVypisu);
                }
                else if (vstup.Key == ConsoleKey.C && Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _skopirovanyText = _editor.PrecitajText(
                        _parametreVyberu.Zaciatok.Value.Riadok, _parametreVyberu.Zaciatok.Value.Stlpec,
                        _parametreVyberu.Koniec.Value.Riadok, _parametreVyberu.Koniec.Value.Stlpec);

                    Clipboard.Clipboard.SkopirujDoClipboardu(_skopirovanyText);
                }
                else if (vstup.Key == ConsoleKey.X && Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _skopirovanyText = _editor.PrecitajText(
                        _parametreVyberu.Zaciatok.Value.Riadok, _parametreVyberu.Zaciatok.Value.Stlpec,
                        _parametreVyberu.Koniec.Value.Riadok, _parametreVyberu.Koniec.Value.Stlpec);

                    Clipboard.Clipboard.SkopirujDoClipboardu(_skopirovanyText);

                    _editor.ZmazText(_parametreVyberu.Zaciatok.Value.Stlpec, _parametreVyberu.Zaciatok.Value.Riadok,
                        _parametreVyberu.Koniec.Value.Stlpec, _parametreVyberu.Koniec.Value.Riadok, _parametreVypisu);

                    _parametreVyberu = new ParametreVyberu();
                }
                else if (vstup.Key == ConsoleKey.V)
                {
                    _skopirovanyText = Clipboard.Clipboard.PreciajZClipboardu();

                    if (!string.IsNullOrEmpty(_skopirovanyText))
                    {
                        _editor.NapisText(_skopirovanyText, _parametreVypisu);
                        _parametreVyberu = new ParametreVyberu();
                    }
                }
                else if (vstup.Key == ConsoleKey.F)
                {
                    VyhladajZvyraznenyText();
                }
                else if (vstup.Key == ConsoleKey.A)
                {
                    VyberVsetko();

                }
                else if (vstup.Key == ConsoleKey.S)
                {
                    UlozSubor();
                    necitaj = true;
                }
                else if (vstup.Key == ConsoleKey.Q)
                {
                    if (_editor.MaZmenu())
                    {
                        Hlaska("Neulozene zmeny v subore. Naozaj ukoncit? (a/n)");
                        _dialog = TypDialogu.PotvrdUkoncenie;
                        necitaj = true;
                    }
                    else
                    {
                        _ukonci = true;
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
            }

            var p = new ParametrePrekreslenia()
            {
                Necitaj = necitaj
            };
            Prekresli(p);

            if (_cmdMode)
            {
                _cmdLineEditor.Prekresli();
            }

            if (_ukonci)
            {
                Console.Write(VykreslovaciAutomat.EraseScree() + VykreslovaciAutomat.NastavKurzor(1, 1));
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Resize(int novaSirka, int novaVyska)
        {
            Console.Write(VykreslovaciAutomat.EraseScree());

            var riadok = _parametreVypisu.IndexRiadok;
            var stlpec = _parametreVypisu.IndexStlpec;

            _parametreVypisu.SirkaKonzoly = novaSirka;
            _parametreVypisu.OffsetStlpec = 0;
            _parametreVypisu.Stlpec = 0;

            _parametreVypisu.VyskaKonzoly = novaVyska;
            _parametreVypisu.OffsetRiadok = 0;
            _parametreVypisu.Riadok = 0;

            Kurzor.GoTo(riadok, stlpec, _parametreVypisu, _editor.Riadky());

            var p = new ParametrePrekreslenia()
            {
                Resize = true
            };
            Prekresli(p);

            _cmdLineEditor.Resize(novaSirka);

            if (_cmdMode)
            {
                _cmdLineEditor.Prekresli();
            }
        }

        private void Prekresli(ParametrePrekreslenia p)
        {
            var screen = _vykreslovaciAutomat.Precitaj(_parametreVypisu, _search, _parametreVyberu, _parametreZapisu, p);

            var stavovyRiadok = new StavovyRiadokInfo()
            {
                CestaKSuboru = _cestaKSuboru,
                Stav = string.Format("Ln: {0}  Col: {1}  | Sel: {2} / {3}", _parametreVypisu.IndexRiadok, _parametreVypisu.IndexStlpec,
                    _parametreVyberu.PocetZnakov > 0 ? _parametreVyberu.PocetZnakov.ToString() : "-",
                    _parametreVyberu.PocetRiadkov > 1 ? _parametreVyberu.PocetRiadkov.ToString() : "-"),
                MaZmenu = _editor.MaZmenu(),
            };
            
            _vykreslovaciAutomat.VykresliNaKonzolu(screen, stavovyRiadok, _parametreVypisu, _hlaska, _cmdMode, p);

            if (_dialog.HasValue)
            {
                Console.Write(VykreslovaciAutomat.NastavKurzor(2, _parametreVypisu.OkrajVlavo + 1));
            }

            _hlaska = null;
            _chyba = null;
        }

        private void VyberVsetko()
        {
            _parametreVyberu = new ParametreVyberu();

            _navigovaciPrikaz.Vyber = false;
            _navigovaciPrikaz.Typ = TypNavigacie.ZaciatokTextu;

            Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _editor.Riadky(), _parametreVyberu);

            _navigovaciPrikaz.Vyber = true;
            _navigovaciPrikaz.Typ = TypNavigacie.KoniecTextu;

            Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _editor.Riadky(), _parametreVyberu);
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

        private void CommandLineMode(ConsoleKeyInfo? vstup = null)
        {
            _cmdMode = true;

            var r = _cmdLineEditor.NacitajPrikaz(_commandForCmdLine, vstup);
            _commandForCmdLine = null;

            if (r.ZavriRiadok)
            {
                _cmdMode = false;
                _search.VyhladaneSlovo = null;
                return;
            }
            else if(r.Prikaz != null)
            {
                SpracujPrikaz(r.Prikaz);
            }
        }

        private void SpracujPrikaz(Prikaz prikaz)
        {
            ProcessorPrikazov.SpracujPrikaz(prikaz, _search, _parametreVypisu, _editor, ref _cmdMode);

            Prekresli(new ParametrePrekreslenia());
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
                var cesta = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config/Lex/Jazyk.json");

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

        public bool NacitajSubor(string cesta)
        {
            Console.Write(VykreslovaciAutomat.EraseScree());
            if (string.IsNullOrEmpty(cesta))
            {
                Console.Write(VykreslovaciAutomat.VykresliHlasku("Zadaj prosim nazov alebo cestu k suboru.", _parametreVypisu.OkrajVlavo));
                Console.Write(VykreslovaciAutomat.NastavKurzor(2, _parametreVypisu.OkrajVlavo + 1));
                cesta = Console.ReadLine();
            }

            try
            {
                if (File.Exists(cesta))
                {
                    using (var streamReader = new StreamReader(cesta))
                    {
                        var text = streamReader.ReadToEnd();
                        if (text != null && text.Length > 0)
                        {
                            _editor.NapisTextZoSuboru(text);
                        }
                    }
                }
                else
                {
                    using (var f = File.Create(cesta)) { };
                }

                _cestaKSuboru = Path.GetFullPath(cesta);
                return true;
            }
            catch (Exception ex)
            {
                Console.Write(VykreslovaciAutomat.VykresliChybu(_parametreVypisu.OkrajVlavo));
                return false;
            }
        }

        private void UlozSubor()
        {
            var text = _editor.PrecitajText();

            using (var writer = new StreamWriter(_cestaKSuboru))
            {
                writer.Write(text);
            }
        }
    }
}
