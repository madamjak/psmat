using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PisaciAutomat.Prikazy
{
    public class PrikazovyAutomatResult
    {
        public Prikaz Prikaz { get; set; }

        public bool ZavriRiadok { get; set; }

        public bool Ukonci { get; set; }

        public string Hlaska { get; set; }

        public string Dialog { get; set; }
        public bool Potvrd { get; internal set; }
    }

    public enum TypPrikazu
    {
        Vyhladaj,
        VyhladajReset,
        VyhladajDalsi,
        VyhladajPredosly,
        VyhladajNahrad,
        VyhladajNahradVsetky,
        UlozAko,
        GoToSlovo,
        GoToPozicia
    }

    public class Prikaz
    {
        public TypPrikazu Typ { get; set; }

        public string VyhladavanyText { get; set; }

        public bool JeRegex { get; set; }

        public string NovyText { get; set; }

        public bool ZavriRiadok { get; internal set; }

        public VyhladaneSlovo? GoTo { get; set; }
    }

    public static class CitacPrikazov
    {
        public static PrikazovyAutomatResult NacitajPrikaz(GapBuffer prikazovyRiadok, LexResult tokeny)
        {
            var r = new PrikazovyAutomatResult();

            try
            {
                var parts = tokeny.Tokeny.Values.ToList()[0].Values.ToList();

                if (parts[0].Typ != TypTokenu.KlucoveSlovo)
                {
                    return r;
                }

                //sucastou tokenu prikazu je konecna medzera
                var typPrikazu = prikazovyRiadok.Read(parts[0].Pozicia, parts[0].Dlzka - 1);

                if (Vyhladavanie.CitacPrikazov.VyhladavaciePrikazy.Contains(typPrikazu))
                {
                    return Vyhladavanie.CitacPrikazov.NacitajPrikaz(prikazovyRiadok, parts, typPrikazu, tokeny);
                }
                else if (Subory.CitacPrikazov.VyhladavaciePrikazy.Contains(typPrikazu))
                {
                    return Subory.CitacPrikazov.NacitajPrikaz(prikazovyRiadok, parts, typPrikazu);
                }
                else if (Navigacia.CitacPrikazov.VyhladavaciePrikazy.Contains(typPrikazu))
                {
                    return Navigacia.CitacPrikazov.NacitajPrikaz(prikazovyRiadok, parts, typPrikazu);
                }
                else
                {
                    return new PrikazovyAutomatResult();
                }
            }
            catch(Exception ex)
            {
                return r;
            }
            
        }
    }
}
