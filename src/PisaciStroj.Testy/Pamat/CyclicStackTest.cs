using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using System;
using System.Collections.Generic;

namespace PSMat.Testy.Pamat
{
    public class CyclicStackTest : ITest
    {
        public List<TestResult> Spust()
        {
            return new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "CyclicStackTest.Test1",
                    Pass = Test1()
                },
                new TestResult()
                {
                    TestName = "CyclicStackTest.Test2",
                    Pass = Test2()
                }
            };
        }

        public bool Test2()
        {
            var stack = new CyclicStack<Operacia>(4);

            stack.Push(new Operacia() { NovyText = "a" });
            stack.Push(new Operacia() { NovyText = "b" });
            stack.Push(new Operacia() { NovyText = "c" });
            stack.Push(new Operacia() { NovyText = "d" });
            stack.Push(new Operacia() { NovyText = "e" });
            stack.Push(new Operacia() { NovyText = "f" });
            stack.Pop();
            stack.Pop();
            stack.Push(new Operacia() { NovyText = "x" });
            stack.Push(new Operacia() { NovyText = "y" });

            var o1 = stack.Pop();
            var o1pass = o1.NovyText == "y";

            var o2 = stack.Pop();
            var o2pass = o2.NovyText == "x";

            var o3 = stack.Pop();
            var o3pass = o3.NovyText == "d";

            var o4 = stack.Pop();
            var o4pass = o4.NovyText == "c";

            var o5pass = false;
            try
            {
                var o5 = stack.Pop();
            }
            catch (ApplicationException ex)
            {
                o5pass = ex.Message == "Stack empty";
            }

            return o1pass && o2pass && o3pass && o4pass && o5pass;
        }

        public bool Test1()
        {
            var stack = new CyclicStack<Operacia>(4);

            stack.Push(new Operacia() { NovyText = "a" });
            stack.Push(new Operacia() { NovyText = "b" });
            stack.Push(new Operacia() { NovyText = "c" });
            stack.Push(new Operacia() { NovyText = "d" });
            stack.Push(new Operacia() { NovyText = "e" });
            stack.Push(new Operacia() { NovyText = "f" });
            stack.Push(new Operacia() { NovyText = "g" });

            var o1 = stack.Pop();
            var o1pass = o1.NovyText == "g";

            var o2 = stack.Pop();
            var o2pass = o2.NovyText == "f";

            var o3 = stack.Pop();
            var o3pass = o3.NovyText == "e";

            var o4 = stack.Pop();
            var o4pass = o4.NovyText == "d";

            var o5pass = false;
            try
            {
                var o5 = stack.Pop();
            }
            catch(ApplicationException ex)
            {
                o5pass = ex.Message == "Stack empty";
            }

            return o1pass && o2pass && o3pass && o4pass && o5pass;
        }
    }
}
