using PisaciAutomat.Config.Locale;
using PisaciStroj;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Prikazy.Formatovanie
{
    public static class ProcesorPrikazov
    { 
        public static HashSet<TypPrikazu> TypyVyhladavacihPrikazov = new HashSet<TypPrikazu>()
            { TypPrikazu.ZalomRiadky };

        public static ProcessorPrikazovResult SpracujPrikaz(Prikaz prikaz,
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor)
        {
            return SpracujPrikazInternal(prikaz, parametreVypisu, editor);

        }

        private static ProcessorPrikazovResult SpracujPrikazInternal(Prikaz prikaz, ParametreVypisu parametreVypisu, IPisaciStroj editor)
        {
            var r = new ProcessorPrikazovResult();

            if (prikaz.Typ == TypPrikazu.ZalomRiadky)
            {
                var pocet = editor.ZalomRiadky(parametreVypisu);

                r.Success = true;
                r.Hlaska = string.Format(Lokalizacia.Hlasky.PocetUprav, pocet);
                return r;
            }


            return r;
        }
    }
}
