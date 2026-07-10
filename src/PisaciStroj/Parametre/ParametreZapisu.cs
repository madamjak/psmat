using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using System;
using System.Collections.Generic;
using System.Text;

namespace PisaciStroj.Parametre
{
    public class ParametreZapisu
    {
        public int Okraj { get; set; }
    }

    public static class Indentation
    {
        public static void NastavOkraj(ParametreZapisu z, GapBuffer riadok)
        {
            var i = 0;
            while (true)
            {
                if(i == riadok.Length())
                {
                    break;
                }

                var ch = riadok.CharAt(i);
                if(ch != ' ')
                {
                    break;
                }

                i++;
            }

            z.Okraj = i;
        }

        public static void SimpleAutoIndent(List<GapBuffer> riadky, ParametreVypisu p, ParametreZapisu parametreZapisu)
        {
            var nr = riadky[p.IndexRiadok];
            var i = 0;
            while (i != parametreZapisu.Okraj)
            {
                nr.Insert(' ', 0);
                Kurzor.PosunKurzorDoprava(p, riadky);
                i++;
            }
        }
    }
}
