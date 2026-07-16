using PisaciStroj;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;

namespace PisaciAutomat.Prikazy
{
    public static class ProcesorPrikazov
    {
        public static void SpracujPrikaz(Prikaz prikaz, 
            ParametreVyhladavania search, 
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor)
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
                prikaz.ZavriRiadok = true;
            }

            if (prikaz.Typ == TypPrikazu.Vyhladaj)
            {
                search.VyhladaneSlovo = null;
                search.ZaciatokVyhladavania = null;
                prikaz.ZavriRiadok = true;
            }

            if (prikaz.Typ == TypPrikazu.VyhladajDalsi)
            {
                var s = editor.Vyhladaj(prikaz.VyhladavanyText, parametreVypisu);
                if (s.HasValue)
                {
                    search.VyhladaneSlovo = s.Value;
                    Kurzor.GoTo(s.Value.Riadok, s.Value.Pozicia + s.Value.Dlzka, parametreVypisu, editor.Riadky());
                }
            }

            //if (prikaz.Typ == TypPrikazu.VyhladajPredosly)
            //{
            //    //vyhladavany text mozne pre vyhladavaci automat obratit (reverse)
            //    //nova funkcia pisaceho stroja (alebo vyhladavaca?), prechadzat textom od pozicie kurzora 'dolava a hore'
            //}

            if (prikaz.Typ == TypPrikazu.VyhladajNahrad)
            {
                search.VyhladaneSlovo = null;
                if (editor.VyhladajANahrad(prikaz.VyhladavanyText, prikaz.NovyText, parametreVypisu))
                {
                    search.ZaciatokVyhladavania = new Pozicia()
                    {
                        Riadok = parametreVypisu.IndexRiadok,
                        Stlpec = parametreVypisu.IndexStlpec,
                    };
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
                    Kurzor.GoTo(aktualnyR, aktualnyS, parametreVypisu, editor.Riadky());
                };

                prikaz.ZavriRiadok = true;
            }
        }
    }
}
