using PisaciStroj.Lexer.Algoritmy;
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

            return true;
        }

        private List<GapBuffer> StubText()
        {
            return SyntaxHighlightStubs.Riadky();
        }
    }
}
