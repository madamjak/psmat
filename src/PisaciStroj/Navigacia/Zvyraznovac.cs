using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System.Collections.Generic;

namespace PisaciStroj.Navigacia
{
    public static class Zvyraznovac
    {
        public static bool MaVybranyText(ParametreVyberu parametre)
        {
            return parametre.Zaciatok.HasValue && parametre.Koniec.HasValue;
        }

        public static bool MaVybranyTextPreMultiLineOkraj(ParametreVyberu parametreVyberu)
        {
            return MaVybranyText(parametreVyberu) && !(parametreVyberu.Zaciatok.Value.Riadok == parametreVyberu.Koniec.Value.Riadok);
        }

        public static VyhladaneSlovo? ZvyraznenyText(ParametreVyberu parametere, int indexRiadku, int dlzkaRiadku)
        {
            if (!(indexRiadku >= parametere.Zaciatok.Value.Riadok
                && indexRiadku <= parametere.Koniec.Value.Riadok))
            {
                return null;
            }

            var zaciatok = 0;
            var dlzka = 0;
            if (indexRiadku == parametere.Zaciatok.Value.Riadok)
            {
                zaciatok = parametere.Zaciatok.Value.Stlpec;

                if (parametere.Zaciatok.Value.Riadok == parametere.Koniec.Value.Riadok)
                {
                    dlzka = parametere.Koniec.Value.Stlpec - parametere.Zaciatok.Value.Stlpec;
                }
                else
                {
                    dlzka = dlzkaRiadku - parametere.Zaciatok.Value.Stlpec;
                }
            }
            else if(indexRiadku == parametere.Koniec.Value.Riadok)
            {
                dlzka = parametere.Koniec.Value.Stlpec;
            }
            else
            {
                dlzka = dlzkaRiadku;
            }

            return new VyhladaneSlovo()
            {
                Riadok = indexRiadku,
                Pozicia = zaciatok,
                Dlzka = dlzka
            };
        }

        internal static void UpravVyber(Pozicia posPred, Pozicia posPo, ParametreVyberu parametreVyberu)
        {
            if(!parametreVyberu.Zaciatok.HasValue && !parametreVyberu.Koniec.HasValue)
            {
                if(posPred.CompareTo(posPo) > 0)
                {
                    parametreVyberu.Zaciatok = posPo;
                    parametreVyberu.Koniec = posPred;
                }
                else
                {
                    parametreVyberu.Zaciatok = posPred;
                    parametreVyberu.Koniec = posPo;
                }

                return;
            }

            //posun doprava a naspat
            if(posPred.CompareTo(parametreVyberu.Koniec.Value) == 0)
            {
                parametreVyberu.Koniec = posPo;
            }

            //posun dolava a naspat
            if (posPred.CompareTo(parametreVyberu.Zaciatok.Value) == 0)
            {
                parametreVyberu.Zaciatok = posPo;
            }

            if(parametreVyberu.Zaciatok.Value.CompareTo(parametreVyberu.Koniec.Value) == 0)
            {
                parametreVyberu.Zaciatok = null;
                parametreVyberu.Koniec = null;
            }
        }

        public static void SpocitajVyber(ParametreVyberu parametreVyberu, List<GapBuffer> riadky)
        {
            parametreVyberu.PocetRiadkov = 0;
            parametreVyberu.PocetZnakov = 0;

            for (int i = parametreVyberu.Zaciatok.Value.Riadok; i <= parametreVyberu.Koniec.Value.Riadok; i++)
            {
                parametreVyberu.PocetRiadkov++;

                if (parametreVyberu.Zaciatok.Value.Riadok == parametreVyberu.Koniec.Value.Riadok)
                {
                    parametreVyberu.PocetZnakov += parametreVyberu.Koniec.Value.Stlpec - parametreVyberu.Zaciatok.Value.Stlpec;
                    continue;
                }

                if(i == parametreVyberu.Zaciatok.Value.Riadok)
                {
                    parametreVyberu.PocetZnakov += riadky[i].Length() - parametreVyberu.Zaciatok.Value.Stlpec;
                    continue;
                }

                if (i == parametreVyberu.Koniec.Value.Riadok)
                {
                    parametreVyberu.PocetZnakov += parametreVyberu.Koniec.Value.Stlpec;
                    continue;
                }

                parametreVyberu.PocetZnakov += riadky[i].Length();
            }
        }

        public static void PosunVyberDoprava(ParametreVyberu p, int pocetStlpcov)
        {
            p.Zaciatok = p.Zaciatok.Value.PosunDoprava(pocetStlpcov);
            p.Koniec = p.Koniec.Value.PosunDoprava(pocetStlpcov);
        }

        internal static void PosunVyberDolava(ParametreVyberu p, int v)
        {
            p.Zaciatok = p.Zaciatok.Value.PosunDolava(v);
            p.Koniec = p.Koniec.Value.PosunDolava(v);
        }
    }
}
