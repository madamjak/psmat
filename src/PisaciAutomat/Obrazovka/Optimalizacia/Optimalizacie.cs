using PisaciAutomat.Obrazovka.Optimalizacia;
using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System.Collections.Generic;
using System.Linq;

namespace PisaciAutomat.Obrazovka
{
    public class OptimalizacieResult
    {
        public bool KomentarUpraveny { get; set; }
        public bool PrecitajZnova { get; set; }
    }

    public static class Optimalizacie
    {
        /// <summary>
        /// 1. v pripade klasickeho zapisu alebo mazania na jedinom riadku nie je potrebne citat cely text ak nebol zapisany viacriadkovy komentar alebo zatvorka
        ///     - postacuje syntax highlight riadku, ktory vyzera byt dostatocne rychly bez potreby optimalizacie 
        ///     - pouzita jednoducha optimalizacia zapamatat si precitane tokeny a pouzit ich znova ak sa da
        /// 2. v pripade komentara ci zatvorky over ci je mozne upravit lexResult bez potreby citania:
        ///     - v pripade zapisu uprostred komentara staci upravit jeho dlzku / koniec
        ///     - v pripade zapisu zatvorky over ci nestaci prepocitat jediny riadok
        /// </summary>
        internal static bool UpravPrecitanyText(
            ParametrePrekreslenia parametrePrekreslenia, 
            ParametreVypisu parametre,
            LexResult precitanyText, 
            List<GapBuffer> gapBuffers,
            ILexer lexer)
        {
            if (!lexer.JeLexerNastaveny())
            {
                return true;
            }

            var result = new OptimalizacieResult();

            Komentare.UpravExistujuciKomentar(parametrePrekreslenia, parametre, precitanyText, gapBuffers, result);

            if (result.KomentarUpraveny || result.PrecitajZnova)
            {
                return !result.PrecitajZnova;
            }

            Zatvorky.UpravPrecitanyText(parametre, precitanyText, gapBuffers, result);

            return !result.PrecitajZnova;
        }

        public static void UpravTokenyRiadkov(
            Dictionary<int, Dictionary<int, Token>> tokeny, 
            ParametreVypisu parametreVypisu,
            LexResult precitanyText,
            List<GapBuffer> riadky,
            ILexer lexer)
        {
            var noveTokeny = new Dictionary<int, Dictionary<int, Token>>();

            if (!lexer.JeLexerNastaveny())
            {
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
                if (tokeny != null && tokeny.TryGetValue(riadok, out tokenyRiadku))
                {
                    noveTokeny.Add(riadok, tokenyRiadku);
                }
                else
                {
                    noveTokeny.Add(riadok, PrecitajTokenyRiadku(precitanyText, lexer, riadky, riadok));
                }

                riadok++;
                pocetRiadkov++;
            }

            precitanyText.Tokeny = noveTokeny;
        }

        public static Dictionary<int, Token> PrecitajTokenyRiadku(LexResult lexResult, ILexer lexer, List<GapBuffer> riadky, int i)
        {
            Dictionary<int, Token> tokeny;
            Dictionary<int, Token> noveTokeny = null;
            if (lexResult.Komentare == null || !lexResult.Komentare.TryGetValue(i, out noveTokeny))
            {
                noveTokeny = new Dictionary<int, Token>();
            }

            tokeny = lexer.LexPreEditor(riadky[i]);

            foreach (var to in tokeny)
            {
                var zvyrazniToken = true;
                foreach (var koment in noveTokeny)
                {
                    if (koment.Key <= to.Key && to.Key <= koment.Key + koment.Value.Dlzka)
                    {
                        zvyrazniToken = false;
                    }
                }

                if (zvyrazniToken)
                {
                    noveTokeny.Add(to.Key, to.Value);
                }
            }

            if (lexResult.Tokeny == null)
            {
                lexResult.Tokeny = new Dictionary<int, Dictionary<int, Token>>();
                lexResult.Tokeny.Add(i, noveTokeny);
            }
            else
            {
                lexResult.Tokeny[i] = noveTokeny;
            }

            return noveTokeny;
        }
    }
}
