using Newtonsoft.Json;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PSMat.Testy.Obrazovka;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PSMat.Testy.Lexer
{
    public class LexerTest
    {
        public void Spust()
        {
            BasicTest();
        }

        private void BasicTest()
        {
            LexGramatika gramatika;
            using (var file = File.Open("Lexer/Config/CSharp.json", FileMode.Open))
            {
                using (var reader = new StreamReader(file))
                {
                    var s = reader.ReadToEnd();
                    gramatika = (LexGramatika)JsonConvert.DeserializeObject(s, typeof(LexGramatika));
                }
            }

            var lexer = new LexAutomat(gramatika);

            var text = SyntaxHighlightStubs.Riadky();
            var r = lexer.Lex(text);

            var parametreVykreslovania = SyntaxHighlightStubs.ParametreVypisu();
            parametreVykreslovania.Riadok = 1;

            var result = VykreslovaciAutomat.Precitaj2(
                parametreVykreslovania,
                SyntaxHighlightStubs.VysledkyVyhladavania(),
                r,
                new PisaciStroj.Program(text),
                new PisaciStroj.Parametre.ParametreVyberu(),
                new PisaciStroj.Parametre.ParametreZapisu(),
                new LexAutomat(new LexGramatika()));

            var sb = new StringBuilder();

            VykreslovaciAutomat.Vykresli(result, sb, string.Empty, parametreVykreslovania);

            var ansiKod = sb.ToString();

            Console.Write(ansiKod);

            Console.ReadKey();
        }
    }
}
