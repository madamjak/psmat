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

        public static PrikazovyAutomatResult NacitajPrikaz(GapBuffer prikazovyRiadok, List<Token> tokeny, string typPrikazu)
        {
            var r = new PrikazovyAutomatResult()
            {
                Prikaz = new Prikaz()
            };

            var p = r.Prikaz;

            try
            {
                if (typPrikazu == "fall")
                {
                    if (!(tokeny.Count == 2 && tokeny[1].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }
                    p.Typ = TypPrikazu.Vyhladaj;
                    p.VyhladavanyText = prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2);

                    return r;
                }
                else if (typPrikazu == "fnext")
                {
                    if (!(tokeny.Count == 2 && tokeny[1].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }
                    p.Typ = TypPrikazu.VyhladajDalsi;
                    p.VyhladavanyText = prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2);

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
                else if (typPrikazu == "rfrst")
                {
                    if (!(tokeny.Count == 3 && tokeny[1].Typ == TypTokenu.Retazec && tokeny[2].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }

                    p.Typ = TypPrikazu.VyhladajNahrad;
                    p.VyhladavanyText = prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2);
                    p.NovyText = prikazovyRiadok.Read(tokeny[2].Pozicia + 1, tokeny[2].Dlzka - 2);

                    return r;
                }
                else if (typPrikazu == "rall")
                {
                    if (!(tokeny.Count == 3 && tokeny[1].Typ == TypTokenu.Retazec && tokeny[2].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }

                    p.Typ = TypPrikazu.VyhladajNahradVsetky;
                    p.VyhladavanyText = prikazovyRiadok.Read(tokeny[1].Pozicia + 1, tokeny[1].Dlzka - 2);
                    p.NovyText = prikazovyRiadok.Read(tokeny[2].Pozicia + 1, tokeny[2].Dlzka - 2);
                    r.ZavriRiadok = true;

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
