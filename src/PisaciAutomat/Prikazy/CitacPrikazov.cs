using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using System;
using System.Linq;

namespace PisaciAutomat.Prikazy
{
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

                var typPrikazu = prikazovyRiadok.Read(parts[0].Pozicia, parts[0].Dlzka);

                if (Vyhladavanie.CitacPrikazov.VyhladavaciePrikazy.Contains(typPrikazu))
                {
                    return Vyhladavanie.CitacPrikazov.NacitajPrikaz(prikazovyRiadok, parts, typPrikazu);
                }
                else if (Subory.CitacPrikazov.VyhladavaciePrikazy.Contains(typPrikazu))
                {
                    return Subory.CitacPrikazov.NacitajPrikaz(prikazovyRiadok, parts, typPrikazu);
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
