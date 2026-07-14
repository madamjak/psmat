using PisaciStroj.Parametre;
using PisaciStroj.Vypis;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Obrazovka
{
    public class StavovyRiadokInfo
    {
        public bool MaZmenu { get; set; }
        public string Stav { get; set; }
        public string CestaKSuboru { get; set; }
    }

    public class StavovyRiadok
    {
        private static StavovyRiadokInfo _aktualnyStav;

        public string Vykresli(bool resize, StavovyRiadokInfo stavovyRiadok, ParametreVypisu parametre)
        {
            var sb = new StringBuilder();
            if (_aktualnyStav == null || resize)
            {
                sb.Append(VykresliStavovyRiadok(stavovyRiadok, parametre));
                _aktualnyStav = stavovyRiadok;
            }
            else
            {
                if (_aktualnyStav.CestaKSuboru != stavovyRiadok.CestaKSuboru)
                {
                    sb.Append(VykresliStavovyRiadok(stavovyRiadok, parametre));
                }
                else
                {
                    sb.Append(PrekresliStavovyRiadok(parametre, stavovyRiadok, false));
                }

                _aktualnyStav = stavovyRiadok;
            }

            return sb.ToString();
        }

        private static string PrekresliStavovyRiadok(ParametreVypisu parametre, StavovyRiadokInfo stavovyRiadok, bool resize)
        {
            var sb = new StringBuilder();
            var okrajVpravo = 5;

            if (resize || _aktualnyStav.Stav != stavovyRiadok.Stav || _aktualnyStav.MaZmenu != stavovyRiadok.MaZmenu)
            {
                var dlzkaStavu = stavovyRiadok.Stav.Length;
                var maxDlzkaStavu = parametre.SirkaKonzoly - stavovyRiadok.CestaKSuboru.Length - parametre.OkrajVlavo - okrajVpravo;
                var dostatocnaSirka = dlzkaStavu <= maxDlzkaStavu;

                //pridaj medzeru (okraj - 3), lebo 3 pre info o zmene suboru
                if (!dostatocnaSirka)
                {
                    stavovyRiadok.Stav = string.Format("{0}{1}{2}", stavovyRiadok.Stav.Substring(0, maxDlzkaStavu - 3), "...", "  ");
                }
                else
                {
                    stavovyRiadok.Stav = string.Format("{0}{1}", stavovyRiadok.Stav, "  ");
                }

                sb.Append(StylovaciAutomat.AnsiStyl(StylovaciAutomat.FarbaPozadia.Biela));

                //zaciatok vypisu info
                var s = parametre.SirkaKonzoly - stavovyRiadok.Stav.Length - 3;

                if (resize)
                {
                    var medzera = s - stavovyRiadok.CestaKSuboru.Length - parametre.OkrajVlavo;
                    if (medzera > 0)
                    {
                        sb.Append(NastavPozadie(medzera));
                    }
                }
                else
                {
                    sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.VyskaKonzoly, s));
                    sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
                }

                if (!resize && _aktualnyStav != null)
                {
                    var r = (parametre.SirkaKonzoly - okrajVpravo - _aktualnyStav.Stav.Length - 3)
                    - (parametre.SirkaKonzoly - okrajVpravo - dlzkaStavu - 3);
                    if (r > 0)
                    {
                        sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.VyskaKonzoly, s - r));
                        sb.Append(NastavPozadie(r));
                    }
                }

                sb.Append(stavovyRiadok.Stav);

                var farba = stavovyRiadok.MaZmenu ? StylovaciAutomat.FarbaPozadia.Cervena : StylovaciAutomat.FarbaPozadia.Zelena;
                sb.Append(StylovaciAutomat.AnsiStyl(farba));
                sb.Append(NastavPozadie(3));
                sb.Append(StylovaciAutomat.AnsiReset());
            }

            return sb.ToString();
        }

        private static string VykresliStavovyRiadok(StavovyRiadokInfo stavovyRiadok, ParametreVypisu parametre)
        {
            var sb = new StringBuilder();
            var okrajVpravo = 5;

            var maxDlzkaNazvu = (int)(parametre.SirkaKonzoly * 0.7) - okrajVpravo;
            sb.Append(VykreslovaciAutomat.NastavKurzor(parametre.VyskaKonzoly, 1));
            sb.Append(VykreslovaciAutomat.ZmazOdKurzoraPoKoniecRiadku());
            sb.Append(StylovaciAutomat.AnsiStyl(StylovaciAutomat.FarbaPozadia.Biela));
            sb.Append(NastavPozadie(parametre.OkrajVlavo));

            var dlzkaNazvu = stavovyRiadok.CestaKSuboru.Length;
            var dostatocnaSirka = dlzkaNazvu <= maxDlzkaNazvu;
            if (!dostatocnaSirka)
            {
                stavovyRiadok.CestaKSuboru = string.Format("{0}{1}{2}", stavovyRiadok.CestaKSuboru.Substring(0, maxDlzkaNazvu - 3), "...", "     ");//okrajvpravo
            }
            else
            {
                stavovyRiadok.CestaKSuboru = string.Format("{0}{1}", stavovyRiadok.CestaKSuboru, "     ");//okrajvpravo
            }

            sb.Append(stavovyRiadok.CestaKSuboru);
            sb.Append(StylovaciAutomat.AnsiReset());

            //stav    
            sb.Append(PrekresliStavovyRiadok(parametre, stavovyRiadok, true));

            return sb.ToString();
        }

        private static string NastavPozadie(int sirka)
        {
            var i = 0;
            var sb = new StringBuilder();
            while (true)
            {
                if (i == sirka)
                {
                    break;
                }
                sb.Append(" ");
                i++;
            }
            return sb.ToString();
        }
    }
}
