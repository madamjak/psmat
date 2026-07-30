using PisaciAutomat.Config.Locale;
using PisaciStroj;
using PisaciStroj.Chyby;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;

namespace PisaciAutomat.Prikazy
{
    public struct ProcessorPrikazovResult
    {
        public string Hlaska { get; set; }

        public bool Success { get; set; }
    }

    public static class ProcesorPrikazov
    {
        public static ProcessorPrikazovResult SpracujPrikaz(Prikaz prikaz,
            ParametreVyhladavania search,
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor,
            IVyhladavac vyhladavac)
        {
            var r = new ProcessorPrikazovResult();

            try
            {
                if (Vyhladavanie.ProcessorPrikazov.TypyVyhladavacihPrikazov.Contains(prikaz.Typ))
                {
                    return Vyhladavanie.ProcessorPrikazov.SpracujPrikaz(prikaz, search, parametreVypisu, editor, vyhladavac);
                }
                else if (Subory.ProcesorPrikazov.TypyVyhladavacihPrikazov.Contains(prikaz.Typ))
                {
                    return Subory.ProcesorPrikazov.SpracujPrikaz(prikaz, parametreVypisu, editor);
                }
                else if (Navigacia.ProcesorPrikazov.TypyVyhladavacihPrikazov.Contains(prikaz.Typ))
                {
                    return Navigacia.ProcesorPrikazov.SpracujPrikaz(prikaz, parametreVypisu, editor);
                }
                else if (Formatovanie.ProcesorPrikazov.TypyVyhladavacihPrikazov.Contains(prikaz.Typ))
                {
                    return Formatovanie.ProcesorPrikazov.SpracujPrikaz(prikaz, parametreVypisu, editor);
                }
                else
                {
                    return r;
                }
            }catch(Exception ex)
            {
                ErrorLogger.GetInstance().Log(new Chyba()
                {
                    Ex = ex
                });
                r.Success = false;
                r.Hlaska = Lokalizacia.Hlasky.NeznamaChyba;
                return r;
            }
        }
    }
}
