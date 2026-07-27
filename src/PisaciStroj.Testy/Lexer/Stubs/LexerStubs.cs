using PisaciStroj.Lexer;
using System.Collections.Generic;

namespace PSMat.Testy.Lexer.Stubs
{
    public struct LexerStubData
    {
        public string Text { get; set; }

        public Dictionary<int, Token> Tokens { get; set; }
    }

    public static class LexerStubs
    {
        internal static LexGramatika CSharpGramatika()
        {
            var p1 = new LexPravidlo()
            {
                TypTokenu = TypTokenu.KlucoveSlovo,
                Regex = "((n.u.l.l)|(i.f)|(e.l.s.e)|(f.o.r.e.a.c.h)|(v.a.r)|(b.o.o.l)|(g.e.t)|(s.e.t)).\u0000"
            };
            var p2 = new LexPravidlo()
            {
                TypTokenu = TypTokenu.KlucoveSlovo,
                Regex = "((v.o.i.d)|(u.s.i.n.g)|(r.e.t.u.r.n)|(n.a.m.e.s.p.a.c.e)|(p.u.b.l.i.c)|(p.r.i.v.a.t.e)).\u0000"
            };
            var p3 = new LexPravidlo()
            {
                TypTokenu = TypTokenu.KlucovaFunkcia,
			    Regex = "((D.i.c.t.i.o.n.a.r.y)|(H.a.s.h.S.e.t)|(A.d.d)|(C.o.n.t.a.i.n.s.K.e.y)|(C.o.n.t.a.i.n.s)).\u0000"
            };
            var p4 = new LexPravidlo()
            {
                TypTokenu = TypTokenu.Identifikator,
                Regex = "(q|w|e|r|t|y|u|i|o|p|a|s|d|f|g|h|j|k|l|z|x|c|v|b|n|m|Q|W|E|R|T|Y|U|I|O|P|A|S|D|F|G|H|J|K|L|Z|C|V|B|N|M).(q|w|e|r|t|y|u|i|o|p|a|s|d|f|g|h|j|k|l|z|x|c|v|b|n|m|Q|W|E|R|T|Y|U|I|O|P|A|S|D|F|G|H|J|K|L|Z|C|V|B|N|M|1|2|3|4|5|6|7|8|9|0)*.\u0000"
            };
            var p5 = new LexPravidlo()
            {
                TypTokenu = TypTokenu.Cislo,
                Regex = "(1|2|3|4|5|6|7|8|9|0).\u0000"
            };
            var p6 = new LexPravidlo()
            {
                TypTokenu = TypTokenu.Operator,
                Regex = "(=|<|>|!|+|-|/).\u0000"
            };

            var g = new LexGramatika()
            {
                JednoriadkovyKomentar = "//",
                ZaciatokKomentara = "/*",
                KoniecKomentara = "*/",
                ZaciatokRetazca = "\"",
                KoniecRetazca = "\"",
                Pravidla = new LexPravidlo[] { p1, p2, p3, p4, p5, p6 }
            };

            return g;
        }

        public static LexerStubData JednoduchyRiadok()
        {
            return new LexerStubData()
            {
                Text = "var vara = \"var\"",
                Tokens = new Dictionary<int, Token>()
                {
                    { 0, new Token()
                    {
                        Typ = TypTokenu.KlucoveSlovo,
                        Pozicia = 0,
                        Dlzka = 3
                    } },
                    { 4, new Token()
                    {
                        Typ = TypTokenu.Identifikator,
                        Pozicia = 4,
                        Dlzka = 4
                    } },
                    { 9, new Token()
                    {
                        Typ = TypTokenu.Operator,
                        Pozicia = 9,
                        Dlzka = 1
                    } },
                    { 11, new Token()
                    {
                        Typ = TypTokenu.Retazec,
                        Pozicia = 11,
                        Dlzka = 5
                    } }
                }
            };
        }
    }
}
