namespace PisaciStroj.Parametre
{
    public class ParametreVypisu
    {
        public int SirkaKonzoly { get; set; }

        public int Sirka 
        {
            get 
            {
                return SirkaKonzoly - OkrajVlavo - OkrajVpravo;
            }
        }

        public int VyskaKonzoly { get; set; }

        public int Vyska
        {
            get
            {
                return VyskaKonzoly - OkrajHore - OkrajDole;
            }
        } 


        public int Riadok { get; set; }

        public int RiadokKurzora { get { return Riadok + OkrajHore; } }

        public int OffsetRiadok { get; set; }

        public int IndexRiadok
        {
            get
            {
                return Riadok + OffsetRiadok;
            }
        }
        

        public int Stlpec { get; set; }

        public int StlpecKurzora { get { return Stlpec + OkrajVlavo; } }

        public int OffsetStlpec { get; set; }

        public int IndexStlpec
        {
            get
            {
                return Stlpec + OffsetStlpec;
            }
        }

        public int OkrajVlavo { get; set; }

        public int OkrajVpravo { get; set; }

        public int OkrajHore { get; set; }

        public int OkrajDole { get; set; }
    }
}
