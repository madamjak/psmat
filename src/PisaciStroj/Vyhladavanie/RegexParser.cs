using PisaciStroj.Lexer;
using PisaciStroj.Pamat;
using System.Text;

namespace PisaciStroj.Vyhladavanie
{
    public static class RegexParser
    {
        public static string ParseRegex(LexResult lexResults, GapBuffer riadok, Token regexToken)
        {
            var sb = new StringBuilder();

            var koniecRegexu = regexToken.Pozicia + regexToken.Dlzka - 1;
            var zaciatokRegexu = regexToken.Pozicia + LexAutomat.RegexPrefix;

            var pridajZatvorky = lexResults.RegexTokeny[0].Count > 2;
            if(pridajZatvorky && lexResults.Zatvorky != null && lexResults.Zatvorky.Count > 0)
            {
                Zatvorka vonkajsiaZatvorka;
                if(lexResults.Zatvorky[0].TryGetValue(zaciatokRegexu, out vonkajsiaZatvorka))
                {
                    if(vonkajsiaZatvorka.End.Stlpec == koniecRegexu)
                    {
                        pridajZatvorky = false;
                    }
                }
            }

            if (pridajZatvorky)
            {
                sb.Append("(");
            }

            var i = zaciatokRegexu;
            var regexTokeny = lexResults.RegexTokeny[0];
            Token? lastT = null;
            while (true)
            {
                if(i == koniecRegexu)
                {
                    break;
                }

                Token t;
                if(regexTokeny.TryGetValue(i, out t))
                {
                    if (lastT.HasValue)
                    {
                        if((lastT.Value.Typ != TypTokenu.Operator && t.Typ != TypTokenu.Operator)
                           || (riadok.CharAt(lastT.Value.Pozicia) == '*') && t.Typ != TypTokenu.Operator)
                        {
                            sb.Append(".");
                        }
                    }
                    if(t.Dlzka == 1)
                    {
                        sb.Append(riadok.CharAt(i));
                        i++;
                    }
                    else
                    {NahradToken(t, sb, riadok);
                        i += t.Dlzka;
                    }

                    lastT = t;
                }
                else
                {
                    if (lastT.HasValue && lastT.Value.Typ != TypTokenu.Operator)
                    {
                        sb.Append(".");
                    }

                    sb.Append(riadok.CharAt(i));
                    i++;
                }
            }

            if (pridajZatvorky)
            {
                sb.Append(")");
            }
            sb.Append(".\0");

            return sb.ToString();
        }

        private static void NahradToken(Token t, StringBuilder sb, GapBuffer riadok)
        {
            if(t.Typ == TypTokenu.Retazec)
            {
                NahradRetazec(t, sb, riadok);
            }
            else if(t.Typ == TypTokenu.KlucoveSlovo)
            {
                NahradKlucoveSlovo(t, sb, riadok);
            }
        }

        private static void NahradKlucoveSlovo(Token t, StringBuilder sb, GapBuffer riadok)
        {
            var slovo = riadok.Read(t.Pozicia, t.Dlzka);

            if(slovo == "\\w")
            {
                sb.Append(RegexGramatika.Abeceda);
            }
            else if(slovo == "\\d")
            {
                sb.Append(RegexGramatika.Cislo);
            }else if(slovo == "\\s")
            {
                sb.Append(RegexGramatika.BieleMiesto);
            }
        }

        private static void NahradRetazec(Token t, StringBuilder sb, GapBuffer riadok)
        {
            var i = t.Pozicia;
            var e = t.Pozicia + t.Dlzka;
            sb.Append("(");
            while (true)
            {
                if(i == e)
                {
                    break;
                }

                sb.Append(riadok.CharAt(i));
                if(i < e - 1)
                {
                    sb.Append(".");
                }

                i++;
            }
            sb.Append(")");
        }
    }
}
