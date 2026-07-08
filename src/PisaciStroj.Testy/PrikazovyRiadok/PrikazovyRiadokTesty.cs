using PisaciAutomat.Prikazy;
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

            p.Prekresli();

            PrikazovyAutomatResult r = null;

            while (true)
            {
                var vstup = Console.ReadKey();

                r = p.SpracujVstup(vstup);

                p.Prekresli();
            }
        }
    }
}
