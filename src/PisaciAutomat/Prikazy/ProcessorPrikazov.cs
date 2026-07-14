using PisaciStroj;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;

namespace PisaciAutomat.Prikazy
{
    public static class ProcessorPrikazov
    {
        public static Prikaz NacitajPrikaz(GapBuffer prikazovyRiadok)
        {
            var p = new Prikaz();
            try
            {
                var parts = prikazovyRiadok.Read().Split(' ');

                if (parts.Length == 2 && (parts[0] == "find"))
                {
                    p.Typ = TypPrikazu.Vyhladaj;
                    p.VyhladavanyText = parts[1];

                    return p;
                }
                if (parts.Length == 2 && (parts[0] == "next"))
                {
                    p.Typ = TypPrikazu.VyhladajDalsi;
                    p.VyhladavanyText = parts[1];

                    return p;
                }
                if (parts.Length == 1 && parts[0] == "rest")
                {
                    p.Typ = TypPrikazu.VyhladajReset;

                    return p;
                }
                else if (parts.Length == 3 && (parts[0] == "rfirst"))
                {
                    p.Typ = TypPrikazu.VyhladajNahrad;
                    p.VyhladavanyText = parts[1];
                    p.NovyText = parts[2];

                    return p;
                }
                else if (parts.Length == 3 && parts[0] == "rall")
                {
                    p.Typ = TypPrikazu.VyhladajNahradVsetky;
                    p.VyhladavanyText = parts[1];
                    p.NovyText = parts[2];

                    return p;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static void SpracujPrikaz(Prikaz prikaz, 
            ParametreVyhladavania search, 
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor,
            ref bool cmdMode)
        {
            if(prikaz.VyhladavanyText != search.VyhladavanyText)
            {
                search.VyhladavanyText = prikaz.VyhladavanyText;
                if(search.VyhladavanyText != null)
                {
                    editor.NastavVyhladavanie(search.VyhladavanyText);
                }
            }

            if (prikaz.Typ == TypPrikazu.VyhladajReset)
            {
                search = new ParametreVyhladavania();
                cmdMode = false;
            }

            if (prikaz.Typ == TypPrikazu.Vyhladaj)
            {
                search.VyhladaneSlovo = null;
                cmdMode = false;
            }

            if (prikaz.Typ == TypPrikazu.VyhladajDalsi)
            {
                if (search.VyhladaneSlovo.HasValue)
                {
                    Kurzor.PosunKurzorDoprava(parametreVypisu, editor.Riadky());
                }

                var s = editor.Vyhladaj(prikaz.VyhladavanyText, parametreVypisu);
                if (s.HasValue)
                {
                    search.VyhladaneSlovo = s.Value;
                    Kurzor.GoTo(s.Value.Riadok, s.Value.Pozicia, parametreVypisu, editor.Riadky());
                }

                if (search.VyhladaneSlovo.HasValue && !s.HasValue)
                {
                    Kurzor.PosunKurzorDolava(parametreVypisu, editor.Riadky());
                }
            }

            if (prikaz.Typ == TypPrikazu.VyhladajNahrad)
            {
                search.VyhladaneSlovo = null;
                if (editor.VyhladajANahrad(prikaz.VyhladavanyText, prikaz.NovyText, parametreVypisu))
                {
                    
                };
            }

            if (prikaz.Typ == TypPrikazu.VyhladajNahradVsetky)
            {
                search.VyhladaneSlovo = null;
                search.VyhladavanyText = null;
                var aktualnyR = parametreVypisu.IndexRiadok;
                var aktualnyS = parametreVypisu.IndexStlpec;

                if (editor.VyhladajANahradVsetky(prikaz.VyhladavanyText, prikaz.NovyText, parametreVypisu))
                {
                    
                };

                Kurzor.GoTo(aktualnyR, aktualnyS, parametreVypisu, editor.Riadky());
                cmdMode = false;
            }
        }
    }
}
