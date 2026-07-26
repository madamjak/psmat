using PisaciAutomat.Config.Locale;
using PisaciAutomat.Subory;
using PisaciStroj;
using PisaciStroj.Chyby;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.IO;

namespace PisaciAutomat.Prikazy.Subory
{
    public static class ProcesorPrikazov
    {
        public static HashSet<TypPrikazu> TypyVyhladavacihPrikazov = new HashSet<TypPrikazu>() { TypPrikazu.UlozAko };

        public static ProcessorPrikazovResult SpracujPrikaz(Prikaz prikaz,
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor)
        {
            var r = new ProcessorPrikazovResult();

            try
            {
                var text = editor.PrecitajTextNaUlozenie();

                using (var writer = new StreamWriter(prikaz.NovyText))
                {
                    writer.Write(text);
                }

                r.Success = true;
                r.Hlaska = Lokalizacia.Hlasky.UspesneUlozeny;
            }
            catch(Exception ex)
            {
                ErrorLogger.GetInstance().Log(new Chyba()
                {
                    Ex = ex
                });

                r.Hlaska = Lokalizacia.Hlasky.ChybaPriUkladani;
            }

            return r;
        }
    }
}
