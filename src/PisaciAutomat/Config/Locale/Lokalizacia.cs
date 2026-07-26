namespace PisaciAutomat.Config.Locale
{
    public class Lokalizacia
    {
        public static Hlasky Hlasky { get; private set; }

        public static void NastavHlasky(Hlasky? h)
        {
            if (h.HasValue)
            {
                Hlasky = h.Value;
            }
            else
            {
                Hlasky = Default();
            }
        }

        private static Hlasky Default()
        {
            return new Hlasky()
            {
                Ano = "a",
                PotvrdUkoncenie = "Neulozene zmeny v subore. Naozaj ukoncit? (a/n)",
                NeocakavanaChyba = "Neocakavana chyba, mozne nahlasit na {0}{1}{2} a pridat zaznam ulozeny v subore {3}{4}{5}",
                ZalohaSuboru = "Zaloha rozpracovanej prace ulozena v subore {0}{1}{2}",
                NeznamaChyba = "Neocakavana chyba",
                
                CisloRiadkuAStlpca = "Zadaj cislo riadku a stlpca.",
                CestaKSuboru = "Zadaj cestu k suboru.",
                
                PocetVysledkov = "Najdenych {0} vysledkov",
                KoniecVysledkov = "Koniec vysledkov alebo ziadne vysledky.",
                PocetUprav = "{0} uprav.",

                ValidnaCesta = "Zadaj validnu cestu.",
                SuborExistuje = "Subor existuje, prepisat? (a/n)",
                UspesneUlozeny = "Uspesne ulozeny",
                ChybaPriUkladani = "Chyba pri ukladani",

                NeexistujucaPozicia = "Neexistujuca pozicia"
            };
        }
    }
}
