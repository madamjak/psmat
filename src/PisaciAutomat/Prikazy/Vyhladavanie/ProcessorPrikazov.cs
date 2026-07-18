using PisaciStroj;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System.Collections.Generic;
using System.Linq;

namespace PisaciAutomat.Prikazy.Vyhladavanie
{
    public static class ProcessorPrikazov
    {
        private const string _ziadneVysledky = "End of results or no search result.";

        public static HashSet<TypPrikazu> TypyVyhladavacihPrikazov = new HashSet<TypPrikazu>()
        { TypPrikazu.Vyhladaj, TypPrikazu.VyhladajReset, TypPrikazu.VyhladajDalsi, TypPrikazu.VyhladajPredosly, TypPrikazu.VyhladajNahrad, TypPrikazu.VyhladajNahradVsetky };

        public static ProcessorPrikazovResult SpracujPrikaz(Prikaz prikaz,
            ParametreVyhladavania search,
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor,
            IVyhladavac vyhladavac)
        {
            var r = new ProcessorPrikazovResult()
            {
                Success = true
            };

            if (prikaz.VyhladavanyText != search.VyhladavanyText)
            {
                ResetVyhladavania(search);
                if (prikaz.VyhladavanyText != null)
                {
                    search.VyhladavanyText = prikaz.VyhladavanyText;
                    vyhladavac.NastavVyhladavanie(search.VyhladavanyText);
                }
            }

            if (prikaz.Typ == TypPrikazu.VyhladajReset)
            {
                ResetVyhladavania(search);
                return r;
            }

            TypVyhladavania? typVyhladavania = null;
            if (prikaz.Typ == TypPrikazu.VyhladajDalsi || prikaz.Typ == TypPrikazu.VyhladajPredosly)
            {
                typVyhladavania = TypVyhladavania.Dalsi;
            }
            if (prikaz.Typ == TypPrikazu.Vyhladaj)
            {
                typVyhladavania = TypVyhladavania.Vsetky;
            }

            if (prikaz.Typ == TypPrikazu.Vyhladaj)
            {
                if (search.Typ.HasValue && search.Typ.Value != typVyhladavania.Value)
                {
                    ResetVyhladavania(search);
                    search.Typ = typVyhladavania;
                }

                if (search.VyhladaneSlova == null)
                {
                    var vysl = vyhladavac.VyhladajVsetky(editor.Riadky(), search.VyhladavanyText);

                    if (vysl.PocetNajdenychSlov == 0)
                    {
                        r.Hlaska = _ziadneVysledky;
                        return r;
                    }

                    search.VyhladaneSlova = vysl.Slova;
                }

                if (!search.VyhladaneSlovo.HasValue)
                {
                    search.VyhladaneSlovo = VyhladajDalsie(search.VyhladaneSlova, parametreVypisu, editor.Riadky(), 0, true);
                }
                else
                {
                    search.VyhladaneSlovo = VyhladajDalsie(search.VyhladaneSlova, parametreVypisu, editor.Riadky(), search.VyhladaneSlovo.Value.Riadok, false);
                }

                if (!search.VyhladaneSlovo.HasValue)
                {
                    r.Hlaska = _ziadneVysledky;
                    return r;
                }

                return r;
            }

            if (prikaz.Typ == TypPrikazu.VyhladajDalsi || prikaz.Typ == TypPrikazu.VyhladajPredosly)
            {
                if (search.Typ.HasValue && search.Typ.Value != typVyhladavania.Value)
                {
                    ResetVyhladavania(search);
                    search.Typ = typVyhladavania;
                }

                var obratene = prikaz.Typ == TypPrikazu.VyhladajPredosly;
                var s = vyhladavac.Vyhladaj(prikaz.VyhladavanyText, parametreVypisu, editor.Riadky(), obratene);
                if (!s.HasValue)
                {
                    r.Hlaska = _ziadneVysledky;
                    return r;
                }

                search.VyhladaneSlovo = s.Value;
                search.Obratene = obratene;
                var go = obratene ? s.Value.Pozicia : s.Value.Pozicia + s.Value.Dlzka;
                Kurzor.GoTo(s.Value.Riadok, go, parametreVypisu, editor.Riadky());

                return r;
            }

            if (prikaz.Typ == TypPrikazu.VyhladajNahrad)
            {
                search.VyhladaneSlova = null;
                search.VyhladaneSlovo = null;
                if (search.ZaciatokVyhladavania.HasValue)
                {
                    Kurzor.GoTo(search.ZaciatokVyhladavania.Value.Riadok, search.ZaciatokVyhladavania.Value.Riadok, parametreVypisu, editor.Riadky());
                }
                if (editor.VyhladajANahrad(prikaz.VyhladavanyText, prikaz.NovyText, parametreVypisu))
                {
                    search.ZaciatokVyhladavania = new Pozicia()
                    {
                        Riadok = parametreVypisu.IndexRiadok,
                        Stlpec = parametreVypisu.IndexStlpec,
                    };
                }
                else
                {
                    r.Hlaska = _ziadneVysledky;

                    return r;
                }
            }

            if (prikaz.Typ == TypPrikazu.VyhladajNahradVsetky)
            {
                var aktualnyR = parametreVypisu.IndexRiadok;
                var aktualnyS = parametreVypisu.IndexStlpec;

                var pocetNahradenych = editor.VyhladajANahradVsetky(prikaz.VyhladavanyText, prikaz.NovyText, parametreVypisu);
                if (pocetNahradenych > 0)
                {
                    Kurzor.GoTo(aktualnyR, aktualnyS, parametreVypisu, editor.Riadky());
                    r.Hlaska = string.Format("{0} uprav", pocetNahradenych);
                }
                else
                {
                    r.Hlaska = _ziadneVysledky;
                }

                ResetVyhladavania(search);
            }

            return r;
        }

        private static void ResetVyhladavania(ParametreVyhladavania search)
        {
            search.VyhladaneSlova = null;
            search.VyhladaneSlovo = null;
            search.ZaciatokVyhladavania = null;
            search.Typ = null;
            search.VyhladavanyText = null;
        }

        private static VyhladaneSlovo? VyhladajDalsie(Dictionary<int, Dictionary<int, VyhladaneSlovo>> vyhladaneSlova,
            ParametreVypisu parametreVypisu, List<GapBuffer> riadky, int indexRiadku, bool prveVyhladavanie)
        {
            var navigovaciPrikaz = new NavigovaciPrikaz()
            {
                Typ = TypNavigacie.DalsiaStranka
            };

            Kurzor.GoTo(indexRiadku, 0, parametreVypisu, riadky);
            if (!prveVyhladavanie)
            {
                //navigacia vo vysledkoch, hladaj na dalsej stranke
                Navigator.Naviguj(navigovaciPrikaz, parametreVypisu, riadky, new ParametreVyberu());
            }

            while (true)
            {
                var pocetRiadkov = 0;
                for (int index = parametreVypisu.IndexRiadok; index < riadky.Count; index++)
                {
                    Dictionary<int, VyhladaneSlovo> slovaNaRiadku = vyhladaneSlova[index];
                    if (slovaNaRiadku.Count > 0)
                    {
                        var result = slovaNaRiadku.Values.ToList().First();
                        return new VyhladaneSlovo()
                        {
                            Dlzka = result.Dlzka,
                            Pozicia = result.Pozicia,
                            Riadok = index
                        };
                    }

                    if (pocetRiadkov == parametreVypisu.Vyska)
                    {
                        break;
                    }
                }

                Navigator.Naviguj(navigovaciPrikaz, parametreVypisu, riadky, new ParametreVyberu());

                if (parametreVypisu.IndexRiadok + parametreVypisu.Vyska >= riadky.Count)
                {
                    break;
                }
            }

            return null;
        }
    }
}
