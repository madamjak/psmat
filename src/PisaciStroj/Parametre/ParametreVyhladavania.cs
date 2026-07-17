using PisaciStroj.Navigacia;
using PisaciStroj.Vyhladavanie;
using System.Collections.Generic;

namespace PisaciStroj.Parametre
{
    public enum TypVyhladavania
    {
        Vsetky,
        Dalsi
    }

    public class ParametreVyhladavania
    {
        public Pozicia? ZaciatokVyhladavania { get; set; }

        public string VyhladavanyText { get; set; }

        public VyhladaneSlovo? VyhladaneSlovo { get; set; }
        
        public bool Obratene { get; set; }

        public Dictionary<int, Dictionary<int, VyhladaneSlovo>> VyhladaneSlova { get; set; }

        public TypVyhladavania? Typ { get; set; }
    }
}
