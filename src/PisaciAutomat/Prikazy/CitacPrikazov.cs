using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Vyhladavanie;
using System.Linq;

namespace PisaciAutomat.Prikazy
{
    public static class CitacPrikazov
    {
        public static PrikazovyAutomatResult NacitajPrikaz(GapBuffer prikazovyRiadok, LexResult tokeny)
        {
            var r = new PrikazovyAutomatResult()
            {
                Prikaz = new Prikaz()
            };

            var p = r.Prikaz;
            
            try
            {
                var parts = tokeny.Tokeny.Values.ToList()[0].Values.ToList();

                if(parts[0].Typ != TypTokenu.KlucoveSlovo)
                {
                    return null;
                }

                var typPrikazu = prikazovyRiadok.Read(parts[0].Pozicia, parts[0].Dlzka);

                if(typPrikazu == "fall")
                {
                    if (!(parts.Count == 2 && parts[1].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }
                    p.Typ = TypPrikazu.Vyhladaj;
                    p.VyhladavanyText = prikazovyRiadok.Read(parts[1].Pozicia + 1, parts[1].Dlzka - 2);

                    return r;
                }
                else if (typPrikazu == "fnext")
                {
                    if (!(parts.Count == 2 && parts[1].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }
                    p.Typ = TypPrikazu.VyhladajDalsi;
                    p.VyhladavanyText = prikazovyRiadok.Read(parts[1].Pozicia + 1, parts[1].Dlzka - 2);

                    return r;
                } else if (typPrikazu == "fprev")
                {
                    if (!(parts.Count == 2 && parts[1].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }
                    p.Typ = TypPrikazu.VyhladajPredosly;
                    p.VyhladavanyText = VyhladavaciAutomat.ReverseString(prikazovyRiadok.Read(parts[1].Pozicia + 1, parts[1].Dlzka - 2));

                    return r;
                }
                else if (typPrikazu == "rst")
                {
                    if (!(parts.Count == 1))
                    {
                        return null;
                    }

                    p.Typ = TypPrikazu.VyhladajReset;
                    r.ZavriRiadok = true;

                    return r;
                }
                else if (typPrikazu == "rfrst")
                {
                    if (!(parts.Count == 3 && parts[1].Typ == TypTokenu.Retazec && parts[2].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }

                    p.Typ = TypPrikazu.VyhladajNahrad;
                    p.VyhladavanyText = prikazovyRiadok.Read(parts[1].Pozicia + 1, parts[1].Dlzka - 2);
                    p.NovyText = prikazovyRiadok.Read(parts[2].Pozicia + 1, parts[2].Dlzka - 2);

                    return r;
                }
                else if (typPrikazu == "rall")
                {
                    if (!(parts.Count == 3 && parts[1].Typ == TypTokenu.Retazec && parts[2].Typ == TypTokenu.Retazec))
                    {
                        return null;
                    }

                    p.Typ = TypPrikazu.VyhladajNahradVsetky;
                    p.VyhladavanyText = prikazovyRiadok.Read(parts[1].Pozicia + 1, parts[1].Dlzka - 2);
                    p.NovyText = prikazovyRiadok.Read(parts[2].Pozicia + 1, parts[2].Dlzka - 2);
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
