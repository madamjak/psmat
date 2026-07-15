using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
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
                new PisaciStroj.Program(SyntaxHighlightStubs.Riadky()),
                new PisaciStroj.Parametre.ParametreVyberu(),
                new LexAutomat(new LexGramatika()));

            var sb = new StringBuilder();

            VykreslovaciAutomat.Vykresli(result, sb, new StavovyRiadokInfo(), parametreVykreslovania);

            var ansiKod = sb.ToString();

            Console.Write(ansiKod);

            Console.ReadKey();
        }
    }
}
