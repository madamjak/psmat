using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System;
using System.IO;
using System.Text;

namespace PisaciAutomat.Prikazy
{
    public class PrikazovyAutomat
    {
        private GapBuffer _riadok;
        private ILexer _lexer;
        private ParametreVypisu _parametreVypisu;

        private string _hlaska;
        private string _chyba;

        public PrikazovyAutomat()
        {
            _lexer = new LexAutomat(NacitajLexGramatiku());
            _riadok = new GapBuffer();
            _parametreVypisu = new ParametreVypisu()
            {
                OkrajVlavo = 5
            };
        }

        public PrikazovyAutomatResult NacitajPrikaz()
        {
            _parametreVypisu.SirkaKonzoly = Console.BufferWidth;

            Prekresli();

            PrikazovyAutomatResult r = null;

            while (true)
            {
                var vstup = Console.ReadKey();

                r = SpracujVstup(vstup);

                Prekresli();

                if(r.Prikaz != null || r.ZavriRiadok)
                {
                    break;
                }
            }

            return r;
        }

        public PrikazovyAutomatResult SpracujVstup(ConsoleKeyInfo vstup)
        {
            var r = new PrikazovyAutomatResult();

            if(_parametreVypisu.SirkaKonzoly == 0 && _parametreVypisu.Stlpec == 0 && _parametreVypisu.OffsetStlpec == 0)
            {
                _parametreVypisu.SirkaKonzoly = Console.BufferWidth;
            }

            if (ZmenaRozmerovKonzoly())
            {
                Hlaska();
            }
            else if (vstup.Key == ConsoleKey.RightArrow)
            {
                if (_parametreVypisu.IndexStlpec < _riadok.Length())
                {
                    PosunDoprava();
                }

            }
            else if (vstup.Key == ConsoleKey.LeftArrow)
            {
                if (_parametreVypisu.IndexStlpec > 0)
                {
                    PosunDolava();
                }

            }
            else if (vstup.Key == ConsoleKey.Enter)
            {
                var prikaz = MapPrikaz();

                if(prikaz == null)
                {
                    Chyba();
                }
                else
                {
                    r.Prikaz = prikaz;
                }
            }
            else if (vstup.Key == ConsoleKey.Backspace)
            {
                if (_parametreVypisu.IndexStlpec > 0)
                {
                    PosunDolava();

                    _riadok.Delete(_parametreVypisu.IndexStlpec);
                }
            }
            else if (JeVytlacitelnyAsciiZnak(vstup.KeyChar))
            {
                _riadok.Insert(vstup.KeyChar, _parametreVypisu.IndexStlpec);

                PosunDoprava();
            }
            else if (vstup.Key == ConsoleKey.Escape)
            {
                r.ZavriRiadok = true;
            }
            else
            {
                Chyba();
            }

            return r;
        }

        private bool ZmenaRozmerovKonzoly()
        {
            if (_parametreVypisu.SirkaKonzoly != Console.BufferWidth)
            {
                _parametreVypisu.SirkaKonzoly = Console.BufferWidth;
                _parametreVypisu.Stlpec = 0;
                _parametreVypisu.OffsetStlpec = 0;

                return true;
            }

            return false;
        }

        private void PosunDoprava()
        {
            _parametreVypisu.Stlpec++;
            if (_parametreVypisu.Stlpec == _parametreVypisu.Sirka)
            {
                _parametreVypisu.OffsetStlpec++;
                _parametreVypisu.Stlpec--;

            }
        }

        private void PosunDolava()
        {
            _parametreVypisu.Stlpec--;
            if (_parametreVypisu.Stlpec < 0)
            {
                _parametreVypisu.OffsetStlpec -= _parametreVypisu.Sirka;
                if (_parametreVypisu.OffsetStlpec < 0)
                {
                    _parametreVypisu.OffsetStlpec = 0;
                }

                _parametreVypisu.Stlpec = _parametreVypisu.Sirka - 1;

                if (_parametreVypisu.Stlpec > _riadok.Length() - 1)
                {
                    _parametreVypisu.Stlpec = _riadok.Length() - 1;
                }
            }
        }

        public void Prekresli()
        {
            var sb = new StringBuilder();

            if (_hlaska != null)
            {
                sb.Append(_hlaska);
                _hlaska = null;
            }

            if (_chyba != null)
            {
                sb.Append(_chyba);
                _chyba = null;
            }

            sb.Append(VykreslovaciAutomat.NastavKurzor(1, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(VykreslovaciAutomat.CislaRiadkov("   > "));

            if(_riadok.Length() > 0)
            {
                var tokeny = _lexer.Lex(_riadok);
                sb.Append(StylovaciAutomat.SyntaxHighligt(tokeny, _riadok, _parametreVypisu.OffsetStlpec, _parametreVypisu.Sirka));
            }

            sb.Append(VykreslovaciAutomat.NastavKurzor(1, _parametreVypisu.StlpecKurzora + 1));

            Console.Write(sb.ToString());
        }

        private void Hlaska()
        {
            var sb = new StringBuilder();
            sb.Append(VykreslovaciAutomat.NastavKurzor(2, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(VykreslovaciAutomat.NastavKurzor(2, _parametreVypisu.OkrajVlavo + 1));
            sb.Append(VykreslovaciAutomat.Hlaska("Zmena rozmerov okna, prosim znova."));

            _hlaska = sb.ToString();
        }

        private void Chyba()
        {
            var sb = new StringBuilder();
            sb.Append(VykreslovaciAutomat.NastavKurzor(2, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(VykreslovaciAutomat.NastavKurzor(2, _parametreVypisu.OkrajVlavo + 1));
            sb.Append(VykreslovaciAutomat.Chyba());

            _chyba = sb.ToString();
        }

        private Prikaz MapPrikaz()
        {
            var p = new Prikaz();
            try
            {
                var parts = _riadok.Read().Split(' ');

                if(parts.Length == 2 && (parts[0] == "find"))
                {
                    p.Typ = TypPrikazu.Vyhladaj;
                    p.VyhladavanyText = parts[1];

                    return p;
                }
                if (parts.Length == 2 && (parts[0] == "next"))
                {
                    p.Typ = TypPrikazu.VyhladajDalsi;
                    p.VyhladavanyText = parts[1];

                    return p;
                }
                if (parts.Length == 1 && parts[0] == "rest")
                {
                    p.Typ = TypPrikazu.VyhladajReset;

                    return p;
                }
                else if (parts.Length == 3 && (parts[0] == "rfirst"))
                {
                    p.Typ = TypPrikazu.VyhladajNahrad;
                    p.VyhladavanyText = parts[1];
                    p.NovyText = parts[2];

                    return p;
                }
                else if (parts.Length == 3 && parts[0] == "rall")
                {
                    p.Typ = TypPrikazu.VyhladajNahradVsetky;
                    p.VyhladavanyText = parts[1];
                    p.NovyText = parts[2];

                    return p;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private bool JeVytlacitelnyAsciiZnak(char keyChar)
        {
            return keyChar >= 32 && keyChar <= 127;
        }

        private LexGramatika NacitajLexGramatiku()
        {
            try
            {
                var cesta = "Config/Lex/Commands.json";

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
            catch (Exception ex)
            {
                return new LexGramatika();
            }
        }
    }
}
