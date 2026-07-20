namespace PisaciStroj.Lexer
{
    public static class RegexGramatika
    {
        public static LexPravidlo[] RegexG()
        {
            return new LexPravidlo[]
            {
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.Retazec,
                    Regex = "(q|w|e|r|t|y|u|i|o|p|a|s|d|f|g|h|j|k|l|z|x|c|v|b|n|m|Q|W|E|R|T|Y|U|I|O|P|A|S|D|F|G|H|J|K|L|Z|C|V|B|N|M).(q|w|e|r|t|y|u|i|o|p|a|s|d|f|g|h|j|k|l|z|x|c|v|b|n|m|Q|W|E|R|T|Y|U|I|O|P|A|S|D|F|G|H|J|K|L|Z|C|V|B|N|M|1|2|3|4|5|6|7|8|9|0)*.\u0000"
                },
                new LexPravidlo()
                {
                    TypTokenu = TypTokenu.Operator,
                    Regex = "(\\*|\\.|\\|).\u0000"
                }
            };
        }
    }
}
