using PisaciStroj.Lexer;
using PisaciStroj.Lexer.Algoritmy;
using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using PSMat.Testy.Obrazovka;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Lexer
{
    public class BracketMatchingTest : ITest
    {
        public List<TestResult> Spust()
        {
            return new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "BracketMatchingTest",
                    Pass = BasicTest()
                }
            };
        }

        private bool BasicTest()
        {
            var algo = new StackBracketMatching();

            var stubText = StubText();

            var r = algo.GetMatchingBrackets(stubText);

            var rovnake = Porovnaj(r, Ocakavane());

            return rovnake;
        }

        private Dictionary<int, Dictionary<int, Zatvorka>> Ocakavane()
        {
            return new Dictionary<int, Dictionary<int, Zatvorka>>
            {
                { 0, new Dictionary<int, Zatvorka>()
                {
                    { 0, new Zatvorka()
                    {
                        Start = new Pozicia()
                        {
                            Riadok = 0,
                            Stlpec = 0
                        },
                        End = new Pozicia()
                        {
                            Riadok = 2,
                            Stlpec = 0
                        }
                    } }
                } },
                { 1, new Dictionary<int, Zatvorka>()
                {
                    { 1, new Zatvorka()
                    {
                        Start = new Pozicia()
                        {
                            Riadok = 1,
                            Stlpec = 1
                        },
                        End = new Pozicia()
                        {
                            Riadok = 1,
                            Stlpec = 2
                        }
                    } },
                    { 2, new Zatvorka()
                    {
                        Start = new Pozicia()
                        {
                            Riadok = 1,
                            Stlpec = 1
                        },
                        End = new Pozicia()
                        {
                            Riadok = 1,
                            Stlpec = 2
                        }
                    } }
                } },
                { 2, new Dictionary<int, Zatvorka>()
                {
                    { 0, new Zatvorka()
                    {
                        Start = new Pozicia()
                        {
                            Riadok = 0,
                            Stlpec = 0
                        },
                        End = new Pozicia()
                        {
                            Riadok = 2,
                            Stlpec = 0
                        }
                    } }
                } }
            };
        }

        private bool Porovnaj(Dictionary<int, Dictionary<int, Zatvorka>> r, Dictionary<int, Dictionary<int, Zatvorka>> p)
        {
            var rovnake = true;
            
            var pocetR = r.Count == p.Count;
            if (!pocetR)
            {
                return false;
            }

            foreach(var ra in r)
            {
                var pa = p[ra.Key];
                var pocetS = ra.Value.Count == pa.Count;

                if (!pocetS)
                {
                    rovnake = false;
                    break;
                }
                foreach(var sa in ra.Value)
                {
                    var x = pa[sa.Key];
                    var y = sa.Value;

                    if(x.Start.CompareTo(y.Start) != 0
                        || x.End.CompareTo(y.End) != 0)
                    {
                        rovnake = false;
                        break;
                    }
                }
            }

            return rovnake;
        }

        private List<GapBuffer> StubText()
        {
            return SyntaxHighlightStubs.RiadokPreBracketMatching();
        }
    }
}
