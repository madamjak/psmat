namespace PisaciStroj.Pamat
{
    internal enum TypOperacie
    {
        Zapis,
        Mazanie,
        VyhladajNahrad,
        VyhladajNahradVsetky
    }

    internal class Operacia
    {
        public int ZaciatocnyStlpec { get; set; }

        public int ZaciatocnyRiadok { get; set; }

        public int KonecnyStlpec { get; set; }

        public int KonecnyRiadok { get; set; }

        public TypOperacie Typ { get; set; }

        public string ZmazanaCastTextu { get; set; }

        public int PocetOperacii { get; set; }

        public string VyhladavanyText { get; set; }

        public string NovyText { get; set; }
    }
}
