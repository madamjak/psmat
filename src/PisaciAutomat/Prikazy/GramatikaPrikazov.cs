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
                    Regex = "((f.n.e.x.t)|(f.p.r.e.v)|(f.a.l.l)|(r.a.l.l)|(r.f.r.s.t)|(r.s.t)|(s.a.a.s)).\u0000"
                }
            };
        }
    }
}
