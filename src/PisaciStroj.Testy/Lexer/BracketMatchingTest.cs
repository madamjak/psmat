using PisaciStroj.Lexer.Algoritmy;
using PisaciStroj.Pamat;
using PSMat.Testy.Obrazovka;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Lexer
{
    public class BracketMatchingTest
    {
        public void Spust()
        {
            BasicTest();
        }

        private void BasicTest()
        {
            var algo = new StackBracketMatching();

            var stubText = StubText();

            var r = algo.GetMatchingBrackets(stubText);
        }

        private List<GapBuffer> StubText()
        {
            return SyntaxHighlightStubs.Riadky();
        }
    }
}
