using PSMat.Testy.Lexer;
using PSMat.Testy.Obrazovka;
using PSMat.Testy.Pamat;
using PSMat.Testy.PrikazovyRiadok;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PisaciStroj.Testy
{
    public class TestResult
    {
        public string TestName { get; set; }

        public bool Pass { get; set; }
    }

    public interface ITest
    {
        List<TestResult> Spust();
    }

    class Program
    {
        static int Main(string[] args)
        {
            var results = new List<TestResult>();
            //var testy = new PrikazovyRiadokTesty();
            //testy.Spust();

            //var testy = new GapBufferTest();
            //testy.Spust();

            //var t = new SyntaxHighlightTest();
            //t.Spust();

            //var t = new BracketMatchingTest();
            //t.Spust();

            var testy = new List<ITest>() 
            {
                new LexerTest(),
                new BracketMatchingTest()
            };

            foreach(var t in testy)
            {
                results.AddRange(t.Spust());
            }
            
            var failed = results.Where(x => !x.Pass).ToList();
            var passed = results.Count - failed.Count;

            Console.WriteLine("*** TEST SUMMARY ***");
            Console.WriteLine(string.Format("Total tests: {0}", results.Count));
            Console.WriteLine(string.Format("Passed tests: {0}", passed));
            Console.WriteLine(string.Format("Failed tests: {0}", failed.Count));
            
            if(failed.Count > 0)
            {
                Console.WriteLine("                    ");
                Console.WriteLine("*** FAILED TESTS ***");
                foreach(var f in failed)
                {
                    Console.WriteLine(f.TestName);
                }
                Console.WriteLine("                    ");

                return 1;
            }
            else
            {
                return 0;
            }
            
        }
    }
}
