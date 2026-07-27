using PisaciAutomat.Config;
using PisaciAutomat.Obrazovka;
using PisaciStroj.Lexer;
using PisaciStroj.Navigacia;
using PisaciStroj.Parametre;
using PisaciStroj.Testy;
using PisaciStroj.Vyhladavanie;
using PSMat.Testy.Lexer.Stubs;
using System;
using System.Collections.Generic;
using System.Text;
using static PisaciAutomat.Obrazovka.StylovaciAutomat;

namespace PSMat.Testy.Obrazovka
{
    public class SyntaxHighlightTest : ITest
    {
        public SyntaxHighlightTest()
        {
            Farby.DarkMode = true;
        }

        public List<TestResult> Spust()
        {
            return new List<TestResult>()
            {
                new TestResult()
                {
                    TestName = "CmdLineHighligting",
                    Pass = CmdLineHighligting()
                },
                new TestResult()
                {
                    TestName = "CSharpHighlighting",
                    Pass = CSharpHighlighting()
                }
            };
        }

        public bool CmdLineHighligting()
        {
            var g = PisaciAutomat.Prikazy.GramatikaPrikazov.Gramatika();

            var l = new LexAutomat(g);

            var text = SyntaxHighlightStubs.CmdLinePrikaz();

            var tokeny = l.LexPrePrikazovyRiadok(text);

            var parametreVykreslovania = SyntaxHighlightStubs.ParametreVypisu();

            var slova = new Dictionary<int, VyhladaneSlovo>();
            VyhladaneSlovo? vyhladaneSlovo = null;

            VyhladaneSlovo? zvyrazneneSlovo = null;

            var pozicia = new Pozicia();

            var sb = new StringBuilder();
            sb.Append(StylovaciAutomat.SyntaxAndSearchHighligt2(
                text[0],
                parametreVykreslovania.OffsetStlpec,
                parametreVykreslovania.Sirka,
                slova, vyhladaneSlovo,
                tokeny.Tokeny[0],
                tokeny.Zatvorky[0],
                pozicia,
                zvyrazneneSlovo,
                tokeny.RegexTokeny[0]));
            
            var ansi = sb.ToString();

            //Console.Write(ansi);

            return ansi == "\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ms\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ms\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87m \u001b[0m\u001b[37;48;5;234m\u001b[38;5;214m\"\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214mC\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214m:\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214m/\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214m/\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214mt\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214me\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214mm\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214mp\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214m/\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214mb\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214ml\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;214m\"\u001b[0m\u001b[37;48;5;234m \u001b[38;5;46mr\u001b[0m\u001b[37;48;5;234m\u001b[38;5;46me\u001b[0m\u001b[37;48;5;234m{\u001b[0m\u001b[37;48;5;234m(\u001b[0m\u001b[37;48;5;234m\u001b[38;5;46ma\u001b[0m\u001b[37;48;5;234m\u001b[1;38;5;87m|\u001b[0m\u001b[37;48;5;234m\u001b[38;5;46mb\u001b[0m\u001b[37;48;5;234m)\u001b[0m\u001b[37;48;5;234m\u001b[1;38;5;87m*\u001b[0m\u001b[37;48;5;234m}\u001b[0m\u001b[37;48;5;234m";
        }

        public bool CSharpHighlighting()
        {
            var gramatika = LexerStubs.CSharpGramatika();

            var lexer = new LexAutomat(gramatika);

            var text = SyntaxHighlightStubs.Riadky();
            var r = lexer.ZatvorkyAKomentare(text);

            var parametreVykreslovania = SyntaxHighlightStubs.ParametreVypisu();
            parametreVykreslovania.Riadok = 1;

            var result = VykreslovaciAutomat.Precitaj2(
                parametreVykreslovania,
                SyntaxHighlightStubs.VysledkyVyhladavania(),
                r,
                new PisaciStroj.Program(text),
                new PisaciStroj.Parametre.ParametreVyberu(),
                lexer,
                new VyhladavaciAutomat());

            var sb = new StringBuilder();

            VykreslovaciAutomat.Vykresli(result, sb, new StavovyRiadokInfo(), parametreVykreslovania);

            var ansiKod = sb.ToString();

            //Console.Write(ansiKod);

            return ansiKod == "\u001b[3;1H\u001b[0m\u001b[37;48;5;234m\u001b[2m001  \u001b[0m\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mn\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mm\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87me\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ms\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mp\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mc\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87me\u001b[0m\u001b[37;48;5;234m ConsoleApp1\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m002  \u001b[0m\u001b[0m\u001b[37;48;5;234m\u001b[41;1m{\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m003  \u001b[0m\u001b[0m\u001b[37;48;5;234m    \u001b[3;32m/\u001b[0m\u001b[37;48;5;234m\u001b[3;32m*\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m004  \u001b[0m\u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m*\u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32mR\u001b[0m\u001b[37;48;5;234m\u001b[3;32me\u001b[0m\u001b[37;48;5;234m\u001b[3;32mg\u001b[0m\u001b[37;48;5;234m\u001b[3;32me\u001b[0m\u001b[37;48;5;234m\u001b[3;32mx\u001b[0m\u001b[37;48;5;234m\u001b[3;32mA\u001b[0m\u001b[37;48;5;234m\u001b[3;32ms\u001b[0m\u001b[37;48;5;234m\u001b[3;32mt\u001b[0m\u001b[37;48;5;234m\u001b[3;32mT\u001b[0m\u001b[37;48;5;234m\u001b[3;32mr\u001b[0m\u001b[37;48;5;234m\u001b[3;32me\u001b[0m\u001b[37;48;5;234m\u001b[3;32me\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m005  \u001b[0m\u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32m*\u001b[0m\u001b[37;48;5;234m\u001b[3;32m/\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m006  \u001b[0m\u001b[0m\u001b[37;48;5;234m    publicclass RegexAstTree\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m007  \u001b[0m\u001b[0m\u001b[37;48;5;234m    {\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m008  \u001b[0m\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m009  \u001b[0m\u001b[0m\u001b[37;48;5;234m        \u001b[38;5;87mp\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mu\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mb\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ml\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mc\u001b[0m\u001b[37;48;5;234m \u001b[38;5;78mD\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mc\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mt\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mo\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mn\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mr\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78my\u001b[0m\u001b[37;48;5;234m<int, \u001b[38;5;78mH\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78ms\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mh\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mS\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78me\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mt\u001b[0m\u001b[37;48;5;234m<int>> FollowPos {\u001b[0m\u001b[37;48;5;234m \u001b[38;5;87mg\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87me\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mt\u001b[0m\u001b[37;48;5;234m; \u001b[38;5;87mp\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mr\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mv\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mt\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87me\u001b[0m\u001b[37;48;5;234m \u001b[38;5;87ms\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87me\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mt\u001b[0m\u001b[37;48;5;234m; }\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m010  \u001b[0m\u001b[0m\u001b[37;48;5;234m\u001b[3;32m/\u001b[0m\u001b[37;48;5;234m\u001b[3;32m/\u001b[0m\u001b[37;48;5;234m\u001b[3;32mT\u001b[0m\u001b[37;48;5;234m\u001b[3;32mO\u001b[0m\u001b[37;48;5;234m\u001b[3;32mD\u001b[0m\u001b[37;48;5;234m\u001b[3;32mO\u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32mm\u001b[0m\u001b[37;48;5;234m\u001b[3;32ma\u001b[0m\u001b[37;48;5;234m\u001b[3;32mk\u001b[0m\u001b[37;48;5;234m\u001b[3;32me\u001b[0m\u001b[37;48;5;234m\u001b[3;32m \u001b[0m\u001b[37;48;5;234m\u001b[3;32ms\u001b[0m\u001b[37;48;5;234m\u001b[3;32mt\u001b[0m\u001b[37;48;5;234m\u001b[3;32ma\u001b[0m\u001b[37;48;5;234m\u001b[3;32mt\u001b[0m\u001b[37;48;5;234m\u001b[3;32mi\u001b[0m\u001b[37;48;5;234m\u001b[3;32mc\u001b[0m\u001b[37;48;5;234m\u001b[3;32m?\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m011  \u001b[0m\u001b[0m\u001b[37;48;5;234m        \u001b[38;5;87mp\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mr\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mv\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mt\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87me\u001b[0m\u001b[37;48;5;234m \u001b[38;5;87mv\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mo\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87md\u001b[0m\u001b[37;48;5;234m BracketFun(\u001b[0m\u001b[37;48;5;234mint a, int b)\u001b[0m\u001b[37;48;5;234m{\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m012  \u001b[0m\u001b[0m\u001b[37;48;5;234m        \u001b[38;5;87mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mf\u001b[0m\u001b[37;48;5;234m (\u001b[0m\u001b[37;48;5;234mFollowPos.\u001b[38;5;78mC\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mo\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mn\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mt\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mn\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78ms\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mK\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78me\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78my\u001b[0m\u001b[37;48;5;234m(\u001b[0m\u001b[37;48;5;234ma)\u001b[0m\u001b[37;48;5;234m)\u001b[0m\u001b[37;48;5;234m{\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m013  \u001b[0m\u001b[0m\u001b[37;48;5;234m            vara abd = FollowPos[\u001b[0m\u001b[37;48;5;234ma]\u001b[0m\u001b[37;48;5;234m.\u001b[38;5;78mC\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mo\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mn\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mt\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mi\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78mn\u001b[0m\u001b[37;48;5;234m\u001b[38;5;78ms\u001b[0m\u001b[37;48;5;234m(\u001b[0m\u001b[37;48;5;234mb)\u001b[0m\u001b[37;48;5;234m){\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m014  \u001b[0m\u001b[0m\u001b[37;48;5;234m            \u001b[38;5;87mv\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87ma\u001b[0m\u001b[37;48;5;234m\u001b[38;5;87mr\u001b[0m\u001b[37;48;5;234m x = (\u001b[0m\u001b[37;48;5;234m\u001b[38;5;226m1\u001b[0m\u001b[37;48;5;234m - (\u001b[0m\u001b[37;48;5;234m\u001b[38;5;226m2\u001b[0m\u001b[37;48;5;234m3 + \u001b[38;5;226m5\u001b[0m\u001b[37;48;5;234m)\u001b[0m\u001b[37;48;5;234m / \u001b[38;5;226m4\u001b[0m\u001b[37;48;5;234m * (\u001b[0m\u001b[37;48;5;234m\u001b[38;5;226m3\u001b[0m\u001b[37;48;5;234m2 + a / (\u001b[0m\u001b[37;48;5;234mb - \u001b[38;5;226m1\u001b[0m\u001b[37;48;5;234m)\u001b[0m\u001b[37;48;5;234m)\u001b[0m\u001b[37;48;5;234m)\u001b[0m\u001b[37;48;5;234m)\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m015  \u001b[0m\u001b[0m\u001b[37;48;5;234m        }\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m016  \u001b[0m\u001b[0m\u001b[37;48;5;234m    }\u001b[0m\u001b[37;48;5;234m\r\n\u001b[0m\u001b[37;48;5;234m\u001b[2m017  \u001b[0m\u001b[0m\u001b[37;48;5;234m}\u001b[0m\u001b[37;48;5;234m}\u001b[0m\u001b[37;48;5;234m\u001b[41;1m}\u001b[0m\u001b[37;48;5;234m\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";
            
        }
    }
}
