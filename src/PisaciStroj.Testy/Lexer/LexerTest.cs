using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using PisaciStroj.Vyhladavanie;
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

            return r;
        }

        public bool BasicTest()
        {
            var g = LexerStubs.CmdLineGramatika();

            ILexer lexer = new LexAutomat(g);

            var gb = new GapBuffer();
            gb.Append("fnext \"lexer\"");

            var t = new List<GapBuffer>() { gb };

            var tokeny = lexer.Lex(t);

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

            var pass = true;
            if(tokeny.Tokeny.Count != 1)
            {
                pass = false;
            }

            if (tokeny.Tokeny[0][0].Typ != ocakavane[0].Typ
                || tokeny.Tokeny[0][0].Pozicia != ocakavane[0].Pozicia
                || tokeny.Tokeny[0][0].Dlzka != ocakavane[0].Dlzka


                || tokeny.Tokeny[0][6].Typ != ocakavane[6].Typ
                || tokeny.Tokeny[0][6].Pozicia != ocakavane[6].Pozicia
                || tokeny.Tokeny[0][6].Dlzka != ocakavane[6].Dlzka) 
            {
                pass = false;
            }

            return pass;
        }
    }
}
