using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciStroj.Navigacia
{
    public static class Kurzor
    {
        public static void PosunKurzorDole(ParametreVypisu parametreVypisu, List<GapBuffer> riadky)
        {
            if (parametreVypisu.IndexRiadok + 1 < riadky.Count)
            {
                parametreVypisu.Riadok++;
                if (parametreVypisu.Riadok == parametreVypisu.Vyska)
                {
                    parametreVypisu.OffsetRiadok++;
                    parametreVypisu.Riadok--;
                }

                if (parametreVypisu.IndexStlpec > riadky[parametreVypisu.IndexRiadok].Length())
                {
                    NastavIndexStlpca(parametreVypisu, riadky, riadky[parametreVypisu.IndexRiadok].Length());
                }
            }
        }

        public static bool PosunKurzorDoprava(ParametreVypisu parametreVypisu, List<GapBuffer> riadky)
        {
            if (parametreVypisu.IndexStlpec < riadky[parametreVypisu.IndexRiadok].Length())
            {
                parametreVypisu.Stlpec++;
                if (parametreVypisu.Stlpec == parametreVypisu.Sirka)
                {
                    parametreVypisu.OffsetStlpec++;
                    parametreVypisu.Stlpec--;
                }

                return true;
            }
            else if(parametreVypisu.IndexRiadok + 1 < riadky.Count)
            {
                parametreVypisu.Riadok++;
                parametreVypisu.Stlpec = 0;
                parametreVypisu.OffsetStlpec = 0;
                if (parametreVypisu.Riadok == parametreVypisu.Vyska)
                {
                    parametreVypisu.OffsetRiadok++;
                    parametreVypisu.Riadok--;
                }

                return true;
            }

            return false;
        }

        public static void PosunKurzorHore(ParametreVypisu parametreVypisu, List<GapBuffer> riadky)
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

                    if(!(parametreVypisu.IndexRiadok < riadky.Count))
                    {
                        NastavIndexRiadku(parametreVypisu, riadky, riadky.Count - 1);
                    }
                }

                if(parametreVypisu.IndexRiadok < riadky.Count)
                {
                    if (parametreVypisu.IndexStlpec > riadky[parametreVypisu.IndexRiadok].Length())
                    {
                        NastavIndexStlpca(parametreVypisu, riadky, riadky[parametreVypisu.IndexRiadok].Length());
                    }
                }
            }
        }

        public static bool PosunKurzorDolava(ParametreVypisu parametreVypisu, List<GapBuffer> riadky)
        {
            if (parametreVypisu.IndexStlpec > 0)
            {
                parametreVypisu.Stlpec--;
                if (parametreVypisu.Stlpec < 0)
                {
                    parametreVypisu.OffsetStlpec += parametreVypisu.Stlpec;
                    parametreVypisu.Stlpec = 0;

                    //riesenie nizsie prekresli pri posune dolava celu obrazovku
                    //parametreVypisu.OffsetStlpec -= parametreVypisu.Sirka;

                    //if (parametreVypisu.OffsetStlpec < 0)
                    //{
                    //    parametreVypisu.OffsetStlpec = 0;
                    //}

                    //parametreVypisu.Stlpec = parametreVypisu.Sirka - 1;


                    //if (parametreVypisu.IndexStlpec > riadky[parametreVypisu.IndexRiadok].Length())
                    //{
                    //    NastavIndexStlpca(parametreVypisu, riadky, riadky[parametreVypisu.IndexRiadok].Length() - 1);
                    //}
                }

                return true;
            }
            else if(parametreVypisu.IndexRiadok > 0)
            {
                PosunKurzorHore(parametreVypisu, riadky);

                NastavIndexStlpca(parametreVypisu, riadky, riadky[parametreVypisu.IndexRiadok].Length());

                return true;
            }

            return false;
        }

        private static void NastavIndexStlpca(ParametreVypisu parametreVypisu, List<GapBuffer> riadky, int pozadovanyIndex)
        {
            while (true)
            {
                if (parametreVypisu.IndexStlpec == pozadovanyIndex)
                {
                    break;
                }
                if (parametreVypisu.IndexStlpec > pozadovanyIndex)
                {
                    PosunKurzorDolava(parametreVypisu, riadky);
                }
                if (parametreVypisu.IndexStlpec < pozadovanyIndex)
                {
                    PosunKurzorDoprava(parametreVypisu, riadky);
                }
            }
        }

        private static void NastavIndexRiadku(ParametreVypisu parametreVypisu, List<GapBuffer> riadky, int pozadovanyIndex)
        {
            while (true)
            {
                if (parametreVypisu.IndexRiadok == pozadovanyIndex)
                {
                    break;
                }
                if (parametreVypisu.IndexRiadok < pozadovanyIndex)
                {
                    PosunKurzorDole(parametreVypisu, riadky);
                }
                if (parametreVypisu.IndexRiadok > pozadovanyIndex)
                {
                    PosunKurzorHore(parametreVypisu, riadky);
                }
            }
        }

        public static void GoTo(int zaciatocnyRiadok, int zaciatocnyStlpec, ParametreVypisu parametreVypisu, List<GapBuffer> riadky)
        {
            NastavIndexRiadku(parametreVypisu, riadky, zaciatocnyRiadok);
            NastavIndexStlpca(parametreVypisu, riadky, zaciatocnyStlpec);
        }
    }
}
