using PisaciStroj;
using PisaciStroj.Chyby;
using PisaciStroj.Navigacia;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Prikazy.Navigacia
{
    public static class ProcesorPrikazov
    {
        public static HashSet<TypPrikazu> TypyVyhladavacihPrikazov = new HashSet<TypPrikazu>() { TypPrikazu.GoToPozicia };

        public static ProcessorPrikazovResult SpracujPrikaz(Prikaz prikaz,
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor)
        {
            var r = new ProcessorPrikazovResult();

            try
            {
                var riadok = prikaz.GoTo.Value.Riadok;
                var stlpec = prikaz.GoTo.Value.Pozicia;

                if(riadok < 0 || riadok >= editor.Riadky().Count
                    || stlpec < 0 || stlpec > editor.Riadky()[riadok].Length())
                {
                    r.Success = false;
                    r.Hlaska = "Neexistujuca pozicia";
                    return r;
                }

                Kurzor.GoTo(riadok, 0, parametreVypisu, editor.Riadky());
                Kurzor.GoTo(riadok, stlpec, parametreVypisu, editor.Riadky());

                r.Success = true;
            }
            catch (Exception ex)
            {
                ErrorLogger.GetInstance().Log(new Chyba()
                {
                    Ex = ex
                });

                r.Hlaska = "Neocakavana chyba";
            }

            return r;
        }
    }
}
