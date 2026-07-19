using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciAutomat.Prikazy;
using PisaciAutomat.Subory;
using PisaciStroj;
using PisaciStroj.Chyby;
using PisaciStroj.Formatovanie;
using PisaciStroj.Lexer;
using PisaciStroj.Lexer.Algoritmy;
using PisaciStroj.Navigacia;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace PisaciAutomat
{
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

        private static Dictionary<string, LexGramatika> _jazyky;

        //editor
        private static IPisaciStroj _editor;
        private VykreslovaciAutomat _vykreslovaciAutomat;

        //prikazovy riadok
        private PrikazPrePrikazovyRiadok? _commandForCmdLine;
        private PrikazovyAutomat _cmdLineEditor;
        private bool _cmdMode;

        //syntax highlight
        private ILexer _lexer;

        //kurzor
        private NavigovaciPrikaz _navigovaciPrikaz;
        private static ParametreVypisu _parametreVypisu;
        private ParametreVyberu _parametreVyberu;
        private string _skopirovanyText;

        //vyhladavanie
        private IVyhladavac _vyhladavac;
        private ParametreVyhladavania _search;

        //...
        private string _cestaKSuboru = string.Empty;
        private bool _ukonci;
        
        //info a dialog
        private Hlaska? _hlaska;
        private string _dialog;
        private TypDialogu? _typDialogu;

        private const int _okrajHore = 2;
        private void Konstruktor()
        {
            NacitajLexGramatiku();

            _vyhladavac = new VyhladavaciAutomat();
            _editor = new PisaciStroj.Program(_vyhladavac);
            _lexer = new LexAutomat();
            _vykreslovaciAutomat = new VykreslovaciAutomat(_lexer, _editor, _vyhladavac);
            _cmdLineEditor = new PrikazovyAutomat();

            _navigovaciPrikaz = new NavigovaciPrikaz();
            _parametreVyberu = new ParametreVyberu();
            _parametreVypisu = new ParametreVypisu() 
            {
                OkrajHore = _okrajHore,
                OkrajDole = 2
            };

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
            else if (_typDialogu.HasValue)
            {
                if (_typDialogu == TypDialogu.PotvrdUkoncenie)
                {
                    if (vstup.KeyChar == 'a')
                    {
                        _ukonci = true;
                    }
                }

                _typDialogu = null;
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
            else if (vstup.Key == ConsoleKey.Tab)
            {
                if ((vstup.Modifiers & ConsoleModifiers.Shift) == ConsoleModifiers.Shift)
                {
                    var maVybrany = Zvyraznovac.MaVybranyText(_parametreVyberu);
                    if (!maVybrany || !Zvyraznovac.MaVybranyTextPreMultiLineOkraj(_parametreVyberu))
                    {
                        _editor.ZmazOkraj(_parametreVypisu, _parametreVyberu);
                    }
                    else
                    {
                        _editor.ZmazMultiLineOkraj(_parametreVypisu, _parametreVyberu);
                    }
                }
                else
                {
                    var maVybrany = Zvyraznovac.MaVybranyText(_parametreVyberu);
                    if (!maVybrany || !Zvyraznovac.MaVybranyTextPreMultiLineOkraj(_parametreVyberu))
                    {
                        if (maVybrany)
                        {
                            _editor.ZmazText(_parametreVyberu.Zaciatok.Value.Stlpec, _parametreVyberu.Zaciatok.Value.Riadok,
                                _parametreVyberu.Koniec.Value.Stlpec, _parametreVyberu.Koniec.Value.Riadok, _parametreVypisu);

                            _parametreVyberu = new ParametreVyberu();
                        }
                        _editor.PridajOkraj(_parametreVypisu);
                    }
                    else
                    {
                        _editor.PridajMultiLineOkraj(_parametreVypisu, _parametreVyberu);
                    }
                }
            }
            else if (vstup.Key == ConsoleKey.Enter)
            {
                if (Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _editor.ZmazText(_parametreVyberu.Zaciatok.Value.Stlpec, _parametreVyberu.Zaciatok.Value.Riadok,
                    _parametreVyberu.Koniec.Value.Stlpec, _parametreVyberu.Koniec.Value.Riadok, _parametreVypisu);

                    _parametreVyberu = new ParametreVyberu();
                }

                _editor.NapisText(Environment.NewLine, _parametreVypisu);
            }
            else if ((vstup.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                if (vstup.Key == ConsoleKey.Z)
                {
                    _editor.VratPoslednuOperaciu(_parametreVypisu);
                    _parametreVyberu = new ParametreVyberu();
                }
                else if (vstup.Key == ConsoleKey.Y)
                {
                    _editor.ZopakujPoslednuOperaciu(_parametreVypisu);
                    _parametreVyberu = new ParametreVyberu();
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
                    _parametreVyberu = new ParametreVyberu();
                    Zvyraznovac.VyberVsetko(_parametreVyberu, _parametreVypisu, _editor.Riadky());
                }
                else if (vstup.Key == ConsoleKey.S)
                {
                    UlozSubor();
                    necitaj = true;
                }
                else if (vstup.Key == ConsoleKey.N)
                {
                    UlozSuborAko();
                    necitaj = true;
                }
                else if (vstup.Key == ConsoleKey.Q)
                {
                    UkonciAplikaciu();
                }
                else if (vstup.Key == ConsoleKey.W)
                {
                    CommandLineMode();
                }
            }
            else if (IsPrintable(vstup.KeyChar))
            {
                if (Zvyraznovac.MaVybranyText(_parametreVyberu))
                {
                    _editor.ZmazText(_parametreVyberu.Zaciatok.Value.Stlpec, _parametreVyberu.Zaciatok.Value.Riadok,
                    _parametreVyberu.Koniec.Value.Stlpec, _parametreVyberu.Koniec.Value.Riadok, _parametreVypisu);

                    _parametreVyberu = new ParametreVyberu();
                }

                _editor.NapisZnak(vstup.KeyChar, _parametreVypisu);
            }

            if (_ukonci)
            {
                ErrorLogger.GetInstance().UlozDoSuboru();
                Console.Write(VykreslovaciAutomat.EraseScreen() + VykreslovaciAutomat.NastavKurzor(1, 1));
                return false;
            }

            var p = new ParametrePrekreslenia()
            {
                Necitaj = necitaj,
            };
            Prekresli(p);

            return true;
        }

        public void Resize(int novaSirka, int novaVyska)
        {
            Console.Write(VykreslovaciAutomat.EraseScreen());

            var riadok = _parametreVypisu.IndexRiadok;
            var stlpec = _parametreVypisu.IndexStlpec;

            _parametreVypisu.SirkaKonzoly = novaSirka;
            _parametreVypisu.OffsetStlpec = 0;
            _parametreVypisu.Stlpec = 0;

            _parametreVypisu.VyskaKonzoly = novaVyska;
            _parametreVypisu.OffsetRiadok = 0;
            _parametreVypisu.Riadok = 0;

            Kurzor.GoTo(riadok, stlpec, _parametreVypisu, _editor.Riadky());

            _cmdLineEditor.Resize(novaSirka);

            var p = new ParametrePrekreslenia()
            {
                Resize = true
            };
            Prekresli(p);
        }

        private void Prekresli(ParametrePrekreslenia p)
        {
            if(_parametreVypisu.OkrajVlavo == 0)
            {
                _parametreVypisu.OkrajVlavo = _editor.Riadky().Count.ToString().Length + 2;
                if(_parametreVypisu.OkrajVlavo == 3)
                {
                    _parametreVypisu.OkrajVlavo = 5;
                }
            }

            var sb = new StringBuilder();

            sb.Append(VykreslovaciAutomat.NastavKurzorUnVisible());

            p.OkrajHore = _okrajHore;
            if (_cmdMode)
            {
                p.OkrajVlavo = _parametreVypisu.OkrajVlavo;
                _cmdLineEditor.Prekresli(p, sb, _editor.Riadky());
            }

            _parametreVypisu.OkrajHore = p.OkrajHore;

            var screen = _vykreslovaciAutomat.Precitaj(_parametreVypisu, _search, _parametreVyberu, p);

            var stavovyRiadok = new StavovyRiadokInfo()
            {
                CestaKSuboru = _cestaKSuboru,
                Stav = string.Format("Ln: {0}  Col: {1}  | Sel: {2} / {3}", _parametreVypisu.IndexRiadok, _parametreVypisu.IndexStlpec,
                    _parametreVyberu.PocetZnakov > 0 ? _parametreVyberu.PocetZnakov.ToString() : "-",
                    _parametreVyberu.PocetRiadkov > 1 ? _parametreVyberu.PocetRiadkov.ToString() : "-"),
                MaZmenu = _editor.MaZmenu(),
            };

            _vykreslovaciAutomat.VykresliNaKonzolu(screen, stavovyRiadok, _parametreVypisu, _hlaska, _dialog, _cmdMode, p, sb);

            if (_typDialogu.HasValue)
            {
                sb.Append(VykreslovaciAutomat.NastavKurzor(2, _parametreVypisu.OkrajVlavo + 1));
            }

            _hlaska = null;
            _dialog = null;

            if (!_cmdMode)
            {
                sb.Append(VykreslovaciAutomat.NastavKurzor(screen.Riadok, screen.Stlpec));
                sb.Append(VykreslovaciAutomat.NastavKurzorVisible());
            }
            else
            {
                _cmdLineEditor.NastavKurzor(sb);
            }

            Console.Write(sb.ToString());
        }

        private void UlozSuborAko()
        {
            _commandForCmdLine = new PrikazPrePrikazovyRiadok()
            {
                UlozSuborAko = true,
                ExistujucaCesta = _cestaKSuboru
            };

            CommandLineMode();
        }

        private void VyhladajZvyraznenyText()
        {
            if (!Zvyraznovac.MaVybranyText(_parametreVyberu))
            {
                if (_parametreVypisu.IndexStlpec < _editor.Riadky()[_parametreVypisu.IndexRiadok].Length() - 1)
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

            if (r.Prikaz != null)
            {
                SpracujPrikaz(r);
            }
            else if (r.Hlaska != null)
            {
                _hlaska = new Hlaska() 
                {
                    Typ = TypHlasky.Info,
                    Sprava = r.Hlaska
                };
            }
            else if (r.Dialog != null)
            {
                _hlaska = new Hlaska()
                {
                    Typ = TypHlasky.Dialog,
                    Sprava = r.Dialog
                };
            }
            else if(r.ZavriRiadok)
            {
                _search = new ParametreVyhladavania();
                _cmdMode = false;
            }

            if (r.Ukonci)
            {
                _cmdMode = false;
                UkonciAplikaciu();
            }
        }

        private void SpracujPrikaz(PrikazovyAutomatResult r)
        {
            var pr = ProcesorPrikazov.SpracujPrikaz(r.Prikaz, _search, _parametreVypisu, _editor, _vyhladavac);
            if (r.ZavriRiadok)
            {
                _cmdMode = false;
            }

            if (!pr.Success)
            {
                _hlaska = new Hlaska()
                {
                    Typ = TypHlasky.Chyba,
                    Sprava = pr.Hlaska
                };

                return;
            }
            else if (!string.IsNullOrWhiteSpace(pr.Hlaska))
            {
                _hlaska = new Hlaska()
                {
                    Typ = TypHlasky.Info,
                    Sprava = pr.Hlaska
                };
            }

            if (r.Prikaz.Typ == TypPrikazu.UlozAko && pr.Success)
            {
                var staraPripona = Path.GetExtension(_cestaKSuboru);
                _cestaKSuboru = r.Prikaz.NovyText;

                var novaPripona = Path.GetExtension(_cestaKSuboru);

                if(staraPripona != novaPripona)
                {
                    NastavLex();
                }
            }

            if(r.Prikaz.Typ == TypPrikazu.Vyhladaj && pr.Success)
            {
                _commandForCmdLine = new PrikazPrePrikazovyRiadok()
                {
                    ZobrazVysledky = true,
                    Vysledky = _search.VyhladaneSlova
                };
                CommandLineMode();
            }

            if(r.Prikaz.Typ == TypPrikazu.Vyhladaj && pr.Success)
            {
                _commandForCmdLine = new PrikazPrePrikazovyRiadok()
                {
                    ZobrazVysledky = true,
                    Vysledky = _search.VyhladaneSlova
                };
                CommandLineMode();
            }
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

        private void NacitajLexGramatiku()
        {
            try
            {
                var cesta = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Config/Lex/Jazyk.json");

                KonfiguraciaJazyka konfig;

                using (var file = File.Open(cesta, FileMode.Open))
                {
                    using (var reader = new StreamReader(file))
                    {
                        var s = reader.ReadToEnd();

                        konfig = (KonfiguraciaJazyka)JsonConvert.DeserializeObject(s, typeof(KonfiguraciaJazyka));
                    }
                }

                if(konfig.Jazyky == null || konfig.Jazyky.Length == 0)
                {
                    return;
                }

                _jazyky = new Dictionary<string, LexGramatika>(StringComparer.OrdinalIgnoreCase);
                foreach(var jazyk in konfig.Jazyky)
                {
                    _jazyky.Add(jazyk.Pripona, jazyk);
                }
            }
            catch(Exception ex)
            {
                ErrorLogger.GetInstance().Log(new Chyba()
                {
                    Ex = ex
                });
            }
        }

        private void NastavLex()
        {
            var pripona = Path.GetExtension(_cestaKSuboru);

            LexGramatika g;
            if (_jazyky.TryGetValue(pripona, out g))
            {
                try
                {
                    _lexer.NastavLexer(g);
                }
                catch (Exception ex)
                {
                    ErrorLogger.GetInstance().Log(new Chyba()
                    {
                        Ex = ex
                    });
                }
            }
        }

        public void NacitajSubor(string cesta)
        {
            if (string.IsNullOrWhiteSpace(cesta))
            {
                _cestaKSuboru = ".";
                return;
            }

            if (!File.Exists(cesta) && !Directory.Exists(cesta))
            {
                using (var f = File.Create(cesta)) { };
                _cestaKSuboru = Path.GetFullPath(cesta);
                return;
            }

            if (Validacia.IsTextFile(cesta))
            {
                using (var streamReader = new StreamReader(cesta))
                {
                    var text = streamReader.ReadToEnd();
                    if (text != null && text.Length > 0)
                    {
                        _editor.NapisTextZoSuboru(text);
                    }
                }

                _cestaKSuboru = Path.GetFullPath(cesta);

                NastavLex();
                return;
            }
            else
            {
                throw new ApplicationException(string.Format("{0} is not a text file", cesta));
            }
        }

        private void UlozSubor()
        {
            if(string.IsNullOrEmpty(_cestaKSuboru) || _cestaKSuboru == ".")
            {
                UlozSuborAko();
            }
            else
            {
                SpracujPrikaz(new PrikazovyAutomatResult()
                {
                    Prikaz = new Prikaz()
                    {
                        Typ = TypPrikazu.UlozAko,
                        NovyText = _cestaKSuboru
                    }
                });
            }
        }


        private void UkonciAplikaciu()
        {
            if (_editor.MaZmenu())
            {
                _typDialogu = TypDialogu.PotvrdUkoncenie;
                _dialog = "Neulozene zmeny v subore. Naozaj ukoncit? (a/n)";
            }
            else
            {
                _ukonci = true;
            }
        }

        private enum TypDialogu
        {
            PotvrdUkoncenie
        }
    }

    
}
