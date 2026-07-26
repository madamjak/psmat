using PisaciAutomat.Config.Locale;
using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using System.Collections.Generic;
using System.IO;

namespace PisaciAutomat.Prikazy.Subory
{
    public static class CitacPrikazov
    {
        public static HashSet<string> VyhladavaciePrikazy = new HashSet<string>
        {
            "saas"
        };

        public static PrikazovyAutomatResult NacitajPrikaz(GapBuffer prikazovyRiadok, List<Token> tokeny, string typPrikazu)
        {
            var r = new PrikazovyAutomatResult();

            var p = new Prikaz();

            try
            {
                if (typPrikazu == "saas")
                {
                    if (!(tokeny.Count == 2 && tokeny[1].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }
                    
                    var cesta = prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2);

                    var invalid = string.IsNullOrWhiteSpace(cesta) || Directory.Exists(cesta);
                    var existujue = File.Exists(cesta);

                    if (invalid)
                    {
                        r.Hlaska = Lokalizacia.Hlasky.ValidnaCesta;
                        return r;
                    }

                    p.Typ = TypPrikazu.UlozAko;
                    p.NovyText = cesta;
                    p.ZavriRiadok = true;

                    r.Prikaz = p;

                    if (existujue)
                    {
                        r.Potvrd = true;
                        r.Dialog = Lokalizacia.Hlasky.SuborExistuje;
                    }

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
