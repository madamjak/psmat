using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using PisaciStroj.Vyhladavanie;
using PSMat.Testy.Lexer.Stubs;
using System.Collections.Generic;
using System.Linq;

namespace PSMat.Testy.Vyhladavanie
{
    public class RegexParserTest : ITest
    {
        private LexAutomat _lexer;

        public RegexParserTest()
        {
            var gramatika = LexerStubs.CmdLineGramatika();

            _lexer = new LexAutomat(gramatika);
        }
            
        public List<TestResult> Spust()
        {
            return new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "JednoduchyParseRegexTest",
                    Pass = JednoduchyParseRegexTest()
                },
                new TestResult()
                {
                    TestName = "ParseRegexTestRetazec",
                    Pass = ParseRegexTestRetazec()
                },
                new TestResult()
                {
                    TestName = "ParseRegexTestKlucoveSlova",
                    Pass = ParseRegexTestKlucoveSlova()
                }
            };
        }

        public bool ParseRegexTestKlucoveSlova()
        {
            var r = new GapBuffer();
            r.Append("fall re{\\d.\\s.\\w}");

            var t = new List<GapBuffer>() { r };

            var tokeny = _lexer.LexPrePrikazovyRiadok(t);

            var parts = tokeny.Tokeny.Values.ToList()[0].Values.ToList();

            var regexString = RegexParser.ParseRegex(tokeny, r, parts[1]);

            var ocakavane = "((1|2|3|4|5|6|7|8|9|0).( ).(q|w|e|r|t|y|u|i|o|p|a|s|d|f|g|h|j|k|l|z|x|c|v|b|n|m|Q|W|E|R|T|Y|U|I|O|P|A|S|D|F|G|H|J|K|L|Z|C|V|B|N|M|1|2|3|4|5|6|7|8|9|0)).\0";

            var pass = regexString == ocakavane;

            return pass;
        }

        public bool ParseRegexTestRetazec()
        {
            var r = new GapBuffer();
            r.Append("fall re{abcd}");

            var t = new List<GapBuffer>() { r };

            var tokeny = _lexer.LexPrePrikazovyRiadok(t);

            var parts = tokeny.Tokeny.Values.ToList()[0].Values.ToList();

            var regexString = RegexParser.ParseRegex(tokeny, r, parts[1]);

            var ocakavane = "(a.b.c.d).\0";

            var pass = regexString == ocakavane;

            return pass;
        }

        public bool JednoduchyParseRegexTest()
        {
            var r = new GapBuffer();
            r.Append("fall re{(a|b)*.c|d}");

            var t = new List<GapBuffer>() { r };

            var tokeny = _lexer.LexPrePrikazovyRiadok(t);

            var parts = tokeny.Tokeny.Values.ToList()[0].Values.ToList();

            var regexString = RegexParser.ParseRegex(tokeny, r, parts[1]);

            var ocakavane = "((a|b)*.c|d).\0";

            var pass = regexString == ocakavane;

            return pass;
        }
    }
}
