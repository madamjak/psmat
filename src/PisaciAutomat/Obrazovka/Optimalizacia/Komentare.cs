using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System.Collections.Generic;

namespace PisaciAutomat.Obrazovka.Optimalizacia
{
    public static class Komentare
    {
        internal static void UpravExistujuciKomentar(
            ParametrePrekreslenia parametrePrekreslenia,
            ParametreVypisu parametre,
            LexResult precitanyText,
            List<GapBuffer> gapBuffers,
            OptimalizacieResult result)
        {
            Dictionary<int, Token> komentar;
            if (precitanyText.Komentare != null
                && precitanyText.Komentare.TryGetValue(parametre.IndexRiadok, out komentar))
            {
                if (komentar.Count > 0)
                {
                    var pocetZnakov = parametrePrekreslenia.KonecnySlpec - parametrePrekreslenia.ZaciatocnyStlpec;

                    var poziciaKomentu = 0;
                    Token? token = null;
                    foreach (var koment in komentar)
                    {
                        if (parametrePrekreslenia.ZaciatocnyStlpec > koment.Key
                        && parametrePrekreslenia.KonecnySlpec > koment.Key)
                        {
                            var koniecKomentara = koment.Value.Pozicia + koment.Value.Dlzka;

                            if (koment.Value.Pozicia + koment.Value.Dlzka == gapBuffers[parametre.IndexRiadok].Length() - 1)
                            {
                                token = koment.Value;
                                poziciaKomentu = koment.Key;
                                break;
                            }
                        }
                    }

                    if (token.HasValue)
                    {
                        komentar[poziciaKomentu] = new Token()
                        {
                            Typ = TypTokenu.Komentar,
                            Dlzka = token.Value.Dlzka + pocetZnakov
                        };

                        precitanyText.Tokeny[parametre.IndexRiadok] = komentar;

                        result.KomentarUpraveny = true;
                    }
                    else
                    {
                        result.PrecitajZnova = true;
                    }
                }
            }
        }
    }
}
