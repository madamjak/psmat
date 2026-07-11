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

        public static string SimpleAutoIndent(int okraj)
        {
            var sb = new StringBuilder();
            var i = 0;
            while (i != okraj)
            {
                sb.Append(' ');
                i++;
            }

            return sb.ToString();
        }
    }
}
