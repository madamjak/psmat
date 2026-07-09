using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciAutomat.Prikazy;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using PisaciStroj.Vypis;
using System;
using System.Collections.Generic;
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

    //pridane dodatocne pri tvorbe textu prace
    public interface IPisaciStroj
    {
        List<GapBuffer> Riadky();
        void ZmazText(ParametreVypisu parametreVypisu);
        void NapisText(string vstup, ParametreVypisu parametreVypisu);
        void VratPoslednuOperaciu(ParametreVypisu parametreVypisu);
        void ZopakujPoslednuOperaciu(ParametreVypisu parametreVypisu);
        string PrecitajText(int zaciatocnyRiadok, int zaciatocnyStlpec, int konecnyRiadok, int konecnyStlpec);
        void ZmazText(int zaciatocnyStlpecVyberu, int zaciatocnyRiadokVyberu, int konecnyStlpecVyberu, int konecnyRiadokVyberu, ParametreVypisu parametreVypisu);
    }

    public class Program
    {
        private IVyhladavac _vyhladavac;
        private PisaciStroj.Program _editor;
        private VykreslovaciAutomat _vykreslovaciAutomat;
        
        private PrikazovyAutomat _cmdLineEditor;
        private bool _cmdMode;

        //kurzor
        private NavigovaciPrikaz _navigovaciPrikaz;
        private ParametreVypisu _parametreVypisu;

        public bool MaZmenuVSubore { get; private set; }
        public bool Ukonci { get; private set; }

        private string _cestaKSuboru;

        private ParametreVyberu _parametreVyberu;
        private string _skopirovanyText;

        private string _hlaska;
        private string _chyba;
        private TypDialogu? _dialog;

        //vyhladavanie
        private ParametreVyhladavania _search;

        public Program(string cestaKSuboru)
        {
            _cestaKSuboru = cestaKSuboru;

            _vyhladavac = new VyhladavaciAutomat();
            _editor = new PisaciStroj.Program(_vyhladavac);
            _vykreslovaciAutomat = new VykreslovaciAutomat(NacitajLexGramatiku(), _editor, _vyhladavac);
            _cmdLineEditor = new PrikazovyAutomat();

            _navigovaciPrikaz = new NavigovaciPrikaz();
            _parametreVyberu = new ParametreVyberu();
            _parametreVypisu = new ParametreVypisu() 
            {
                OkrajVlavo = 5,
                OkrajHore = 2,
                OkrajDole = 2
            };

            _search = new ParametreVyhladavania();
        }

        public void Prekresli()
        {
            var screen = _vykreslovaciAutomat.Precitaj(_parametreVypisu, _search);

            var kurzor = string.Format("Riadok: {0} Stlpec: {1}",
                _parametreVypisu.IndexRiadok, _parametreVypisu.IndexStlpec);

            var vyberTextu = string.Format("ZR: {0} ZS: {1} KR: {2} KS: {3}",
                _parametreVyberu.ZaciatocnyRiadok.HasValue ? _parametreVyberu.ZaciatocnyRiadok.ToString() : "-",
                _parametreVyberu.ZaciatocnyStlpec.HasValue ? _parametreVyberu.ZaciatocnyStlpec.ToString() : "-",
                _parametreVyberu.KonecnyRiadok.HasValue ? _parametreVyberu.KonecnyRiadok.ToString() : "-",
                _parametreVyberu.KonecnyStlpec.HasValue ? _parametreVyberu.KonecnyStlpec.ToString() : "-");

            var subor = string.Format("{0}{1}", _cestaKSuboru, MaZmenuVSubore ? "*" : "");

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

            TypNavigacie? typNavigacie = null;
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
                Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _editor.Riadky());
            }
            else if (vstup.Key == ConsoleKey.Backspace)
            {
                _editor.ZmazText(_parametreVypisu);
                MaZmenuVSubore = true;
            }
            else if (vstup.Key == ConsoleKey.Delete)
            {
                Kurzor.PosunKurzorDoprava(_parametreVypisu, _editor.Riadky());
                _editor.ZmazText(_parametreVypisu);
                MaZmenuVSubore = true;
            }
            else if (vstup.Key == ConsoleKey.Enter)
            {
                _editor.NapisText(Environment.NewLine, _parametreVypisu);
                MaZmenuVSubore = true;
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
                else if (vstup.Key == ConsoleKey.O && SpravneKoniecTextu())
                {
                    _parametreVyberu.ZaciatocnyRiadok = _parametreVypisu.IndexRiadok;
                    _parametreVyberu.ZaciatocnyStlpec = _parametreVypisu.IndexStlpec;
                }
                else if (vstup.Key == ConsoleKey.P && SpravneZaciatokTextu())
                {
                    _parametreVyberu.KonecnyRiadok = _parametreVypisu.IndexRiadok;
                    _parametreVyberu.KonecnyStlpec = _parametreVypisu.IndexStlpec;
                }
                else if (vstup.Key == ConsoleKey.K && MaVybranyText())
                {
                    _skopirovanyText = _editor.PrecitajText(
                        _parametreVyberu.ZaciatocnyRiadok.Value, _parametreVyberu.ZaciatocnyStlpec.Value,
                        _parametreVyberu.KonecnyRiadok.Value, _parametreVyberu.KonecnyStlpec.Value);
                }
                else if (vstup.Key == ConsoleKey.M && MaVybranyText())
                {
                    _skopirovanyText = _editor.PrecitajText(
                        _parametreVyberu.ZaciatocnyRiadok.Value, _parametreVyberu.ZaciatocnyStlpec.Value,
                        _parametreVyberu.KonecnyRiadok.Value, _parametreVyberu.KonecnyStlpec.Value);

                    _editor.ZmazText(_parametreVyberu.ZaciatocnyStlpec.Value, _parametreVyberu.ZaciatocnyRiadok.Value,
                        _parametreVyberu.KonecnyStlpec.Value, _parametreVyberu.KonecnyRiadok.Value, _parametreVypisu);

                    _parametreVyberu = new ParametreVyberu();

                    MaZmenuVSubore = true;
                }
                else if (vstup.Key == ConsoleKey.L && !string.IsNullOrEmpty(_skopirovanyText))
                {
                    _editor.NapisText(_skopirovanyText, _parametreVypisu);
                    _parametreVyberu = new ParametreVyberu();
                    MaZmenuVSubore = true;
                }
                else if (vstup.Key == ConsoleKey.H && MaZmenuVSubore)
                {
                    UlozSubor();
                    MaZmenuVSubore = false;
                }
                else if (vstup.Key == ConsoleKey.Q)
                {
                    if (MaZmenuVSubore)
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
                MaZmenuVSubore = true;
            }
        }

        private void CommandLineMode()
        {
            _cmdMode = true;

            var r = _cmdLineEditor.NacitajPrikaz();

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
            if (prikaz.VyhladavanyText != null && prikaz.VyhladavanyText != _search.VyhladavanyText)
            {
                _search.VyhladavanyText = prikaz.VyhladavanyText;
                
                _vyhladavac.NastavVyhladavaciAutomat(_search.VyhladavanyText);
            }

            if (prikaz.Typ == TypPrikazu.VyhladajReset)
            {
                _search = new ParametreVyhladavania();
                _cmdMode = false;
            }

            if (prikaz.Typ == TypPrikazu.Vyhladaj)
            {
                _search.VyhladaneSlovo = null;
                _cmdMode = false;
            }

            if (prikaz.Typ == TypPrikazu.VyhladajDalsi)
            {
                if (_search.VyhladaneSlovo.HasValue)
                {
                    Kurzor.PosunKurzorDoprava(_parametreVypisu, _editor.Riadky());
                }

                var s = _editor.Vyhladaj(prikaz.VyhladavanyText, _parametreVypisu);
                if (s.HasValue)
                {
                    _search.VyhladaneSlovo = s.Value;
                    Kurzor.GoTo(s.Value.Riadok, s.Value.Pozicia, _parametreVypisu, _editor.Riadky());
                }
                
                if(_search.VyhladaneSlovo.HasValue && !s.HasValue)
                {
                    Kurzor.PosunKurzorDolava(_parametreVypisu, _editor.Riadky());
                }
            }

            if (prikaz.Typ == TypPrikazu.VyhladajNahrad)
            {
                _search.VyhladaneSlovo = null;
                if (_editor.VyhladajANahrad(prikaz.VyhladavanyText, prikaz.NovyText, _parametreVypisu)) 
                {
                    MaZmenuVSubore = true;
                };
            }

            if (prikaz.Typ == TypPrikazu.VyhladajNahradVsetky)
            {
                _search.VyhladaneSlovo = null;
                _search.VyhladavanyText = null;
                var aktualnyR = _parametreVypisu.IndexRiadok;
                var aktualnyS = _parametreVypisu.IndexStlpec;
                
                if (_editor.VyhladajANahradVsetky(prikaz.VyhladavanyText, prikaz.NovyText, _parametreVypisu))
                {
                    MaZmenuVSubore = true;
                };

                Kurzor.GoTo(aktualnyR, aktualnyS, _parametreVypisu, _editor.Riadky());
                _cmdMode = false;
            }

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
        static bool IsPrintable(char c)
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

        private bool MaVybranyText()
        {
            return _parametreVyberu.ZaciatocnyRiadok.HasValue && _parametreVyberu.ZaciatocnyStlpec.HasValue
                && _parametreVyberu.KonecnyRiadok.HasValue && _parametreVyberu.KonecnyStlpec.HasValue;
        }

        private bool SpravneZaciatokTextu()
        {
            var konecnyRiadok = _parametreVypisu.IndexRiadok;
            var konecnyStlpec = _parametreVypisu.IndexStlpec;

            var zaciatokNevybraty = !(_parametreVyberu.ZaciatocnyRiadok.HasValue && _parametreVyberu.ZaciatocnyStlpec.HasValue);

            return zaciatokNevybraty ||
                (_parametreVyberu.ZaciatocnyRiadok == konecnyRiadok && _parametreVyberu.ZaciatocnyStlpec < konecnyStlpec) ||
                (_parametreVyberu.ZaciatocnyRiadok < konecnyRiadok);
        }

        private bool SpravneKoniecTextu()
        {
            var zaciatocnyRiadok = _parametreVypisu.IndexRiadok;
            var zaciatocnyStlpec = _parametreVypisu.IndexStlpec;

            var koniecNevybraty = !(_parametreVyberu.KonecnyRiadok.HasValue && _parametreVyberu.KonecnyStlpec.HasValue);

            return koniecNevybraty ||
                (_parametreVyberu.KonecnyRiadok == zaciatocnyRiadok && _parametreVyberu.KonecnyStlpec > zaciatocnyStlpec) ||
                (_parametreVyberu.KonecnyRiadok > zaciatocnyRiadok);
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
