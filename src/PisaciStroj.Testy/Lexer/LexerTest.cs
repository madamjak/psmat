using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using PisaciStroj.Vyhladavanie;
using PSMat.Testy.Lexer.Helpers;
using PSMat.Testy.Lexer.Stubs;
using PSMat.Testy.Obrazovka;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
                Pass = BasicTest()
            });
            r.Add(new TestResult()
            {
                TestName = "VarVaraVarar",
                Pass = VarVaraVarar()
            });

            return r;
        }

        public bool VarVaraVarar()
        {
            var g = LexerStubs.CSharpGramatika();
            var s = LexerStubs.JednoduchyRiadok();

            var l = new LexAutomat(g);

            var gb = new GapBuffer();
            gb.Append(s.Text);

            var t = l.Lex(gb);

            var pass = TokensHelper.SuRovnakeTokeny(s.Tokens, t);

            return pass;
        }

        public bool BasicTest()
        {
            var g = LexerStubs.CmdLineGramatika();

            ILexer lexer = new LexAutomat(g);

            var gb = new GapBuffer();
            gb.Append("fnext \"lexer\"");

            var t = new List<GapBuffer>() { gb };

            var tokeny = lexer.LexZoZatvorkami(t);

            var ocakavane = new Dictionary<int, Token>
            {
                { 0, new Token()
                    {
                        Typ = TypTokenu.KlucoveSlovo,
                        Pozicia = 0,
                        Dlzka = 5
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
