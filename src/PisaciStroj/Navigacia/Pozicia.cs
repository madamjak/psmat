namespace PisaciStroj.Navigacia
{
    public struct Pozicia
    {
        public int Riadok { get; set; }

        public int Stlpec { get; set; }
    }

    public static class PoziciaHelper
    {
        public static int CompareTo(this Pozicia pos, Pozicia pos2)
        {
            if (pos.Riadok == pos2.Riadok)
            {
                if (pos.Stlpec == pos2.Stlpec)
                {
                    return 0;
                }

                if (pos.Stlpec > pos2.Stlpec)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }

            if (pos.Riadok > pos2.Riadok)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }
    }
}
