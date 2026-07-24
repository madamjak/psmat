using PisaciStroj.Lexer;
using PisaciStroj.Lexer.Algoritmy;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PisaciAutomat.Obrazovka.Optimalizacia
{
    public static class Zatvorky
    {
        internal static void UpravPrecitanyText(
            ParametreVypisu parametre,
            LexResult precitanyText,
            List<GapBuffer> gapBuffers,
            OptimalizacieResult result)
        {
            var textUpraveny = false;

            //predpoklad ze precitat riadok je vzdy rychlejsie ako pripadne cely text
            var s = 0;
            var r = gapBuffers[parametre.IndexRiadok];

            var obsahujeZatvorky = false;
            var obsahujeZaciatokKomentara = false;
            var obsahujeKoniecKomentara = false;
            while (true)
            {
                if (s == r.Length())
                {
                    break;
                }

                if (StackBracketMatching.Zatvorky.Contains(r.CharAt(s)))
                {
                    obsahujeZatvorky = true;
                }

                var zaciatokKomentara = r.Read(s, LexAutomat._zaciatokKomentara.Length);
                if (zaciatokKomentara == LexAutomat._zaciatokKomentara)
                {
                    obsahujeZaciatokKomentara = true;
                }

                var koniecKomentara = r.Read(s, LexAutomat._koniecKomentara.Length);
                if (koniecKomentara == LexAutomat._koniecKomentara)
                {
                    obsahujeKoniecKomentara = true;
                }

                s++;
            }

            var precitanyTextObsahujeZatvorku = false;
            Dictionary<int, Zatvorka> zatvorkyNaRiadku = null;
            if (precitanyText.Zatvorky != null && precitanyText.Zatvorky.TryGetValue(parametre.IndexRiadok, out zatvorkyNaRiadku))
            {
                precitanyTextObsahujeZatvorku = true;
            }

            var precitanyTextObsahujeKomentar = false;
            if (precitanyText.Komentare != null
                && precitanyText.Komentare.ContainsKey(parametre.IndexRiadok))
            {
                precitanyTextObsahujeKomentar = true;
            }

            if (!precitanyTextObsahujeKomentar &&
                ((obsahujeZaciatokKomentara && !obsahujeKoniecKomentara)
                || (!obsahujeZaciatokKomentara && obsahujeKoniecKomentara)))
            {
                //predpoklad ze je potreba prekreslit zakomentovany kod
                textUpraveny = false;
            }

            else if (precitanyTextObsahujeZatvorku || obsahujeZatvorky)
            {
                //predpoklad ze ak ide o existujuce zatvorky na riadku, tak je vhodne prepocitat len riadok
                if (precitanyTextObsahujeZatvorku && LenZatvorkyNaRiadku(zatvorkyNaRiadku, parametre.IndexRiadok))
                {
                    zatvorkyNaRiadku = StackBracketMatching.GetMatchingBrackets(gapBuffers[parametre.IndexRiadok], parametre.IndexRiadok);
                    precitanyText.Zatvorky[parametre.IndexRiadok] = zatvorkyNaRiadku;

                    textUpraveny = true;
                }
            }
            else
            {
                textUpraveny = true;
            }

            result.PrecitajZnova = !textUpraveny;
        }

        private static bool LenZatvorkyNaRiadku(Dictionary<int, Zatvorka> zatvorky, int indexRiadku)
        {
            var zatv = zatvorky.Values.ToList();

            foreach (var z in zatv)
            {
                if (z.Start.Riadok != indexRiadku || z.End.Riadok != indexRiadku)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
