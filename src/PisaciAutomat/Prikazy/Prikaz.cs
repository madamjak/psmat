using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Prikazy
{
    public class PrikazovyAutomatResult
    {
        public Prikaz Prikaz { get; set; }

        public bool ZavriRiadok { get; set; }
    }

    public enum TypPrikazu
    {
        Vyhladaj,
        VyhladajReset,
        VyhladajDalsi,
        VyhladajPredosly,
        VyhladajNahrad,
        VyhladajNahradVsetky
    }

    public class Prikaz
    {
        public TypPrikazu Typ { get; set; }

        public string VyhladavanyText { get; set; }

        public string NovyText { get; set; }
        public bool ZavriRiadok { get; internal set; }
    }
}
