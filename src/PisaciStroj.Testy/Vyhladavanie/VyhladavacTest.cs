using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using PisaciStroj.Vyhladavanie;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Vyhladavanie
{
    public class VyhladavacTest : ITest
    {
        public List<TestResult> Spust()
        {
            return new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "JednoduchyTestVyhladavaca",
                    Pass = JednoduchyTestVyhladavaca()
                },
                new TestResult()
                {
                    TestName = "JednoduchyTestVyhladavacaVsetky",
                    Pass = JednoduchyTestVyhladavacaVsetky()
                },
                new TestResult()
                {
                    TestName = "JednoduchyTestRegexVyhladavaca",
                    Pass = JednoduchyTestRegexVyhladavaca()
                },
            };
        }

        public bool JednoduchyTestRegexVyhladavaca()
        {
            var text = "aaabbcbcab";
            var gb = new GapBuffer();
            gb.Append(text);

            var retazecNaVyhladanie = "((a.a*).b|c).\0";
            var gb2 = new GapBuffer();
            gb2.Append(retazecNaVyhladanie);

            IVyhladavac algo = new VyhladavaciAutomat();
            algo.NastavRegexVyhladavanie(retazecNaVyhladanie);

            var slova = algo.VyhladajVsetky(gb, retazecNaVyhladanie);

            var ocakavaneSlova = new Dictionary<int, VyhladaneSlovo>()
            {
                { 0, new VyhladaneSlovo() { Pozicia = 0, Dlzka = 4 } },
                { 5, new VyhladaneSlovo() { Pozicia = 5, Dlzka = 1 } },
                { 7, new VyhladaneSlovo() { Pozicia = 7, Dlzka = 1 } },
                { 8, new VyhladaneSlovo() { Pozicia = 8, Dlzka = 2 } }
            };

            var pass = VyhladavacTestHelper.RovnakeVysledky(slova, ocakavaneSlova);

            return pass;
        }

        public bool JednoduchyTestVyhladavacaVsetky()
        {
            var text = "aabaabaabaaskdcaskdjcnaksjdncakjsdncakdjsncaksjdncaabaabaabacalsdkmalsdkmfalskdmfasldkmfabc";
            var gb = new GapBuffer();
            gb.Append(text);

            var retazecNaVyhladanie = "aabaaba";

            IVyhladavac algo = new VyhladavaciAutomat();
            algo.NastavVyhladavanie(retazecNaVyhladanie);

            var slova = algo.VyhladajVsetky(gb, retazecNaVyhladanie);

            if (!(slova.Count == 2))
            {
                return false;
            }
            if (!(slova[0].Pozicia == 0 && slova[0].Dlzka == 7
                && slova[50].Pozicia == 50 && slova[50].Dlzka == 7))
            {
                return false;
            }

            return true;
        }

        public bool JednoduchyTestVyhladavaca()
        {
            var text = "aabaabaabaaskdcaskdjcnaksjdncakjsdncakdjsncaksjdncaabaabaabacalsdkmalsdkmfalskdmfasldkmfabc";
            var gb = new GapBuffer();
            gb.Append(text);

            var retazecNaVyhladanie = "aabaaba";

            IVyhladavac algo = new VyhladavaciAutomat();
            algo.NastavVyhladavanie(retazecNaVyhladanie);

            var slovo = algo.VyhladajNasledujuci(gb, 0, retazecNaVyhladanie);

            var pass = slovo.HasValue && slovo.Value.Pozicia == 0 && slovo.Value.Dlzka == 7;

            return pass;
        }
    }
}
