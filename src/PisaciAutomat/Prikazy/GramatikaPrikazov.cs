using PisaciStroj.Lexer;

namespace PisaciAutomat.Prikazy
{
    public static class GramatikaPrikazov
    {
        public static LexPravidlo[] Gramatika()
        {
            return new LexPravidlo[]
            {
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.KlucoveSlovo,
                    Regex = "((f.n.e.x.t)|(f.p.r.e.v)|(f.a.l.l)|(r.a.l.l)|(r.f.r.s.t)|(g.o.t.o)|(s.a.a.s)). .\u0000"
                },
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.Cislo,
                    Regex = string.Format("{0}.{1}*.\u0000", RegexGramatika.Cislo, RegexGramatika.Cislo)
                }
            };
        }
    }
}
