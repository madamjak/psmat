namespace PisaciStroj.Lexer
{
    public static class RegexGramatika
    {
        public static string Abeceda =
            "(q|w|e|r|t|y|u|i|o|p|a|s|d|f|g|h|j|k|l|z|x|c|v|b|n|m|Q|W|E|R|T|Y|U|I|O|P|A|S|D|F|G|H|J|K|L|Z|C|V|B|N|M|1|2|3|4|5|6|7|8|9|0)";
        public static string Cislo =
            "(1|2|3|4|5|6|7|8|9|0)";
        public static string BieleMiesto =
            "( |(\\\\.t))";

        public static LexPravidlo[] RegexG()
        {
            return new LexPravidlo[]
            {
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.Retazec,
                    Regex = string.Format("{0}.{1}*.\u0000", Abeceda, Abeceda)
                },
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.Retazec,
                    Regex = "((\\\\.\\*)|(\\\\.\\.)|(\\\\.\\|)).\u0000"
                },
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.Operator,
                    Regex = "(\\*|\\.|\\|).\u0000"
                },
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.KlucoveSlovo,
                    Regex = "((\\\\.w)|(\\\\.d)|(\\\\.s)).\u0000"
                }
            };
        }
    }
}
