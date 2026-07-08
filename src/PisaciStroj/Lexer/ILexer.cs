using PisaciStroj.Pamat;
using System.Collections.Generic;

namespace PisaciStroj.Lexer
{
    public struct LexPravidlo
    {
        public TypTokenu TypTokenu { get; set; }

        public string Regex { get; set; }
    }

    public struct LexGramatika
    {
        public LexPravidlo[] Pravidla { get; set; }
    }

    public enum TypTokenu
    {
        KlucoveSlovo,
        Identifikator,
        Operator,
        Zatvorky,
        Cislo,
        Retazec,
        Komentar,
        BieleMiesto,
        Symbol,
        Chyba,
        KlucovaFunkcia
    }

    public struct Token
    {
        public TypTokenu Typ { get; set; }

        public int Dlzka { get; set; }

        public int Pozicia { get; set; }
    }

    public interface ILexer
    {
        Dictionary<int, Token> Lex(GapBuffer text);
    }
}
