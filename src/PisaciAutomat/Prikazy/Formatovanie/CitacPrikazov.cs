using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Prikazy.Formatovanie
{
    public static class CitacPrikazov
    {
        public static HashSet<string> VyhladavaciePrikazy = new HashSet<string>
        {
            "zari"
        };

        public static PrikazovyAutomatResult NacitajPrikaz(GapBuffer prikazovyRiadok, List<Token> tokeny, string typPrikazu)
        {
            var r = new PrikazovyAutomatResult();

            var p = new Prikaz();

            try
            {
                if (typPrikazu == "zari")
                {
                    if (!(tokeny.Count == 1))
                    {
                        return null;
                    }

                    p.Typ = TypPrikazu.ZalomRiadky;

                    r.Prikaz = p;
                    return r;
                }
                else
                {
                    return r;
                }
            }
            catch
            {
                r.Prikaz = null;
                return r;
            }
        }
    }
}
