using PisaciAutomat.Config.Locale;
using PisaciStroj;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PisaciAutomat.Prikazy.Vyhladavanie
{
    public static class ProcessorPrikazov
    {
        public static HashSet<TypPrikazu> TypyVyhladavacihPrikazov = new HashSet<TypPrikazu>()
        { TypPrikazu.Vyhladaj, TypPrikazu.VyhladajReset, TypPrikazu.VyhladajDalsi, TypPrikazu.VyhladajPredosly, TypPrikazu.VyhladajNahrad, TypPrikazu.VyhladajNahradVsetky,
         TypPrikazu.GoToSlovo, TypPrikazu.GoToSlovo };

        public static ProcessorPrikazovResult SpracujPrikaz(Prikaz prikaz,
            ParametreVyhladavania search,
            ParametreVypisu parametreVypisu,
            IPisaciStroj editor,
            IVyhladavac vyhladavac)
        {
            try
            {
                return SpracujPrikazInternal(prikaz, search, parametreVypisu, editor, vyhladavac);
            }
            catch(Exception ex)
            {
                ResetVyhladavania(search);
                throw;
            }
            
        }

        private static ProcessorPrikazovResult SpracujPrikazInternal(Prikaz prikaz, ParametreVyhladavania search, ParametreVypisu parametreVypisu, IPisaciStroj editor, IVyhladavac vyhladavac)
        {
            var r = new ProcessorPrikazovResult()
            {
                Success = true
            };

            if (prikaz.Typ == TypPrikazu.GoToSlovo)
            {
                search.VyhladaneSlovo = prikaz.GoTo;
                var radok = Math.Max(0, prikaz.GoTo.Value.Riadok - 10);
                var stlpec = prikaz.GoTo.Value.Pozicia + prikaz.GoTo.Value.Dlzka;
                Kurzor.GoTo(radok, 0, parametreVypisu, editor.Riadky());
                Kurzor.GoTo(prikaz.GoTo.Value.Riadok, stlpec, parametreVypisu, editor.Riadky());
                return r;
            }

            if (prikaz.Typ == TypPrikazu.GoToPozicia)
            {
                var radok = Math.Max(0, prikaz.GoTo.Value.Riadok - 10);
                var stlpec = prikaz.GoTo.Value.Pozicia + prikaz.GoTo.Value.Dlzka;
                Kurzor.GoTo(radok, 0, parametreVypisu, editor.Riadky());
                Kurzor.GoTo(prikaz.GoTo.Value.Riadok, stlpec, parametreVypisu, editor.Riadky());
                return r;
            }

            if (prikaz.VyhladavanyText != search.VyhladavanyText)
            {
                ResetVyhladavania(search);
                if (prikaz.VyhladavanyText != null)
                {
                    search.VyhladavanyText = prikaz.VyhladavanyText;
                    if (prikaz.JeRegex)
                    {
                        vyhladavac.NastavRegexVyhladavanie(search.VyhladavanyText);
                    }
                    else
                    {
                        vyhladavac.NastavVyhladavanie(search.VyhladavanyText);
                    }

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
                        r.Hlaska = Lokalizacia.Hlasky.KoniecVysledkov;
                        return r;
                    }

                    search.VyhladaneSlova = vysl.Slova;
                    r.Success = true;
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
                    r.Hlaska = Lokalizacia.Hlasky.KoniecVysledkov;
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
                    r.Success = true;
                    search.ZaciatokVyhladavania = new Pozicia()
                    {
                        Riadok = parametreVypisu.IndexRiadok,
                        Stlpec = parametreVypisu.IndexStlpec,
                    };
                }
                else
                {
                    r.Hlaska = Lokalizacia.Hlasky.KoniecVysledkov;

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
                    r.Hlaska = string.Format(Lokalizacia.Hlasky.PocetUprav, pocetNahradenych);
                    r.Success = true;
                }
                else
                {
                    r.Hlaska = Lokalizacia.Hlasky.KoniecVysledkov;
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
    }
}
