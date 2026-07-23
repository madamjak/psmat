using PisaciStroj.Lexer;
using PisaciStroj.Lexer.Algoritmy;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PisaciAutomat.Obrazovka
{
    public static class Optimalizacie
    {
        /// <summary>
        /// 1. v pripade klasickeho zapisu alebo mazania na jedinom riadku nie je potrebne citat cely text ak nebol zapisany viacriadkovy komentar alebo zatvorka
        ///     - postacuje syntax highlight riadku, ktory vyzera byt dostatocne rychly bez potreby optimalizacie 
        ///     - jednoducha optimalizacia zapamatat si precitane tokeny a pouzit ich znova ak sa da
        /// 2. v pripade komentara ci zatvorky over ci je mozne upravit lexResult bez potreby citania:
        ///     - v pripade zapisu uprostred komentara staci upravit jeho dlzku / koniec
        ///     - v pripade zapisu zatvorky over ci nestaci prepocitat jediny riadok
        /// </summary>
        internal static bool UpravPrecitanyText(
            ParametrePrekreslenia parametrePrekreslenia, 
            ParametreVypisu parametreVypisu, 
            LexResult precitanyText, 
            List<GapBuffer> gapBuffers)
        {
            //ak nebolo nic zmenene necitaj znova
            if (parametrePrekreslenia.LenPrekresli)
            {
                return true;
            }

            //rozhodovanie na zaklade indexov moze byt v pripade zlozitejsich operacii ako replace/undo/redo nespolahlive
            if (!parametrePrekreslenia.ZmazalAleboZapisal)
            {
                precitanyText.Tokeny = null;
                return false;
            }
            else
            {
                //uloz neupravene riadky na znovu-pouzitie pri highlightingu co sa da, ukladaj vsak vzdy len stranku
                UpravTokenyRiadkov(precitanyText.Tokeny,
                    parametrePrekreslenia,
                    parametreVypisu,
                    precitanyText,
                    gapBuffers);
            }

            var textuUpraveny = false;

            //v pripade upravy viacriadkoveho komentara len uprav existujuci token
            if (parametrePrekreslenia.ZaciatokAkcie.Riadok == parametrePrekreslenia.KoniecAkcie.Riadok)
            {
                Dictionary<int, Token> komentar;
                if (precitanyText.Komentare != null
                    && precitanyText.Komentare.TryGetValue(parametrePrekreslenia.ZaciatokAkcie.Riadok, out komentar))
                {
                    if (komentar.Count > 0)
                    {
                        var poziciaKomentu = 0;
                        Token? token = null;
                        foreach(var koment in komentar)
                        {
                            if(parametrePrekreslenia.ZaciatokAkcie.Stlpec > koment.Key
                            && parametrePrekreslenia.KoniecAkcie.Stlpec > koment.Key)
                            {
                                token = koment.Value;
                                poziciaKomentu = koment.Key;
                                break;
                            }
                        }

                        if (token.HasValue)
                        {
                            var pocetZnakov = parametrePrekreslenia.KoniecAkcie.Stlpec - parametrePrekreslenia.ZaciatokAkcie.Stlpec;
                            komentar[poziciaKomentu] = new Token()
                            {
                                Typ = TypTokenu.Komentar,
                                Dlzka = token.Value.Dlzka + pocetZnakov
                            };

                            precitanyText.Tokeny[parametrePrekreslenia.ZaciatokAkcie.Riadok] = komentar;

                            textuUpraveny = true;
                        }
                    }
                }
            }

            if (textuUpraveny)
            {
                return true;
            }

            //predpoklad ze precitat riadok je vzdy rychlejsie ako pripadne cely text
            if (parametrePrekreslenia.ZaciatokAkcie.Riadok == parametrePrekreslenia.KoniecAkcie.Riadok)
            {
                var s = 0;
                var r = gapBuffers[parametrePrekreslenia.ZaciatokAkcie.Riadok];
                var obsahujeZatvorky = false;
                while (true)
                {
                    if(s == r.Length())
                    {
                        break;
                    }

                    if (StackBracketMatching.Zatvorky.Contains(r.CharAt(s)))
                    {
                        obsahujeZatvorky = true;
                        break;
                    }

                    s++;
                }

                if (obsahujeZatvorky)
                {
                    //predpoklad ze ak ide o existujuce zatvorky na riadku, tak je vhodne prepocitat len riadok
                    Dictionary<int, Zatvorka> zatvorkyNaRiadku;
                    if (precitanyText.Zatvorky != null && precitanyText.Zatvorky.TryGetValue(parametrePrekreslenia.ZaciatokAkcie.Riadok, out zatvorkyNaRiadku))
                    {
                        if (LenZatvorkyNaRiadku(zatvorkyNaRiadku, parametrePrekreslenia.ZaciatokAkcie.Riadok))
                        {
                            zatvorkyNaRiadku = StackBracketMatching.GetMatchingBrackets(gapBuffers[parametrePrekreslenia.ZaciatokAkcie.Riadok], parametrePrekreslenia.ZaciatokAkcie.Riadok);
                            precitanyText.Zatvorky[parametrePrekreslenia.ZaciatokAkcie.Riadok] = zatvorkyNaRiadku;

                            textuUpraveny = true;
                        }
                    }
                }
            }

            //...teoreticky mozne vymyslat dalsie optimalizacie, zatial postacuje
            return textuUpraveny;
        }

        private static bool LenZatvorkyNaRiadku(Dictionary<int, Zatvorka> zatvorky, int indexRiadku)
        {
            var zatv = zatvorky.Values.ToList();

            foreach(var z in zatv)
            {
                if(z.Start.Riadok != indexRiadku || z.End.Riadok != indexRiadku)
                {
                    return false;
                }
            }

            return true;
        }

        private static void UpravTokenyRiadkov(
            Dictionary<int, Dictionary<int, Token>> tokeny, 
            ParametrePrekreslenia parametrePrekreslenia,
            ParametreVypisu parametreVypisu,
            LexResult precitanyText,
            List<GapBuffer> riadky)
        {
            var noveTokeny = new Dictionary<int, Dictionary<int, Token>>();

            var posunRiadkov = 0;
            if (parametrePrekreslenia.ZmazalAleboZapisal)
            {
                posunRiadkov = parametrePrekreslenia.KoniecAkcie.Riadok - parametrePrekreslenia.ZaciatokAkcie.Riadok;
            }

            if(posunRiadkov != 0)
            {
                //zatial nic
                precitanyText.Tokeny = noveTokeny;
                return;
            }

            var riadok = parametreVypisu.OffsetRiadok;
            var vyska = parametreVypisu.Vyska;
            var pocetRiadkov = 0;
            while (true)
            {
                if(pocetRiadkov == vyska || pocetRiadkov == riadky.Count())
                {
                    break;
                }

                Dictionary<int, Token> tokenyRiadku = null;
                if (tokeny.TryGetValue(riadok, out tokenyRiadku))
                {
                    if (!parametrePrekreslenia.ZmazalAleboZapisal
                        || parametrePrekreslenia.KoniecAkcie.Riadok != riadok)
                    {
                        noveTokeny.Add(riadok, tokenyRiadku);
                    }
                    else
                    {
                        //upraveny riadok tokenizuj znova
                        noveTokeny.Add(riadok, null);
                    }
                }

                riadok++;
                pocetRiadkov++;
            }

            precitanyText.Tokeny = noveTokeny;
        }
    }
}
