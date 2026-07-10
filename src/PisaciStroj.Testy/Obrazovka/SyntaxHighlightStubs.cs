using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Parametre;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PisaciStroj.Vyhladavanie;
using PisaciStroj.Lexer.Algoritmy;

namespace PSMat.Testy.Obrazovka
{
    public static class SyntaxHighlightStubs
    {
        internal static ParametreVypisu ParametreVypisu()
        {
            return new ParametreVypisu()
            {
                OkrajVlavo = 5,
                OkrajHore = 2,
                OkrajDole = 2,
                VyskaKonzoly = 20,
                SirkaKonzoly = 120
            };
        }

        internal static ParametreVyhladavania VysledkyVyhladavania()
        {
            return new ParametreVyhladavania()
            {
                VyhladaneSlova = new Dictionary<int, Dictionary<int, VyhladaneSlovo>>() 
                {
                    { 5, new Dictionary<int, VyhladaneSlovo> 
                        { 
                            { 4, new VyhladaneSlovo()
                                {
                                    Riadok = 5,
                                    Pozicia = 4,
                                    Dlzka = 6
                                }
                            } 
                        } 
                    }
                },
                VyhladaneSlovo = new VyhladaneSlovo()
                {
                    Riadok = 5,
                    Pozicia = 4,
                    Dlzka = 6
                }
            };
        }

        internal static LexResult Tokeny()
        {
            var r = new LexResult()
            {
                Tokeny = new Dictionary<int, Dictionary<int, Token>>(),
                Zatvorky = new Dictionary<int, Dictionary<int, Zatvorka>>()
            };

            var tokeny = new List<List<Token>>()
            {
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Dlzka = 9 }
                },
                new List<Token>(),
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.Komentar, Pozicia = 4, Dlzka = 2}
                },
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.Komentar, Dlzka = 113 }
                },
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.Komentar, Dlzka = 7 }
                },
                new List<Token>(),
                new List<Token>(),
                new List<Token>(),
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 8, Dlzka = 6 },
                    new Token() { Typ = TypTokenu.KlucovaFunkcia, Pozicia = 15, Dlzka = 10 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 26, Dlzka = 3 },
                    new Token() { Typ = TypTokenu.KlucovaFunkcia, Pozicia = 31, Dlzka = 7 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 39, Dlzka = 3 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 57, Dlzka = 3 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 62, Dlzka = 7 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 70, Dlzka = 3 },
                },
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.Komentar, Dlzka = 19 }
                },
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 8, Dlzka = 7 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 16, Dlzka = 6 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 23, Dlzka = 4 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 39, Dlzka = 3 },
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 46, Dlzka = 3 }
                },
                new List<Token>()
                {
                    new Token() { Typ = TypTokenu.KlucoveSlovo, Pozicia = 12, Dlzka = 3 }
                },
            };

            for(int i = 0; i < tokeny.Count; i++)
            {
                r.Tokeny.Add(i, tokeny[i].ToDictionary(x => x.Pozicia, y => y));
            }

            r.Zatvorky = StubZatvorky();
            return r;
        }

        private static Dictionary<int, Dictionary<int, Zatvorka>> StubZatvorky()
        {
            return new StackBracketMatching().GetMatchingBrackets(Riadky());
        }

        internal static List<GapBuffer> Riadky()
        {
            var riadky = new List<GapBuffer>();

            var text = new List<string>()
            {
                "namespace ConsoleApp1",
                "{",
                "    /*",
                "     * RegexAstTree is useful to construct deterministic finite automaton accepting language generated by regex",
                "     */",
                "    publicclass RegexAstTree",
                "    {",
                "",
                "        public Dictionary<int, HashSet<int>> FollowPos { get; private set; }",
                "//TODO make static?",
                "        private static void BracketFun(int a, int b){",
                "            var x = (1 - (23 + 5) / 4 + (32 + a / (b - 1))))",
                "        }",
                "    }",
                "}}}"
            };

            foreach(var r in text)
            {
                var riadok = new GapBuffer();
                riadok.Append(r);
                riadky.Add(riadok);
            }
            
            return riadky;
        }
    }
}
