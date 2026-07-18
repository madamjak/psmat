using PisaciStroj.Pamat;
using PisaciStroj.Testy;
using System.Collections.Generic;

namespace PSMat.Testy.Pamat
{
    public class GapBufferTest : ITest
    {
        private bool OtestujJednoducheMazanie()
        {
            var b = new GapBuffer();

            b.Append("        public Dictionary");

            b.Delete(0, 6);

            var text = b.Read();

            return text == "  public Dictionary";
        }

        public List<TestResult> Spust()
        {
            return new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "OtestujJednoducheMazanie",
                    Pass = OtestujJednoducheMazanie()
                }
            };
        }
    }
}
