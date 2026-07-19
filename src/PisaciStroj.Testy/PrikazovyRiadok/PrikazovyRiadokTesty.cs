using PisaciAutomat.Obrazovka;
using PisaciAutomat.Prikazy;
using PisaciStroj.Pamat;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.PrikazovyRiadok
{
    public class PrikazovyRiadokTesty
    {
        public void Spust()
        {
            var p = new PrikazovyAutomat();

            p.Prekresli(new ParametrePrekreslenia(), new StringBuilder(), new List<GapBuffer>());

            PrikazovyAutomatResult r = null;

            while (true)
            {
                var vstup = Console.ReadKey();

                r = p.SpracujVstup(vstup);

                p.Prekresli(new ParametrePrekreslenia(), new StringBuilder(), new List<GapBuffer>());
            }
        }
    }
}
