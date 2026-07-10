using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciAutomat.Obrazovka
{
    public static class StylovaciAutomat
    {
        public static string SyntaxHighligt(Dictionary<int, Token> tokens, GapBuffer riadok, int offset, int maxDlzka)
        {
            var sb = new StringBuilder();
            var index = offset;
            var dlzka = 0;

            while (true)
            {
                if(index >= riadok.Length())
                {
                    break;
                }

                if(dlzka == maxDlzka)
                {
                    break;
                }

                Token t;
                if (tokens.TryGetValue(index, out t))
                {
                    var styl = VyberStyl(t.Typ);

                    if (styl != StylTextu.Standard)
                    {
                        sb.Append(AnsiStyl(styl));
                    }

                    var dlzkaT = t.Dlzka;
                    if (dlzka + t.Dlzka > maxDlzka)
                    {
                        dlzkaT = maxDlzka - dlzka;
                    }

                    sb.Append(riadok.Read(index, t.Dlzka));

                    if (styl != StylTextu.Standard)
                    {
                        sb.Append(AnsiReset());
                    }

                    index += dlzkaT;
                    dlzka += dlzkaT;
                }
                else
                {
                    sb.Append(riadok.Read(index, 1));
                    index++;
                    dlzka += 1;
                }
            }

            return sb.ToString();
        }


        public static string SyntaxAndSearchHighligt(Dictionary<int, Token> tokens, GapBuffer riadok, int offset, int maxDlzka, Dictionary<int, VyhladaneSlovo> slova, VyhladaneSlovo? vyhladaneSlovo)
        {
            var sb = new StringBuilder();
            var index = offset;
            var dlzka = 0;

            var dlzkaSlova = 0;
            var dlzkaTokenu = 0;
            Token? lastToken = null;
            bool extraZvyrazni = false;

            while (true)
            {
                var precitalSlovo = false;
                var precitalToken = false;

                if (index >= riadok.Length())
                {
                    break;
                }

                if (dlzka == maxDlzka)
                {
                    break;
                }

                VyhladaneSlovo s;
                if (dlzkaSlova == 0 && slova.TryGetValue(index, out s))
                {
                    dlzkaSlova = s.Dlzka;
                    extraZvyrazni = vyhladaneSlovo.HasValue && vyhladaneSlovo.Value.Pozicia == s.Pozicia;
                }

                Token t;
                if (dlzkaTokenu == 0 && tokens.TryGetValue(index, out t))
                {
                    dlzkaTokenu = t.Dlzka;
                    lastToken = t;
                }

                if (dlzkaSlova > 0) 
                { 
                    sb.Append(extraZvyrazni ? StylSearchResultExtra() : StylSearchResult());

                    sb.Append(riadok.Read(index, 1));

                    if (dlzkaTokenu == 0)
                    {
                        sb.Append(AnsiReset());
                    }

                    dlzkaSlova--;
                    precitalSlovo = true;
                }

                if (dlzkaTokenu > 0)
                {
                    var styl = VyberStyl(lastToken.Value.Typ);
                    if (styl != StylTextu.Standard)
                    {
                        sb.Append(AnsiStyl(styl));
                    }

                    if (precitalSlovo)
                    {
                        sb.Append("\b");
                    }

                    sb.Append(riadok.Read(index, 1));

                    if (styl != StylTextu.Standard || precitalSlovo)
                    {
                        sb.Append(AnsiReset());
                    }

                    dlzkaTokenu--;
                    precitalToken = true;
                }

                if (!precitalSlovo && !precitalToken)
                {
                    sb.Append(riadok.Read(index, 1));
                }

                index += 1;
                dlzka += 1;
            }

            return sb.ToString();
        }

        internal static string SyntaxAndSearchHighligt2(GapBuffer riadok, 
            int offset, 
            int maxDlzka, 
            Dictionary<int, VyhladaneSlovo> slova, 
            VyhladaneSlovo? vyhladaneSlovo, 
            Dictionary<int, Token> tokeny, 
            Dictionary<int, Zatvorka> zatvorky, 
            Pozicia poziciaKurzora,
            VyhladaneSlovo? zvyraznenyText)
        {
            var sb = new StringBuilder();
            var index = offset;
            var dlzka = 0;

            var dlzkaSlova = 0;
            var dlzkaTokenu = 0;
            Token? lastToken = null;
            bool extraZvyrazni = false;
            bool zvyrazniZatvorku = false;
            
            var dlzkaZvyraznenehoTextu = 0;

            while (true)
            {
                var precitalSlovo = false;
                var precitalToken = false;
                var precitalZatvorku = false;

                if (index >= riadok.Length())
                {
                    break;
                }

                if (dlzka == maxDlzka)
                {
                    break;
                }

                VyhladaneSlovo s;
                if (dlzkaSlova == 0 && slova.TryGetValue(index, out s))
                {
                    dlzkaSlova = s.Dlzka;
                    extraZvyrazni = vyhladaneSlovo.HasValue && vyhladaneSlovo.Value.Pozicia == s.Pozicia;
                }

                Token t;
                if (dlzkaTokenu == 0 && tokeny.TryGetValue(index, out t))
                {
                    dlzkaTokenu = t.Dlzka;
                    lastToken = t;
                }

                Zatvorka z;
                if(zatvorky.TryGetValue(index, out z))
                {
                    precitalZatvorku = true;
                    zvyrazniZatvorku = (poziciaKurzora.Riadok == z.Start.Riadok && poziciaKurzora.Stlpec == z.Start.Stlpec)
                        || (poziciaKurzora.Riadok == z.End.Riadok && poziciaKurzora.Stlpec == z.End.Stlpec);
                }

                if (zvyraznenyText.HasValue && zvyraznenyText.Value.Pozicia == index)
                {
                    dlzkaZvyraznenehoTextu = zvyraznenyText.Value.Dlzka;
                }

                if (dlzkaSlova > 0)
                {
                    sb.Append(extraZvyrazni ? StylSearchResultExtra() : StylSearchResult());

                    sb.Append(riadok.Read(index, 1));

                    if (dlzkaTokenu == 0)
                    {
                        sb.Append(AnsiReset());
                    }

                    dlzkaSlova--;
                    precitalSlovo = true;
                }

                if (dlzkaTokenu > 0)
                {
                    var styl = VyberStyl(lastToken.Value.Typ);
                    if (styl != StylTextu.Standard)
                    {
                        sb.Append(AnsiStyl(styl));
                    }

                    if (precitalSlovo)
                    {
                        sb.Append("\b");
                    }

                    sb.Append(riadok.Read(index, 1));

                    if (styl != StylTextu.Standard || precitalSlovo)
                    {
                        sb.Append(AnsiReset());
                    }

                    dlzkaTokenu--;
                    precitalToken = true;
                }

                if(!precitalToken && precitalZatvorku)
                {
                    sb.Append(AnsiStyl(StylTextu.RedBold));
                    if (zvyrazniZatvorku)
                    {
                        sb.Append(StylZatvorky());
                    }
                    sb.Append(riadok.Read(index, 1));
                    sb.Append(AnsiReset());
                }
                
                if (!precitalSlovo && !precitalToken && !precitalZatvorku)
                {
                    sb.Append(riadok.Read(index, 1));
                }

                if (dlzkaZvyraznenehoTextu > 0)
                {
                    sb.Append(StylVyberuTextu());
                    sb.Append("\b");
                    sb.Append(riadok.Read(index, 1));
                    sb.Append(AnsiReset());
                    dlzkaZvyraznenehoTextu--;
                }

                index += 1;
                dlzka += 1;
            }

            return sb.ToString();
        }

        private static string StylSearchResult()
        {
            return string.Format("\u001b[42;1m");
        }

        private static string StylSearchResultExtra()
        {
            return string.Format("\u001b[41;1m");
        }

        private static string StylZatvorky()
        {
            return string.Format("\u001b[48;5;250m");
        }

        private static string StylVyberuTextu()
        {
            return string.Format("\u001b[1;37;44m");
        }

        public static string AnsiReset()
        {
            return "\u001b[0m";
        }

        public static string AnsiStyl(StylTextu styl)
        {
            switch (styl)
            {
                case StylTextu.Bold:
                    return "\u001b[1m";
                case StylTextu.Faint:
                    return "\u001b[2m";
                case StylTextu.Italic:
                    return "\u001b[3m";
                case StylTextu.Underline:
                    return "\u001b[4m";
                case StylTextu.FaintItalic:
                    return "\u001b[2;3m";
                case StylTextu.FaintBold:
                    return "\u001b[2;1m";
                case StylTextu.FaintBoldItalic:
                    return "\u001b[3;2;1m";
                case StylTextu.GreenItalic:
                    return "\u001b[3;32m";
                case StylTextu.OrangeBold:
                    return "\u001b[1;38;5;214m";
                case StylTextu.OrangeClassic:
                    return "\u001b[38;5;215m";
                case StylTextu.YellowItalic:
                    return "\u001b[3;38;5;226m";
                case StylTextu.Yellow:
                    return "\u001b[38;5;226m";
                case StylTextu.RedBold:
                    return "\u001b[1;38;5;196m";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public enum StylTextu
        {
            Standard,
            Bold,
            Faint,
            Italic,
            Underline,
            FaintBold,
            FaintItalic,
            GreenItalic,
            FaintBoldItalic,
            OrangeBold,
            OrangeClassic,
            YellowItalic,
            RedBold,
            Yellow
        }

        public static StylTextu VyberStyl(TypTokenu typ)
        {
            switch (typ)
            {
                case TypTokenu.KlucoveSlovo:
                    return StylTextu.OrangeBold;
                case TypTokenu.KlucovaFunkcia:
                    return StylTextu.OrangeClassic;
                case TypTokenu.Operator:
                case TypTokenu.Symbol:
                    return StylTextu.RedBold;
                case TypTokenu.Retazec:
                    return StylTextu.YellowItalic;
                case TypTokenu.Cislo:
                    return StylTextu.Yellow;
                case TypTokenu.Komentar:
                    return StylTextu.GreenItalic;
                default:
                    return StylTextu.Standard;
            }
        }
    }
}
