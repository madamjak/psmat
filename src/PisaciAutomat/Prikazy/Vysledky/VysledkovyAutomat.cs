using PisaciAutomat.Config;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;
using static PisaciStroj.Vyhladavanie.VyhladavaciAutomat;

namespace PisaciAutomat.Prikazy.Vysledky
{
    public class VysledkovyAutomatResult
    {
        public bool ZavriVysledky { get; set; }

        public PrikazPrePrikazovyRiadok? Prikaz { get; set; }
        public bool Ukonci { get; internal set; }
    }

    public class VysledkovyAutomat
    {
        private const int MaxPocetVysledkov = 8;

        private List<VyhladaneSlovo> _riadky;
        private ParametreVypisu _parametreVypisu;
        private NavigovaciPrikaz _navigovaciPrikaz;

        public VysledkovyAutomat()
        {
            _parametreVypisu = new ParametreVypisu()
            {
                OkrajHore = 1 //pre prikazovy riadok
            };

            _navigovaciPrikaz = new NavigovaciPrikaz();
        }

        public void NastavVysledkovyAutomat(Dictionary<int, Dictionary<int, VyhladaneSlovo>> vyhladaneSlova)
        {
            _riadky = new List<VyhladaneSlovo>();
            foreach(var riadok in vyhladaneSlova)
            {
                foreach(var slovo in riadok.Value)
                {
                    _riadky.Add(slovo.Value);
                }
            }
        }

        public VysledkovyAutomatResult SpracujVstup(ConsoleKeyInfo vstup)
        {
            var r = new VysledkovyAutomatResult();

            if (Navigator.NavigujVoVysledkoch(vstup, _navigovaciPrikaz))
            {
                VysledkovyNavigator.Naviguj(_navigovaciPrikaz, _parametreVypisu, _riadky.Count);
            }
            else if (vstup.Key == ConsoleKey.Escape)
            {
                _riadky = null;
                r.ZavriVysledky = true;
            }
            else if ((vstup.Modifiers & ConsoleModifiers.Control) == ConsoleModifiers.Control)
            {
                if (vstup.Key == ConsoleKey.Q)
                {
                    _riadky = null;
                    r.ZavriVysledky = true;
                    r.Ukonci = true;
                }
            } else if (vstup.Key == ConsoleKey.Enter)
            {
                r = SpracujPrikaz();
            }


            return r;
        }

        private VysledkovyAutomatResult SpracujPrikaz()
        {
            var slovo = _riadky[_parametreVypisu.IndexRiadok];
            return new VysledkovyAutomatResult()
            {
                Prikaz = new PrikazPrePrikazovyRiadok()
                {
                    GoTo = slovo
                }
            };
        }

        public void Resize(int novaSirka)
        {
            _parametreVypisu.SirkaKonzoly = novaSirka;
        }

        public void Prekresli(ParametrePrekreslenia p, StringBuilder sb, List<GapBuffer> riadkyEditora)
        {
            _parametreVypisu.OkrajVlavo = p.OkrajVlavo;

            VykreslovaciAutomat.VykresliInfoHlasku(_parametreVypisu, new Hlaska() 
            {
                Typ = TypHlasky.Info,
                Sprava = string.Format("Najdenych {0} vysledkov", _riadky.Count)
            }, sb);

            var pocetRiadkov = 0;
            var riadokObrazovky = 0; //teoreticky sa inspirovat EditorScreen a neprekreslovat vsetko
            var tokeny = new Dictionary<int, Token>();
            for (int i = _parametreVypisu.OffsetRiadok; i < _riadky.Count; i++)
            {
                if (pocetRiadkov == MaxPocetVysledkov)
                {
                    break;
                }

                var pozadie = Farby.FarbaPrikazRiadku();
                var farbaVybraneho = Farby.FarbaVysledkov();
                if (_parametreVypisu.IndexRiadok == i)
                {
                    pozadie = Farby.FarbaVysledkov();
                    farbaVybraneho = Farby.FarbaPrikazRiadku();
                }

                sb.Append(Farby.AnsiReset());
                sb.Append(VykreslovaciAutomat.NastavKurzor(p.OkrajHore + 1 + pocetRiadkov, 1));
                sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());

                if (_parametreVypisu.IndexRiadok == i)
                {
                    sb.Append(VykreslovaciAutomat.NastavPozadie(p.OkrajVlavo - 2));
                    sb.Append(Farby.AnsiStyl(Farby.FarbaIndikatoraPrikazRiadku()));
                    sb.Append("> ");
                }
                else
                {
                    sb.Append(VykreslovaciAutomat.NastavPozadie(p.OkrajVlavo));
                }

                VyhladaneSlovo zvyraznenyText = _riadky[i];
                var riadokEditora = riadkyEditora[zvyraznenyText.Riadok];

                var zaciatokRiadku = Math.Max(0, zvyraznenyText.Pozicia - _parametreVypisu.Sirka / 2);
                sb.Append(StylovaciAutomat.SyntaxHighligt(tokeny, riadokEditora,
                    zaciatokRiadku,
                    _parametreVypisu.Sirka,
                    zvyraznenyText, pozadie, farbaVybraneho));

                var dlzkaVykresleneho = Math.Min(riadokEditora.Length() - zaciatokRiadku, _parametreVypisu.Sirka);
                if (dlzkaVykresleneho < _parametreVypisu.Sirka)
                {
                    sb.Append(Farby.AnsiStyl(pozadie));
                    sb.Append(VykreslovaciAutomat.NastavPozadie(_parametreVypisu.Sirka - dlzkaVykresleneho));
                    sb.Append(Farby.AnsiReset());
                }

                pocetRiadkov++;
            }

            //2 riadky pre cmd riadok a info hlasku 
            _parametreVypisu.OkrajHore = 2;
            _parametreVypisu.VyskaKonzoly = pocetRiadkov + 2;

            sb.Append(VykreslovaciAutomat.NastavKurzor(p.OkrajHore + 1 + pocetRiadkov, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
            //medzera pred textom
            p.OkrajHore += pocetRiadkov + 1;
        }

        internal void ZmazInfoHlasku(StringBuilder sb)
        {
            VykreslovaciAutomat.ZmazHlasku(sb);
        }
    }
}
