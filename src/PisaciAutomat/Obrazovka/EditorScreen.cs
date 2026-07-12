using System.Collections.Generic;

namespace PisaciStroj.Vypis
{
    public class EditorScreen
    {
        public EditorScreen(int sirka, int vyska)
        {
            Vyska = vyska;
            Sirka = sirka;
            Riadky = new List<string>();

            var x = 1;
            while (x < vyska + 1)
            {
                Riadky.Add(string.Empty);
                x++;
            }
        }

        public int Vyska { get; set; }

        public int Sirka { get; set; }

        public List<string> Riadky { get; set; }

        //pozicia kurzora
        public int Riadok { get; set; }

        public int Stlpec { get; set; }
    }
}