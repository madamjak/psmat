using PSMat.Testy.Lexer;
using PSMat.Testy.Obrazovka;
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

            //var testy = new GapBufferTest();
            //testy.Spust();

            //var t = new SyntaxHighlightTest();
            //t.Spust();

            //var t = new BracketMatchingTest();
            //t.Spust();

            var t = new LexerTest();
            t.Spust();
        }
    }
}
