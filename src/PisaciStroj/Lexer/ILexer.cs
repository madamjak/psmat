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
        public string Pripona { get; set; }

        public string JednoriadkovyKomentar { get; set; }

        public string ZaciatokKomentara { get; set; }

        public string KoniecKomentara { get; set; }

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

    public struct Pozicia
    {
        public int Riadok { get; set; }

        public int Slpec { get; set; }
    }

    public struct Zatvorka
    {
        public Pozicia Start { get; set; }

        public Pozicia End { get; set; }
    }

    public class LexResult
    {
        public Dictionary<int, Dictionary<int, Token>> Tokeny { get; set; }

        public Dictionary<int, Dictionary<int, Zatvorka>> Zatvorky { get; set; }
    }

    public interface ILexer
    {
        Dictionary<int, Token> Lex(GapBuffer text);

        LexResult Lex(List<GapBuffer> text);
    }
}
