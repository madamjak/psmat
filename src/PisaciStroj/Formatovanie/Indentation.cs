using PisaciStroj.Pamat;
using System.Text;

namespace PisaciStroj.Formatovanie
{
    public static class Indentation
    {
        public static int VypocitajZaciatokOkrajaNaZmazanie(GapBuffer riadok, int koniecOkraja, int dlzkaOkraja)
        {
            var i = koniecOkraja;
            var pocetZnakov = 0;
            while (true)
            {
                if (i == 0)
                {
                    break;
                }

                if(pocetZnakov == dlzkaOkraja)
                {
                    break;
                }

                var ch = riadok.CharAt(i);
                if (ch != ' ')
                {
                    break;
                }

                i--;
                pocetZnakov++;
            }

            return i;
        }

        public static string NastavOkraj(int okraj)
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
