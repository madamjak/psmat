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
                VyskaKonzoly = 30,
                SirkaKonzoly = 120
            };
        }

        internal static ParametreVyhladavania VysledkyVyhladavania()
        {
            return new ParametreVyhladavania()
            {
                VyhladaneSlovo = new VyhladaneSlovo()
                {
                    Riadok = 5,
                    Pozicia = 4,
                    Dlzka = 6
                }
            };
        }

        internal static List<GapBuffer> CmdLinePrikaz()
        {
            var r = new GapBuffer();
            r.Append("saas \"C://temp/bla\" re{(a|b)*}");

            return new List<GapBuffer>()
            {
                r
            };
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
                "     * RegexAstTree",
                "     */",
                "    publicclass RegexAstTree",
                "    {",
                "",
                "        public Dictionary<int, HashSet<int>> FollowPos { get; private set; }",
                "//TODO make static?",
                "        private void BracketFun(int a, int b){",
                "        if (FollowPos.ContainsKey(a)){",
                "            vara abd = FollowPos[a].Contains(b)){",
                "            var x = (1 - (23 + 5) / 4 * (32 + a / (b - 1))))",
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

        internal static List<GapBuffer> RiadokPreBracketMatching()
        {
            var riadky = new List<GapBuffer>();

            var text = new List<string>()
            {
                "{",
                "(().(",
                "}"
            };

            foreach (var r in text)
            {
                var riadok = new GapBuffer();
                riadok.Append(r);
                riadky.Add(riadok);
            }

            return riadky;
        }
    }
}
