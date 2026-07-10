using PisaciAutomat.Obrazovka;
using System;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Obrazovka
{
    public class SyntaxHighlightTest
    {
        public void Spust()
        {
            OtestujHighlighting();
        }

        private void OtestujHighlighting()
        {
            var parametreVykreslovania = SyntaxHighlightStubs.ParametreVypisu();
            parametreVykreslovania.Riadok = 1;

            var result = VykreslovaciAutomat.Precitaj2(
                parametreVykreslovania, 
                SyntaxHighlightStubs.VysledkyVyhladavania(), 
                SyntaxHighlightStubs.Tokeny(), 
                SyntaxHighlightStubs.Riadky());

            var sb = new StringBuilder();

            VykreslovaciAutomat.Vykresli(result, sb, string.Empty, parametreVykreslovania);

            var ansiKod = sb.ToString();

            Console.Write(ansiKod);

            Console.ReadKey();
        }
    }
}
