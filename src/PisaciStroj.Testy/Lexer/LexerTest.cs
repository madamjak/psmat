using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using PSMat.Testy.Lexer.Helpers;
using PSMat.Testy.Lexer.Stubs;
using System.Collections.Generic;

namespace PSMat.Testy.Lexer
{
    public class LexerTest : ITest
    {
        public List<TestResult> Spust()
        {
            var r = new List<TestResult>();

            r.Add(new TestResult()
            {
                TestName = "JednoduchyLexerTest",
                Pass = JednoduchyLexerTest()
            });
            r.Add(new TestResult()
            {
                TestName = "VarVaraVarar",
                Pass = VarVaraVarar()
            });
            r.Add(new TestResult()
            {
                TestName = "CmdLineRegexTest",
                Pass = CmdLineRegexTest()
            });

            return r;
        }

        public bool CmdLineRegexTest()
        {
            var g = PisaciAutomat.Prikazy.GramatikaPrikazov.Gramatika();

            ILexer lexer = new LexAutomat(g);

            var gb = new GapBuffer();
            gb.Append("fnext re{(a|b)} \"lexer\" r{\"asdca");

            var t = new List<GapBuffer>() { gb };

            var tokeny = lexer.LexPrePrikazovyRiadok(t);

            var ocakavane = new Dictionary<int, Token>
            {
                { 0, new Token()
                    {
                        Typ = TypTokenu.KlucoveSlovo,
                        Pozicia = 0,
                        Dlzka = 6
                    } },
                { 6, new Token()
                    {
                        Typ = TypTokenu.Regex,
                        Pozicia = 6,
                        Dlzka = 9
                    } },
                { 16, new Token()
                    {
                        Typ = TypTokenu.Retazec,
                        Pozicia = 16,
                        Dlzka = 7
                    } },
            };

            var pass = TokensHelper.SuRovnakeTokeny(tokeny.Tokeny[0], ocakavane);

            var ocakavaneRegex = new Dictionary<int, Token>
            {
                { 10, new Token()
                    {
                        Typ = TypTokenu.Retazec,
                        Pozicia = 10,
                        Dlzka = 1
                    } },
                { 11, new Token()
                    {
                        Typ = TypTokenu.Operator,
                        Pozicia = 11,
                        Dlzka = 1
                    } },
                { 12, new Token()
                    {
                        Typ = TypTokenu.Retazec,
                        Pozicia = 12,
                        Dlzka = 1
                    } },
            };

            var passRegex = TokensHelper.SuRovnakeTokeny(tokeny.RegexTokeny[0], ocakavaneRegex);

            return pass && passRegex;
        }

        public bool EditorLineRegexTest()
        {
            var g = LexerStubs.CSharpGramatika();
            ILexer lexer = new LexAutomat(g);

            var gb = new GapBuffer();
            gb.Append("publicnext \\\\(a|b)\\\\ \"lexer\" \"asdca");

            var tokeny = lexer.LexPreEditor(gb);

            var ocakavane = new Dictionary<int, Token>
            {
                { 0, new Token()
                    {
                        Typ = TypTokenu.Identifikator,
                        Pozicia = 0,
                        Dlzka = 10
                    } },
                { 14, new Token()
                    {
                        Typ = TypTokenu.Identifikator,
                        Pozicia = 14,
                        Dlzka = 1
                    } },
                { 16, new Token()
                    {
                        Typ = TypTokenu.Identifikator,
                        Pozicia = 16,
                        Dlzka = 1
                    } },
                { 21, new Token()
                    {
                        Typ = TypTokenu.Retazec,
                        Pozicia = 21,
                        Dlzka = 7
                    } },
                { 29, new Token()
                    {
                        Typ = TypTokenu.Retazec,
                        Pozicia = 29,
                        Dlzka = 6
                    } },
            };

            var pass = TokensHelper.SuRovnakeTokeny(tokeny, ocakavane);

            return pass;
        }

        public bool VarVaraVarar()
        {
            var g = LexerStubs.CSharpGramatika();
            var s = LexerStubs.JednoduchyRiadok();

            var l = new LexAutomat(g);

            var gb = new GapBuffer();
            gb.Append(s.Text);

            var t = l.LexPreEditor(gb);

            var pass = TokensHelper.SuRovnakeTokeny(s.Tokens, t);

            return pass;
        }

        public bool JednoduchyLexerTest()
        {
            var g = PisaciAutomat.Prikazy.GramatikaPrikazov.Gramatika();

            ILexer lexer = new LexAutomat(g);

            var gb = new GapBuffer();
            gb.Append("fnext \"lexer\" \"lexer");

            var t = new List<GapBuffer>() { gb };

            var tokeny = lexer.LexPrePrikazovyRiadok(t);

            var ocakavane = new Dictionary<int, Token>
            {
                { 0, new Token()
                    {
                        Typ = TypTokenu.KlucoveSlovo,
                        Pozicia = 0,
                        Dlzka = 6
                    } },
                { 6, new Token()
                    {
                        Typ = TypTokenu.Retazec,
                        Pozicia = 6,
                        Dlzka = 7
                    } },
            };

            var pass = TokensHelper.SuRovnakeTokeny(tokeny.Tokeny[0], ocakavane);

            return pass;
        }
    }
}
