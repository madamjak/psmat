using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Vyhladavanie
{
    public class RegexVyhladavanie : ITest
    {
        private ILexer _lexer;

        public RegexVyhladavanie()
        {
            _lexer = new LexAutomat();
        }

        public List<TestResult> Spust()
        {
            return new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "RegexVyhladavanie.Test1",
                    Pass = Test1()
                },
                new TestResult()
                {
                    TestName = "RegexVyhladavanie.Test2",
                    Pass = Test2()
                },
                new TestResult()
                {
                    TestName = "RegexVyhladavanie.Test3",
                    Pass = Test3()
                },
                new TestResult()
                {
                    TestName = "RegexVyhladavanie.Test4",
                    Pass = Test4()
                },
            };
        }

        public bool Test4()
        {
            var algo = new VyhladavaciAutomat();

            var text = "alskdc sdsaad asdkc aks99dcm ckadlkc sdsaa asdkc aks     asjkdc";
            var gb = new GapBuffer();
            gb.Append(text);
            var riadky = new List<GapBuffer>() { gb };

            var cmd = new GapBuffer();
            cmd.Append("re{\\w*\\ssdsaad* asdkc aks\\d*\\s*as}");

            var tokeny = _lexer.LexPrePrikazovyRiadok(new List<GapBuffer>() { cmd });

            var regexParsed = RegexParser.ParseRegex(tokeny, cmd, tokeny.Tokeny[0][0]);

            algo.NastavRegexVyhladavanie(regexParsed);

            var result = algo.VyhladajVsetky(riadky, regexParsed);

            var ocakavanyPocet = result.PocetNajdenychSlov == 1;
            var ocakavaneSlova = new Dictionary<int, VyhladaneSlovo>()
            {
                { 29, new VyhladaneSlovo() { Pozicia = 29, Dlzka = 30 } }
            };

            var pass = VyhladavacTestHelper.RovnakeVysledky(result.Slova[0], ocakavaneSlova);

            return ocakavanyPocet && pass;
        }

        public bool Test3()
        {
            var algo = new VyhladavaciAutomat();

            var text = "alskdc sdsaad asdkc aks99dcm ckadlkc sdsaa asdkc aks asjkdc";
            var gb = new GapBuffer();
            gb.Append(text);
            var riadky = new List<GapBuffer>() { gb };

            var cmd = new GapBuffer();
            cmd.Append("re{\\w*\\ssdsaad* asdkc aks\\d*}");

            var tokeny = _lexer.LexPrePrikazovyRiadok(new List<GapBuffer>() { cmd });

            var regexParsed = RegexParser.ParseRegex(tokeny, cmd, tokeny.Tokeny[0][0]);

            algo.NastavRegexVyhladavanie(regexParsed);

            var result = algo.VyhladajVsetky(riadky, regexParsed);

            var ocakavanyPocet = result.PocetNajdenychSlov == 2;
            var ocakavaneSlova = new Dictionary<int, VyhladaneSlovo>()
            {
                { 0, new VyhladaneSlovo() { Pozicia = 0, Dlzka = 25 } },
                { 29, new VyhladaneSlovo() { Pozicia = 29, Dlzka = 23 } }
            };

            var pass = VyhladavacTestHelper.RovnakeVysledky(result.Slova[0], ocakavaneSlova);

            return ocakavanyPocet && pass;
        }

        public bool Test2()
        {
            var algo = new VyhladavaciAutomat();

            var text = "alskdc sdsaada asdkc aks99dcm ckadlkc sdsaada asdkc aks asjkdc";
            var gb = new GapBuffer();
            gb.Append(text);
            var riadky = new List<GapBuffer>() { gb };

            var cmd = new GapBuffer();
            cmd.Append("re{\\w*\\ssdsaada asdkc aks\\d*}");

            var tokeny = _lexer.LexPrePrikazovyRiadok(new List<GapBuffer>() { cmd });

            var regexParsed = RegexParser.ParseRegex(tokeny, cmd, tokeny.Tokeny[0][0]);

            algo.NastavRegexVyhladavanie(regexParsed);

            var result = algo.VyhladajVsetky(riadky, regexParsed);

            var ocakavanyPocet = result.PocetNajdenychSlov == 2;
            var ocakavaneSlova = new Dictionary<int, VyhladaneSlovo>()
            {
                { 0, new VyhladaneSlovo() { Pozicia = 0, Dlzka = 26 } },
                { 30, new VyhladaneSlovo() { Pozicia = 30, Dlzka = 25 } }
            };

            var pass = VyhladavacTestHelper.RovnakeVysledky(result.Slova[0], ocakavaneSlova);

            return ocakavanyPocet && pass;
        }

        public bool Test1()
        {
            var algo = new VyhladavaciAutomat();

            var text = "alskdc sdsaada asdkc aksdcm ckadlkc sdsaada asdkc aks asjkdc";
            var gb = new GapBuffer();
            gb.Append(text);
            var riadky = new List<GapBuffer>() { gb };

            var cmd = new GapBuffer();
            cmd.Append("re{\\w\\ssdsaada asdkc aks}");

            var tokeny = _lexer.LexPrePrikazovyRiadok(new List<GapBuffer>() { cmd });

            var regexParsed = RegexParser.ParseRegex(tokeny, cmd, tokeny.Tokeny[0][0]);

            algo.NastavRegexVyhladavanie(regexParsed);

            var result = algo.VyhladajVsetky(riadky, regexParsed);

            var ocakavanyPocet = result.PocetNajdenychSlov == 2;
            var ocakavaneSlova = new Dictionary<int, VyhladaneSlovo>()
            {
                { 5, new VyhladaneSlovo() { Pozicia = 5, Dlzka = 19 } },
                { 34, new VyhladaneSlovo() { Pozicia = 34, Dlzka = 19 } }
            };

            var pass = VyhladavacTestHelper.RovnakeVysledky(result.Slova[0], ocakavaneSlova);

            return ocakavanyPocet && pass;
        }
    }
}
