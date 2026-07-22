using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Prikazy.Navigacia
{
    public static class CitacPrikazov
    {
        public static HashSet<string> VyhladavaciePrikazy = new HashSet<string>
        {
            "goto"
        };

        public static PrikazovyAutomatResult NacitajPrikaz(GapBuffer prikazovyRiadok, List<Token> tokeny, string typPrikazu)
        {
            var r = new PrikazovyAutomatResult();

            var p = new Prikaz();

            try
            {
                if (typPrikazu == "goto")
                {
                    if (!(tokeny.Count == 3 && tokeny[1].Typ == TypTokenu.Cislo) && tokeny[2].Typ == TypTokenu.Cislo)
                    {
                        return null;
                    }

                    var riadok = Convert.ToInt32(prikazovyRiadok.Read(tokeny[1].Pozicia, tokeny[1].Dlzka));
                    var stlpec = Convert.ToInt32(prikazovyRiadok.Read(tokeny[2].Pozicia, tokeny[2].Dlzka));

                    p.Typ = TypPrikazu.GoToPozicia;
                    p.GoTo = new PisaciStroj.Vyhladavanie.VyhladaneSlovo() 
                    {
                        Riadok = riadok - 1,
                        Pozicia = stlpec - 1
                    };
                    p.ZavriRiadok = true;

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
