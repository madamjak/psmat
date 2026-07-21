using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Vyhladavanie;
using System.Collections.Generic;

namespace PisaciAutomat.Prikazy.Vyhladavanie
{
    public static class CitacPrikazov
    {
        public static HashSet<string> VyhladavaciePrikazy = new HashSet<string>
        {
            "fall", "fnext", "fprev", "rfrst", "rall"
        };

        public static PrikazovyAutomatResult NacitajPrikaz(GapBuffer prikazovyRiadok, List<Token> tokeny, string typPrikazu, LexResult lexResults)
        {
            var r = new PrikazovyAutomatResult()
            {
                Prikaz = new Prikaz()
            };

            var p = r.Prikaz;

            try
            {
                if (typPrikazu == "fall" || typPrikazu == "fnext")
                {
                    if (!(tokeny.Count == 2 
                        && (tokeny[1].Typ == TypTokenu.Retazec)
                            || tokeny[1].Typ == TypTokenu.Regex))
                    {
                        return null;
                    }
                    p.Typ = typPrikazu == "fall" ? TypPrikazu.Vyhladaj : TypPrikazu.VyhladajDalsi;

                    

                    if (tokeny[1].Typ == TypTokenu.Regex)
                    {
                        p.JeRegex = true;
                        p.VyhladavanyText = RegexParser.ParseRegex(lexResults, prikazovyRiadok, tokeny[1]);
                    }
                    else
                    {
                        p.VyhladavanyText = prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2);
                    }

                    return r;
                }
                else if (typPrikazu == "fprev")
                {
                    if (!(tokeny.Count == 2 && tokeny[1].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }
                    p.Typ = TypPrikazu.VyhladajPredosly;
                    p.VyhladavanyText = VyhladavaciAutomat.ReverseString(prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2));

                    return r;
                }
                else if (typPrikazu == "rst")
                {
                    if (!(tokeny.Count == 1))
                    {
                        return null;
                    }

                    p.Typ = TypPrikazu.VyhladajReset;
                    r.ZavriRiadok = true;

                    return r;
                }
                else if (typPrikazu == "rfrst" || typPrikazu == "rall")
                {
                    if (!(tokeny.Count == 3 
                        && (tokeny[1].Typ == TypTokenu.Retazec || tokeny[1].Typ == TypTokenu.Regex)
                        && tokeny[2].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }

                    p.Typ = typPrikazu == "rfrst" ? TypPrikazu.VyhladajNahrad : TypPrikazu.VyhladajNahradVsetky;
                    p.NovyText = prikazovyRiadok.Read(tokeny[2].Pozicia + 1, tokeny[2].Dlzka - 2);

                    if (tokeny[1].Typ == TypTokenu.Regex)
                    {
                        p.JeRegex = true;
                        p.VyhladavanyText = RegexParser.ParseRegex(lexResults, prikazovyRiadok, tokeny[1]);
                    }
                    else
                    {
                        p.VyhladavanyText = prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2);
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
