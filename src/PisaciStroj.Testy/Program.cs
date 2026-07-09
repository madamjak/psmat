using PSMat.Testy.Pamat;
using PSMat.Testy.PrikazovyRiadok;

namespace PisaciStroj.Testy
{
    class Program
    {
        static void Main(string[] args)
        {
            //var testy = new PrikazovyRiadokTesty();
            //testy.Spust();

            var testy = new GapBufferTest();
            testy.Spust();
        }
    }
}
