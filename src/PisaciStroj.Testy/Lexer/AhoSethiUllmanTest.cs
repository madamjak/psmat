using Lexer.Algoritmy;
using PisaciStroj.Lexer;
using PisaciStroj.Testy;
using PSMat.Testy.Lexer.Helpers;
using PSMat.Testy.Lexer.Stubs;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Lexer
{
    public class AhoSethiUllmanTest : ITest
    {
        public List<TestResult> Spust()
        {
            var r = new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "KonstrukciaJednoduchehoAutomatu",
                    Pass = KonstrukciaJednoduchehoAutomatu()
                },
                new TestResult()
                {
                    TestName = "KonstrukciaAutomatuAsociativita",
                    Pass = KonstrukciaAutomatuAsociativita()
                },
                new TestResult()
                {
                    TestName = "KonstrukciaAutomatu2",
                    Pass = KonstrukciaAutomatu2()
                }
            };

            return r;
        }

        public bool KonstrukciaAutomatu2()
        {
            var sethiUllman = new AhoSethiUllman();
            var ocakavanyAutomat = DfaAutomatonStubs.DruhyJednoduchyRegexAutomat2();

            var startTime = DateTime.Now;

            var vygenerovanyAutomat = sethiUllman.BuildDfa(new LexPravidlo
            {
                Regex = "((f.n.e.x.t)|(f.p.r.e.v)).\u0000"
            });

            var endTime = DateTime.Now;
            var diff1 = (endTime - startTime).TotalMilliseconds;

            if (diff1 > 100)
            {
                return false;
            }

            var spravne = DfaHelper.PorovnajAutomaty(ocakavanyAutomat, vygenerovanyAutomat);

            return spravne;
        }

        public bool KonstrukciaAutomatuAsociativita()
        {
            var sethiUllman = new AhoSethiUllman();

            var vygenerovanyAutomat1 = sethiUllman.BuildDfa(new LexPravidlo
            {
                Regex = "(((((f.n).e).x).t)|((((f.p).r).e).v)).\0"
            });

            var vygenerovanyAutomat2 = sethiUllman.BuildDfa(new LexPravidlo
            {
                Regex = "((f.(n.(e.(x.t))))|(f.(p.(r.(e.v))))).\0"
            });

            var rovnake = DfaHelper.PorovnajAutomaty(vygenerovanyAutomat1, vygenerovanyAutomat2);
            return rovnake;
        }

        private bool KonstrukciaJednoduchehoAutomatu()
        {
            var sethiUllman = new AhoSethiUllman();
            var ocakavanyAutomat = DfaAutomatonStubs.JednoduchyRegexAutomat();

            var startTime = DateTime.Now;

            var vygenerovanyAutomat = sethiUllman.BuildDfa(new LexPravidlo
            {
                Regex = "(a|b.c*).\0"
            });

            var endTime = DateTime.Now;
            var diff1 = (endTime - startTime).TotalMilliseconds;

            if (diff1 > 100)
            {
                return false;
            }

            var spravne = DfaHelper.PorovnajAutomaty(ocakavanyAutomat, vygenerovanyAutomat);

            return spravne;
        }
    }
}
