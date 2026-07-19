using PisaciStroj.Navigacia;
using PisaciStroj.Parametre;
using System;

namespace PisaciAutomat.Prikazy.Vysledky
{
    public static class VysledkovyNavigator
    {
        public static void Naviguj(NavigovaciPrikaz prikaz, ParametreVypisu parametreVypisu, int pocetVysledkov)
        {
            switch (prikaz.Typ)
            {
                case TypNavigacie.Hore:

                    PosunKurzorHore(parametreVypisu);
                    break;

                case TypNavigacie.Dole:

                    PosunKurzorDole(parametreVypisu, pocetVysledkov);
                    break;

                case TypNavigacie.ZaciatokRiadku:
                case TypNavigacie.ZaciatokTextu:

                    GoTo(0, parametreVypisu, pocetVysledkov);
                    break;

                case TypNavigacie.KonecRiadku:
                case TypNavigacie.KoniecTextu:

                    GoTo(pocetVysledkov - 1, parametreVypisu, pocetVysledkov);
                    break;

                case TypNavigacie.DalsiaStranka:

                    //teoreticky by to slo lepsie nejak cez offset
                    var koniecDalsejStranky = Math.Min(parametreVypisu.IndexRiadok + (parametreVypisu.Vyska - 1) * 2, pocetVysledkov - 1);
                    GoTo(koniecDalsejStranky, parametreVypisu, pocetVysledkov);

                    var zaciatokDalsejStranky = Math.Max(parametreVypisu.IndexRiadok - parametreVypisu.Vyska + 1, 0);

                    GoTo(zaciatokDalsejStranky, parametreVypisu, pocetVysledkov);
                    break;

                case TypNavigacie.PredoslaStranka:

                    var predIndexRiadku = Math.Max(parametreVypisu.IndexRiadok - parametreVypisu.Vyska, 0);
                    GoTo(predIndexRiadku, parametreVypisu, pocetVysledkov);
                    break;
            }
        }

        private static void GoTo(int v, ParametreVypisu parametreVypisu, int pocetVysledkov)
        {
            while (true)
            {
                if (parametreVypisu.IndexRiadok == v)
                {
                    break;
                }
                if (parametreVypisu.IndexRiadok < v)
                {
                    PosunKurzorDole(parametreVypisu, pocetVysledkov);
                }
                if (parametreVypisu.IndexRiadok > v)
                {
                    PosunKurzorHore(parametreVypisu);
                }
            }
        }

        private static void PosunKurzorDole(ParametreVypisu parametreVypisu, int pocetVysledkov)
        {
            if (parametreVypisu.IndexRiadok + 1 < pocetVysledkov)
            {
                parametreVypisu.Riadok++;
                if (parametreVypisu.Riadok == parametreVypisu.Vyska)
                {
                    parametreVypisu.OffsetRiadok++;
                    parametreVypisu.Riadok--;
                }
            }
        }

        private static void PosunKurzorHore(ParametreVypisu parametreVypisu)
        {
            if (parametreVypisu.IndexRiadok > 0)
            {
                parametreVypisu.Riadok--;
                if (parametreVypisu.Riadok < 0)
                {
                    parametreVypisu.OffsetRiadok -= parametreVypisu.Vyska;
                    if (parametreVypisu.OffsetRiadok < 0)
                    {
                        parametreVypisu.OffsetRiadok = 0;
                    }

                    parametreVypisu.Riadok = parametreVypisu.Vyska - 1;
                }
            }
        }
    }
}
