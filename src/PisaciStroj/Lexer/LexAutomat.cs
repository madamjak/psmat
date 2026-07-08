using Lexer.Algoritmy;
using PisaciStroj.Pamat;
using System.Collections.Generic;

namespace PisaciStroj.Lexer
{
    public class LexAutomat : ILexer
    {
        private SethiUllman _sethiUllman;
        private MultipleDfaSimulator _dfa;
        private LexGramatika _gramatika;

        public LexAutomat(LexGramatika gramatika)
        {
            _sethiUllman = new SethiUllman();
            _gramatika = gramatika;
            _dfa = BuildDfaAutomaton(gramatika);
        }

        private MultipleDfaSimulator BuildDfaAutomaton(LexGramatika gramatika)
        {
            var result = new List<IDfaSimulator>();

            foreach (var pravidlo in gramatika.Pravidla)
            {
                result.Add(BuildDfaAutomatSimulator(pravidlo));
            }

            return new MultipleDfaSimulator(result);
        }

        protected virtual IDfaSimulator BuildDfaAutomatSimulator(LexPravidlo pravidlo)
        {
            return new DfaSimulator(_sethiUllman.BuildDfa(pravidlo));
        }

        public Dictionary<int, Token> Lex(GapBuffer text)
        {
            if (_dfa == null)
            {
                return new Dictionary<int, Token>();
            }

            var result = new Dictionary<int, Token>();

            var poziciaHlavy = 0;

            while (true)
            {
                if (poziciaHlavy == text.Length())
                {
                    break;
                }

                if (JeBielyZnak(text.CharAt(poziciaHlavy)))
                {
                    poziciaHlavy++;

                    continue;
                }

                if (_dfa.ReadSymbol(text.CharAt(poziciaHlavy)))
                {
                    var token = VratNasledujuciToken(text, ref poziciaHlavy);

                    result.Add(token.Pozicia, token);
                }
                else
                {
                    //result.Add(poziciaHlavy, new Token()
                    //{
                    //    Typ = TypTokenu.Chyba,
                    //    Pozicia = poziciaHlavy,
                    //    Dlzka = 1
                    //});

                    _dfa.Reset();

                    poziciaHlavy++;

                    continue;
                }
            }

            return result;
        }

        private bool JeBielyZnak(char ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\t';
        }

        private Token VratNasledujuciToken(GapBuffer text, ref int poziciaHlavy)
        {
            //2. najdlhsi akceptovany lexem ma prioritu
            //3. v pripade rovnako dlhych akceptovanych lexemov ma prioritu ten ktory je v gramatike specifikovany prvy

            var zaciatokTokenu = poziciaHlavy;
            poziciaHlavy++;

            while (true)
            {
                if (poziciaHlavy == text.Length())
                {
                    break;
                }

                var canRead = _dfa.ReadSymbol(text.CharAt(poziciaHlavy));
                poziciaHlavy++;

                if (!canRead)
                {
                    poziciaHlavy--;
                    break;
                }

                //TODO lexgramatika nespecifikuje abecedu co moze viest k nespravnemu akceptovaniu klucovych slov
                //tj syntax highlight bez abecedy v lex gramatike funguje ako regex vyhladavanie
            }

            var akceptovanyToken = poziciaHlavy == text.Length() ?
                _dfa.IsAccepting() :
                _dfa.IsPreviousStateAccepting();

            _dfa.Reset();

            var token = new Token()
            {
                Typ = akceptovanyToken.HasValue ? akceptovanyToken.Value : TypTokenu.Chyba,
                Dlzka = poziciaHlavy - zaciatokTokenu,
                Pozicia = zaciatokTokenu
            };

            return token;
        }
    }
}