
using PisaciStroj.Lexer;
using System;

namespace PisaciAutomat.Config
{
    public static class Farby
    {
        public static bool DarkMode { get; set; }

        public static bool BracketHighlighted { get; set; }

        public static string StylSearchResult()
        {
            if (DarkMode)
            {
                return string.Format("\u001b[42;1m");
            }
            else
            {
                return string.Format("\u001b[97;102;1m");
            }
        }

        public static string StylSearchResultExtra()
        {
            if (DarkMode)
            {
                return string.Format("\u001b[41;1m");
            }
            else
            {
                return string.Format("\u001b[97;101;1m");
            }
        }

        public static string AnsiReset2()
        {
            return "\u001b[0m";
        }

        public static string AnsiReset(FarbaPozadia? p = null)
        {
            if (p.HasValue)
            {
                return string.Format("\u001b[0m{0}", AnsiStyl(p.Value));
            }
            if (DarkMode)
            {
                return "\u001b[0m\u001b[37;48;5;234m";
            }
            else
            {
                return "\u001b[0m\u001b[30;48;5;231m";
            }
        }

        public enum FarbaPozadia
        {
            Zlta,
            Cyan,
            Siva,
            Modra,
            Cervena,
            Zelena,
            Biela,
            CervenaLight,
            CiernaDark,
            Cierna
        }

        public static string AnsiStyl(FarbaPozadia p)
        {
            switch (p)
            {
                case FarbaPozadia.Zlta:
                    return "\u001b[1;90;103m";
                case FarbaPozadia.Cyan:
                    return "\u001b[1;90;106m";
                case FarbaPozadia.Siva:
                    return "\u001b[48;5;236m";
                case FarbaPozadia.Biela:
                    return "\u001b[1;90;107m";
                case FarbaPozadia.Modra:
                    return "\u001b[44m";
                case FarbaPozadia.Cervena:
                    return "\u001b[41;1m";
                case FarbaPozadia.Zelena:
                    return "\u001b[42;1m";
                case FarbaPozadia.CervenaLight:
                    return "\u001b[48;5;124m";
                case FarbaPozadia.CiernaDark:
                    return "\u001b[48;5;232m";
                case FarbaPozadia.Cierna:
                    return "\u001b[40m";
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                case StylTextu.Green:
                    return "\u001b[32m";
                case StylTextu.OrangeBold:
                    return "\u001b[1;38;5;214m";
                case StylTextu.OrangeClassic:
                    return "\u001b[38;5;214m";
                case StylTextu.YellowItalic:
                    return "\u001b[3;38;5;226m";
                case StylTextu.Yellow:
                    return "\u001b[38;5;226m";
                case StylTextu.RedBold:
                    return "\u001b[1;38;5;196m";
                case StylTextu.Red:
                    return "\u001b[31m";
                case StylTextu.BrightRed:
                    return "\u001b[91m";
                case StylTextu.Red196:
                    return "\u001b[38;5;196m";
                case StylTextu.Cyan:
                    return "\u001b[38;5;87m";
                case StylTextu.CyanBold:
                    return "\u001b[1;38;5;87m";
                case StylTextu.NejakaZelena:
                    return "\u001b[38;5;78m";
                case StylTextu.NejakaZelenaBold:
                    return "\u001b[1;38;5;78m";
                case StylTextu.Blue:
                    return "\u001b[38;5;27m";
                case StylTextu.Biela:
                    return "\u001b[38;5;15m";
                case StylTextu.Fialova:
                    return "\u001b[38;5;128m";
                case StylTextu.Cierna:
                    return "\u001b[38;5;16m";
                case StylTextu.Siva:
                    return "\u001b[38;5;241m";
                case StylTextu.Siva2:
                    return "\u001b[38;5;248m";
                case StylTextu.NejakaZelena2:
                    return "\u001b[38;5;46m";
                case StylTextu.NejakaCervena:
                    return "\u001b[38;5;202m";
                case StylTextu.NejakaOranzovaCiZlta:
                    return "\u001b[38;5;178m";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static string StylKurzora()
        {
            return "\u001b[1 q";
        }

        public enum StylTextu
        {
            Standard,
            Bold,
            Faint,
            Italic,
            Underline,
            FaintBold,
            Siva,
            Siva2,
            FaintItalic,
            GreenItalic,
            Green,
            FaintBoldItalic,
            OrangeBold,
            OrangeClassic,
            YellowItalic,
            RedBold,
            Red,
            Yellow,
            Cyan,
            CyanBold,
            Blue,
            Biela,
            Fialova,
            Cierna,
            NejakaZelena,
            NejakaZelenaBold,
            NejakaZelena2,
            NejakaCervena,
            NejakaOranzovaCiZlta,
            BrightRed,
            Red196
        }

        public static StylTextu VyberStylRegex(TypTokenu typ)
        {
            if (DarkMode)
            {
                switch (typ)
                {
                    case TypTokenu.Operator:
                        return StylTextu.CyanBold;
                    case TypTokenu.KlucoveSlovo:
                        return StylTextu.OrangeBold;
                    default:
                        return VyberStyl(TypTokenu.Regex);
                }
            }
            else
            {
                switch (typ)
                {
                    case TypTokenu.Operator:
                        return StylTextu.Blue;
                    case TypTokenu.KlucoveSlovo:
                        return StylTextu.RedBold;
                    default:
                        return VyberStyl(TypTokenu.Regex);
                }
            }
        }

        public static StylTextu VyberStyl(TypTokenu typ)
        {
            if (DarkMode)
            {
                switch (typ)
                {
                    case TypTokenu.KlucoveSlovo:
                        return StylTextu.Cyan;
                    case TypTokenu.KlucovaFunkcia:
                        return StylTextu.NejakaZelena;
                    case TypTokenu.Operator:
                    case TypTokenu.Symbol:
                        return StylTextu.Standard;
                    case TypTokenu.Retazec:
                        return StylTextu.OrangeClassic;
                    case TypTokenu.Cislo:
                        return StylTextu.Yellow;
                    case TypTokenu.Komentar:
                        return StylTextu.GreenItalic;
                    case TypTokenu.Regex:
                        return StylTextu.NejakaZelena2;
                    default:
                        return StylTextu.Standard;
                }
            }
            else
            {
                switch (typ)
                {
                    case TypTokenu.KlucoveSlovo:
                        return StylTextu.Blue;
                    case TypTokenu.KlucovaFunkcia:
                        return StylTextu.Fialova;
                    case TypTokenu.Operator:
                    case TypTokenu.Symbol:
                        return StylTextu.Standard;
                    case TypTokenu.Retazec:
                        return StylTextu.NejakaCervena;
                    case TypTokenu.Cislo:
                        return StylTextu.OrangeClassic;
                    case TypTokenu.Komentar:
                        return StylTextu.GreenItalic;
                    case TypTokenu.Regex:
                        return StylTextu.Green;
                    default:
                        return StylTextu.Standard;
                }
            }
        }

        internal static string StylZatvorky()
        {
            if (DarkMode)
            {
                return string.Format("{0}", AnsiStyl(StylTextu.Red196));
            }
            else
            {
                return string.Format("{0}", AnsiStyl(StylTextu.Red196));
            }
        }

        public static string StylCislaRiadkov(string cislo)
        {
            if (DarkMode)
            {
                return string.Format("{0}{1}{2}  \u001b[0m", AnsiReset(), AnsiStyl(StylTextu.Faint), cislo);
            }
            else
            {
                return string.Format("{0}{1}{2}  \u001b[0m", AnsiReset(), AnsiStyl(StylTextu.Siva2), cislo);
            }
            
        }

        public static FarbaPozadia FarbaPrikazRiadku()
        {
            if (DarkMode)
            {
                return FarbaPozadia.Siva;
            }
            else
            {
                return FarbaPozadia.Biela;
            }
        }

        public static StylTextu FarbaIndikatoraPrikazRiadku()
        {
            if (DarkMode)
            {
                return StylTextu.Biela;
            }
            else
            {
                return StylTextu.Cierna;
            }
        }

        public static FarbaPozadia FarbaVysledkov()
        {
            if (DarkMode)
            {
                return FarbaPozadia.Modra;
            }
            else
            {
                return FarbaPozadia.Zlta;
            }
        }

        public static string Info(string hlaska)
        {
            if (DarkMode)
            {
                return string.Format("\u001b[44;1m{0}\u001b[0m{1}{2} {3} \u001b[0m", " i ",
                Farby.AnsiStyl(Farby.FarbaPozadia.Siva),
                Farby.AnsiStyl(Farby.StylTextu.Biela),
                hlaska);
            }
            else
            {
                return string.Format("\u001b[104m{0}\u001b[0m{1}{2} {3} \u001b[0m", " i ",
                Farby.AnsiStyl(Farby.FarbaPozadia.Biela),
                Farby.AnsiStyl(Farby.StylTextu.Cierna),
                hlaska);
            }
        }

        public static string Dialog(string hlaska)
        {
            if (DarkMode)
            {
                return string.Format("\u001b[42;1m{0}\u001b[0m{1}{2} {3} \u001b[0m", " ? ",
                Farby.AnsiStyl(Farby.FarbaPozadia.Siva),
                Farby.AnsiStyl(Farby.StylTextu.Biela),
                hlaska);
            }
            else
            {
                return string.Format("\u001b[102m{0}\u001b[0m{1}{2} {3} \u001b[0m", " ? ",
                Farby.AnsiStyl(Farby.FarbaPozadia.Biela),
                Farby.AnsiStyl(Farby.StylTextu.Cierna),
                hlaska);
            }
        }

        public static string Chyba(string hlaska)
        {
            if (DarkMode)
            {
                return string.Format("\u001b[41;1m{0}\u001b[0m{1}{2} {3} \u001b[0m", " ! ",
                Farby.AnsiStyl(Farby.FarbaPozadia.Siva),
                Farby.AnsiStyl(Farby.StylTextu.Biela),
                hlaska);
            }
            else
            {
                return string.Format("\u001b[101m{0}\u001b[0m{1}{2} {3} \u001b[0m", " ! ",
                Farby.AnsiStyl(Farby.FarbaPozadia.Biela),
                Farby.AnsiStyl(Farby.StylTextu.Cierna),
                hlaska);
            }
        }
    }
}
