using Lexer.Algoritmy;
using PisaciStroj.Testy;
using PSMat.Testy.Lexer.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Lexer
{
    public class RegexAstTreeTest : ITest
    {
        public List<TestResult> Spust()
        {
            var t = new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "OtestujJednoduchyRegexAst",
                    Pass = OtestujJednoduchyRegexAst()
                },
                new TestResult()
                {
                    TestName = "OtestujJednoduchyRegexAst2",
                    Pass = OtestujJednoduchyRegexAst2()
                },
                new TestResult()
                {
                    TestName = "OtestujJednoduchyRegexAstNullableUzly",
                    Pass = OtestujJednoduchyRegexAstNullableUzly()
                },
                new TestResult()
                {
                    TestName = "OtestujJednoduchyRegexAstWithPos",
                    Pass = OtestujJednoduchyRegexAstWithPos()
                },
                new TestResult()
                {
                    TestName = "OtestujFollowPos",
                    Pass = OtestujFollowPos()
                },
                new TestResult()
                {
                    TestName = "OtestujFollowPos2",
                    Pass = OtestujFollowPos2()
                }
            };

            return t;
        }

        public bool OtestujJednoduchyRegexAst2()
        {
            var algo = new ShuntingYard();
            var regex = "((f.n.e.x.t)|(f.p.r.e.v)).\u0000";


            var ocakavany = "((((f).((n).((e).((x).(t)))))|((f).((p).((r).((e).(v)))))).(\0))";

            var tree = algo.BuildTree(regex);

            var sb = new StringBuilder();
            RegexAstTreeHelper.InorderTraversalPrint(tree.Root, sb);

            var actual = sb.ToString();

            return ocakavany == actual;
        }

        public bool OtestujJednoduchyRegexAst()
        {
            var algo = new ShuntingYard();
            var regex = "((/.\\*).(q|w|e|r|t|y)*.(\\*./)).\u0000";
            var ocakavany = "((((/).(*)).((((q)|((w)|((e)|((r)|((t)|(y))))))*).((*).(/)))).(\0))";

            var tree = algo.BuildTree(regex);
            var sb = new StringBuilder();
            RegexAstTreeHelper.InorderTraversalPrint(tree.Root, sb);

            var actual = sb.ToString();

            return ocakavany == actual;
        }

        public bool OtestujJednoduchyRegexAstNullableUzly()
        {
            var algo = new ShuntingYard();
            var regex = "a|b.c*";
            var ocakavany = "[[a]|[[b].[[c]*-nullable]]]";

            var tree = algo.BuildTree(regex);

            var sb = new StringBuilder();
            RegexAstTreeHelper.InorderTraversalNullablePrint(tree.Root, sb);

            var actual = sb.ToString();

            return ocakavany == actual;
        }

        public bool OtestujJednoduchyRegexAstWithPos()
        {
            var algo = new ShuntingYard();
            var regex = "a|b.c*";
            var ocakavany = "[[position: 1; firstPos: 1, lastPos: 1, ]firstPos: 1, 2, lastPos: 1, 3, 2, [[position: 2; firstPos: 2, lastPos: 2, ]firstPos: 2, lastPos: 3, 2, [[position: 3; firstPos: 3, lastPos: 3, ]firstPos: 3, lastPos: 3, ]]]";

            var tree = algo.BuildTree(regex);
            tree.PostorderTraversalToConstructFollowpos();

            var sb = new StringBuilder();
            RegexAstTreeHelper.InorderTraversalPositionsPrint(tree.Root, sb);

            var actual = sb.ToString();

            return ocakavany == actual;
        }

        public bool OtestujFollowPos()
        {
            var algo = new ShuntingYard();
            var regex = "(a|b.c*).\0";
            var ocakavanySymbolPos = new Dictionary<int, char>()
            {
                { 1, 'a' },
                { 2, 'b' },
                { 3, 'c' },
                { 4, '\0' }
            };

            var ocakavanyFollowPos = new Dictionary<int, HashSet<int>>()
            {
                { 1, new HashSet<int> { 4 } },
                { 2, new HashSet<int> { 3, 4 } },
                { 3, new HashSet<int> { 3, 4 } },
            };

            
            var tree = algo.BuildTree(regex);
            tree.PostorderTraversalToConstructFollowpos();

            var spravne = RegexAstTreeHelper.PorovnajFollowPos(tree, ocakavanySymbolPos, ocakavanyFollowPos);

            return spravne;
        }

        public bool OtestujFollowPos2()
        {
            var algo = new ShuntingYard();
            var regex = "((f.n.e.x.t)|(f.p.r.e.v)).\u0000";
            var ocakavanySymbolPos = new Dictionary<int, char>()
            {
                { 1, 'f' },
                { 2, 'n' },
                { 3, 'e' },
                { 4, 'x' },
                { 5, 't' },
                { 6, 'f' },
                { 7, 'p' },
                { 8, 'r' },
                { 9, 'e' },
                { 10, 'v' },
                { 11, '\0' }
            };

            var ocakavanyFollowPos = new Dictionary<int, HashSet<int>>()
            {
                { 1, new HashSet<int> { 2 } },
                { 2, new HashSet<int> { 3 } },
                { 3, new HashSet<int> { 4 } },
                { 4, new HashSet<int> { 5 } },
                { 5, new HashSet<int> { 11 } },
                { 6, new HashSet<int> { 7 } },
                { 7, new HashSet<int> { 8 } },
                { 8, new HashSet<int> { 9 } },
                { 9, new HashSet<int> { 10 } },
                { 10, new HashSet<int> { 11 } },
            };

            var tree = algo.BuildTree(regex);
            tree.PostorderTraversalToConstructFollowpos();

            var spravne = RegexAstTreeHelper.PorovnajFollowPos(tree, ocakavanySymbolPos, ocakavanyFollowPos);

            return spravne;
        }
    }
}
