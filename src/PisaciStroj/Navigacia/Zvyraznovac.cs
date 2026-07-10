using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;

namespace PisaciStroj.Navigacia
{
    public static class Zvyraznovac
    {
        public static bool MaVybranyText(ParametreVyberu parametre)
        {
            return parametre.Zaciatok.HasValue && parametre.Koniec.HasValue;
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

            //if(parametere.Zaciatok.Value.Riadok == parametere.Koniec.Value.Riadok)
            //{
            //    parametere.PocetZnakov = dlzka;
            //}
            //else
            //{
            //    parametere.PocetZnakov = parametere.PocetZnakov.HasValue ? parametere.PocetZnakov + dlzka : dlzka;
            //}

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
                parametreVyberu.PocetZnakov = null;
            }
        }
    }
}
