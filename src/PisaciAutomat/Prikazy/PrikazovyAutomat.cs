using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PisaciAutomat.Prikazy
{
    public struct PrikazPrePrikazovyRiadok
    {
        public string VyhladavanyText { get; set; }
    }

    public class PrikazovyAutomat
    {
        private GapBuffer _riadok;
        private List<GapBuffer> _riadky;
        
        private ILexer _lexer;

        private ParametreVypisu _parametreVypisu;
        private NavigovaciPrikaz _navigovaciPrikaz;
        private ParametreVyberu _vyber;

        private string _hlaska;
        private string _chyba;

        public PrikazovyAutomat()
        {
            _lexer = new LexAutomat(NacitajLexGramatiku());
            _riadok = new GapBuffer();
            _riadky = new List<GapBuffer> { _riadok };
            _parametreVypisu = new ParametreVypisu()
            {
                OkrajVlavo = 5
            };
            _navigovaciPrikaz = new NavigovaciPrikaz();
            _vyber = new ParametreVyberu();
        }

        public PrikazovyAutomatResult NacitajPrikaz(PrikazPrePrikazovyRiadok? prikazZEditora = null)
        {
            _parametreVypisu.SirkaKonzoly = Console.BufferWidth;

            if(prikazZEditora.HasValue && prikazZEditora.Value.VyhladavanyText != null)
            {
                var p = VyhladavaciPrikaz(prikazZEditora);
                Prekresli();
                return p;
            }

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

        private PrikazovyAutomatResult VyhladavaciPrikaz(PrikazPrePrikazovyRiadok? prikazZEditora)
        {
            _riadok.Delete(0, _riadok.Length());
            _parametreVypisu.Stlpec = 0;
            _parametreVypisu.OffsetStlpec = 0;

            var prikaz = "next " + prikazZEditora.Value.VyhladavanyText;
            foreach(char ch in prikaz)
            {
                NapisZnak(ch);
            }

            return new PrikazovyAutomatResult()
            {
                Prikaz = new Prikaz()
                {
                    Typ = TypPrikazu.VyhladajDalsi,
                    VyhladavanyText = prikazZEditora.Value.VyhladavanyText
                }
            };
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
            else if (Navigator.NavigujVPrikazovomRiadku(vstup, _navigovaciPrikaz))
            {
                Navigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _riadky, _vyber);

                if (!_navigovaciPrikaz.Vyber)
                {
                    _vyber = new ParametreVyberu();
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
            else if (PisaciAutomat.Program.IsPrintable(vstup.KeyChar))
            {
                NapisZnak(vstup.KeyChar);
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

        private void NapisZnak(char ch)
        {
            _riadok.Insert(ch, _parametreVypisu.IndexStlpec);

            PosunDoprava();
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

                VyhladaneSlovo? zvyraznenyText = null;
                if (Zvyraznovac.MaVybranyText(_vyber))
                {
                    zvyraznenyText = Zvyraznovac.ZvyraznenyText(_vyber, 0, _riadok.Length());
                }

                sb.Append(StylovaciAutomat.SyntaxHighligt(tokeny, _riadok, _parametreVypisu.OffsetStlpec, _parametreVypisu.Sirka, zvyraznenyText));
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
            return ProcessorPrikazov.NacitajPrikaz(_riadok);
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
