using Lexer.Algoritmy;
using PisaciStroj.Lexer.Algoritmy;
using PisaciStroj.Pamat;
using System.Collections.Generic;

namespace PisaciStroj.Lexer
{
    public class LexAutomat : ILexer
    {
        private AhoSethiUllman _sethiUllman;
        private MultipleDfaSimulator _dfa;

        private string _komentar;
        private string _zaciatokKomentara;
        private string _koniecKomentara;

        public LexAutomat(LexGramatika gramatika)
        {
            _sethiUllman = new AhoSethiUllman();
            NastavLexer(gramatika);
        }

        public LexAutomat()
        {
            _sethiUllman = new AhoSethiUllman();
        }

        public void NastavLexer(LexGramatika gramatika)
        {
            _dfa = BuildDfaAutomaton(gramatika);

            _komentar = gramatika.JednoriadkovyKomentar;
            _zaciatokKomentara = gramatika.ZaciatokKomentara;
            _koniecKomentara = gramatika.KoniecKomentara;
        }

        private MultipleDfaSimulator BuildDfaAutomaton(LexGramatika gramatika)
        {
            var result = new List<IDfaSimulator>();

            if(gramatika.Pravidla != null)
            {
                foreach (var pravidlo in gramatika.Pravidla)
                {
                    result.Add(BuildDfaAutomatSimulator(pravidlo));
                }
            }

            return new MultipleDfaSimulator(result);
        }

        private IDfaSimulator BuildDfaAutomatSimulator(LexPravidlo pravidlo)
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

                    if (token.HasValue)
                    {
                        result.Add(token.Value.Pozicia, token.Value);
                    }
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

        private Token? VratNasledujuciToken(GapBuffer text, ref int poziciaHlavy)
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


            if (akceptovanyToken.HasValue) 
            {
                return new Token()
                {
                    Typ = akceptovanyToken.HasValue ? akceptovanyToken.Value : TypTokenu.Chyba,
                    Dlzka = poziciaHlavy - zaciatokTokenu,
                    Pozicia = zaciatokTokenu
                };
            }

            return null;
        }

        public LexResult Lex(List<GapBuffer> text)
        {
            var bmAlgo = new StackBracketMatching();
            var zatvorky = bmAlgo.GetMatchingBrackets(text);

            var result = new Dictionary<int, Dictionary<int, Token>>();
            
            var riadok = 0;
            
            var jeKomentar = false;
            Token komentar = new Token();

            var jeRetazec = false;
            Token retazec = new Token();
            
            foreach(var r in text)
            {
                var poziciaHlavy = 0;
                var rowResult = new Dictionary<int, Token>();
                while (true)
                {
                    if (poziciaHlavy == r.Length())
                    {
                        if (jeKomentar)
                        {
                            rowResult.Add(komentar.Pozicia, new Token() 
                            {
                                Typ = TypTokenu.Komentar,
                                Pozicia = komentar.Pozicia,
                                Dlzka = komentar.Dlzka
                            });

                            komentar.Pozicia = 0;
                            komentar.Dlzka = 0;
                        }

                        break;
                    }

                    if (jeKomentar)
                    {
                        var koniecKomentara = r.Read(poziciaHlavy, _koniecKomentara.Length);
                        if(!(koniecKomentara == _koniecKomentara))
                        {
                            poziciaHlavy++;
                            komentar.Dlzka++;
                            continue;
                        }
                        else
                        {
                            rowResult.Add(komentar.Pozicia, new Token()
                            {
                                Typ = TypTokenu.Komentar,
                                Pozicia = komentar.Pozicia,
                                Dlzka = poziciaHlavy + _koniecKomentara.Length
                            });

                            jeKomentar = false;
                            poziciaHlavy += koniecKomentara.Length;
                            continue;
                        }
                        
                    }
                    else
                    {
                        if (!jeRetazec)
                        {
                            if(!string.IsNullOrEmpty(_komentar))
                            {
                                var jednoRiadkovyKomentar = r.Read(poziciaHlavy, _komentar.Length);
                                if (jednoRiadkovyKomentar == _komentar)
                                {
                                    rowResult.Add(poziciaHlavy, new Token()
                                    {
                                        Typ = TypTokenu.Komentar,
                                        Pozicia = poziciaHlavy,
                                        Dlzka = r.Length() - poziciaHlavy
                                    });

                                    jeKomentar = false;
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(_zaciatokKomentara))
                            {
                                var zaciatokKomentara = r.Read(poziciaHlavy, _zaciatokKomentara.Length);
                                if (zaciatokKomentara == _zaciatokKomentara)
                                {
                                    jeKomentar = true;
                                    komentar.Pozicia = poziciaHlavy;
                                    komentar.Dlzka = 1;
                                    poziciaHlavy++;
                                    continue;
                                }
                            }
                        }
                    }

                    if (!jeRetazec)
                    {
                        if(r.CharAt(poziciaHlavy) == '"' || r.CharAt(poziciaHlavy) == '\'')
                        {
                            jeRetazec = true;
                            retazec = new Token()
                            {
                                Pozicia = poziciaHlavy,
                                Dlzka = 1
                            };
                            poziciaHlavy++;
                            continue;
                        }
                    }
                    else
                    {
                        if(poziciaHlavy < r.Length() - 1 && r.CharAt(poziciaHlavy) != '\\'
                            && (r.CharAt(poziciaHlavy + 1) == '"' || r.CharAt(poziciaHlavy + 1) == '\'')) 
                        {
                            rowResult.Add(retazec.Pozicia, new Token()
                            {
                                Typ = TypTokenu.Retazec,
                                Pozicia = retazec.Pozicia,
                                Dlzka = retazec.Dlzka + 2
                            });
                            
                            jeRetazec = false;

                            poziciaHlavy ++;
                            continue;
                        }
                        else
                        {
                            retazec.Dlzka++;
                            poziciaHlavy++;

                            continue;
                        }
                    }

                    if (zatvorky[riadok].ContainsKey(poziciaHlavy))
                    {
                        poziciaHlavy++;
                        continue;
                    }

                    if (JeBielyZnak(r.CharAt(poziciaHlavy)))
                    {
                        poziciaHlavy++;

                        continue;
                    }

                    if (_dfa.ReadSymbol(r.CharAt(poziciaHlavy)))
                    {
                        var token = VratNasledujuciToken(r, ref poziciaHlavy);

                        if (token.HasValue)
                        {
                            rowResult.Add(token.Value.Pozicia, token.Value);
                        }
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

                result.Add(riadok, rowResult);
                riadok++;
            }

            var lr = new LexResult()
            {
                Tokeny = result,
                Zatvorky = zatvorky
            };

            return lr;
        }

        public LexResult ZatvorkyAKomentare(List<GapBuffer> text)
        {
            var bmAlgo = new StackBracketMatching();
            var zatvorky = bmAlgo.GetMatchingBrackets(text);

            var result = new Dictionary<int, Dictionary<int, Token>>();

            var riadok = 0;

            var jeKomentar = false;
            Token komentar = new Token();

            var jeRetazec = false;
            Token retazec = new Token();

            foreach (var r in text)
            {
                var poziciaHlavy = 0;
                var rowResult = new Dictionary<int, Token>();
                while (true)
                {
                    if (poziciaHlavy == r.Length())
                    {
                        if (jeKomentar)
                        {
                            rowResult.Add(komentar.Pozicia, new Token()
                            {
                                Typ = TypTokenu.Komentar,
                                Pozicia = komentar.Pozicia,
                                Dlzka = komentar.Dlzka
                            });

                            komentar.Pozicia = 0;
                            komentar.Dlzka = 0;
                        }

                        if (jeRetazec)
                        {
                            rowResult.Add(retazec.Pozicia, new Token()
                            {
                                Typ = TypTokenu.Retazec,
                                Pozicia = retazec.Pozicia,
                                Dlzka = retazec.Dlzka
                            });

                            retazec.Pozicia = 0;
                            retazec.Dlzka = 0;
                            jeRetazec = false;
                        }

                        break;
                    }

                    if (jeKomentar)
                    {
                        var koniecKomentara = r.Read(poziciaHlavy, _koniecKomentara.Length);
                        if (!(koniecKomentara == _koniecKomentara))
                        {
                            poziciaHlavy++;
                            komentar.Dlzka++;
                            continue;
                        }
                        else
                        {
                            rowResult.Add(komentar.Pozicia, new Token()
                            {
                                Typ = TypTokenu.Komentar,
                                Pozicia = komentar.Pozicia,
                                Dlzka = poziciaHlavy + _koniecKomentara.Length - komentar.Pozicia
                            });

                            jeKomentar = false;
                            poziciaHlavy += koniecKomentara.Length;
                            continue;
                        }

                    }
                    else
                    {
                        if (!jeRetazec)
                        {
                            if (!string.IsNullOrEmpty(_komentar)) 
                            {
                                var jednoRiadkovyKomentar = r.Read(poziciaHlavy, _komentar.Length);
                                if (jednoRiadkovyKomentar == _komentar)
                                {
                                    rowResult.Add(poziciaHlavy, new Token()
                                    {
                                        Typ = TypTokenu.Komentar,
                                        Pozicia = poziciaHlavy,
                                        Dlzka = r.Length() - poziciaHlavy
                                    });

                                    jeKomentar = false;
                                    break;
                                }
                            }

                            if (!string.IsNullOrEmpty(_zaciatokKomentara))
                            {
                                var zaciatokKomentara = r.Read(poziciaHlavy, _zaciatokKomentara.Length);
                                if (zaciatokKomentara == _zaciatokKomentara)
                                {
                                    jeKomentar = true;
                                    komentar.Pozicia = poziciaHlavy;
                                    komentar.Dlzka = 1;
                                    poziciaHlavy++;
                                    continue;
                                }
                            }
                        }
                    }

                    if (!jeRetazec)
                    {
                        if (r.CharAt(poziciaHlavy) == '"' || r.CharAt(poziciaHlavy) == '\'')
                        {
                            jeRetazec = true;
                            retazec = new Token()
                            {
                                Pozicia = poziciaHlavy,
                                Dlzka = 1
                            };
                            poziciaHlavy++;
                            continue;
                        }
                    }
                    else
                    {
                        if (poziciaHlavy < r.Length() - 1 && r.CharAt(poziciaHlavy) != '\\'
                            && (r.CharAt(poziciaHlavy + 1) == '"' || r.CharAt(poziciaHlavy + 1) == '\''))
                        {
                            rowResult.Add(retazec.Pozicia, new Token()
                            {
                                Typ = TypTokenu.Retazec,
                                Pozicia = retazec.Pozicia,
                                Dlzka = retazec.Dlzka + 2
                            });
                            jeRetazec = false;

                            poziciaHlavy += 2;
                            continue;
                        }
                        else
                        {
                            retazec.Dlzka++;
                            poziciaHlavy++;

                            continue;
                        }
                    }

                    poziciaHlavy++;
                }

                if(rowResult.Count > 0)
                {
                    result.Add(riadok, rowResult);
                }
                riadok++;
            }

            var lr = new LexResult()
            {
                Tokeny = result,
                Zatvorky = zatvorky
            };

            return lr;
        }
    }
}