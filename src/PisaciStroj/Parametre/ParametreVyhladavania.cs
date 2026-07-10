using PisaciStroj.Vyhladavanie;
using System.Collections.Generic;

namespace PisaciStroj.Parametre
{
    public struct ParametreVyhladavania
    {
        public string VyhladavanyText { get; set; }

        public VyhladaneSlovo? VyhladaneSlovo { get; set; }

        public Dictionary<int, Dictionary<int, VyhladaneSlovo>> VyhladaneSlova { get; set; }
    }
}
