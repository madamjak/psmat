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

        private MultipleDfaSimulator _regexAutomat;

        public LexAutomat(LexGramatika gramatika)
        {
            _sethiUllman = new AhoSethiUllman();
            NastavLexer(gramatika);
            _regexAutomat = SkonstruujRegexAutomat(RegexGramatika.RegexG());
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

        private MultipleDfaSimulator SkonstruujRegexAutomat(LexPravidlo[] pravidla)
        {
            var result = new List<IDfaSimulator>();

            foreach (var pravidlo in pravidla)
            {
                result.Add(BuildDfaAutomatSimulator(pravidlo));
            }

            return new MultipleDfaSimulator(result);
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

        /// <summary>
        /// Lex pre editor
        /// </summary>
        public Dictionary<int, Token> LexPreEditor(GapBuffer text)
        {
            if (_dfa == null)
            {
                return new Dictionary<int, Token>();
            }

            var t = new List<GapBuffer> { text };
            var z = new Dictionary<int, Dictionary<int, Zatvorka>>();

            var tokeny = LexInternal(t, z, false, false);

            return tokeny[0];
        }

        private bool JeBielyZnak(char ch)
        {
            return ch == ' ' || ch == '\t' || ch == '\r' || ch == '\t';
        }

        /// <summary>
        /// pravidla:
        /// 1. retazce a komentare su spracovavane zvlast mimo tejto funkcie,
        /// 2. najdlhsi akceptovany lexem ma prioritu, t.j. pokial automat dokaze citat tak cita aj v pripade ze nejaky automat akceptuje
        /// 3. v pripade rovnako dlhych akceptovanych lexemov ma prioritu ten ktory je v gramatike specifikovany prvy
        /// 
        /// poznamka; konfiguracia gramatiky nespecifikuje abecedu, 
        /// t.j. ak je sucastou retazca nieco co nie je nakonfigurovane ako sucast regexu, automat to neprecita...
        /// ...co nasledne moze viest k nespravnemu akceptovaniu retazca ktory nakonfigurovany, napriek tomu ze dany retazec je sucastou niecoho ineho
        /// </summary>
        /// <returns></returns>
        private Token? VratNasledujuciToken(GapBuffer text, ref int poziciaHlavy, MultipleDfaSimulator automat)
        {
            var zaciatokTokenu = poziciaHlavy;
            poziciaHlavy++;

            while (true)
            {
                if (poziciaHlavy == text.Length())
                {
                    break;
                }

                var canRead = automat.ReadSymbol(text.CharAt(poziciaHlavy));
                poziciaHlavy++;

                if (!canRead)
                {
                    poziciaHlavy--;
                    break;
                }
            }

            var akceptovanyToken = poziciaHlavy == text.Length() ?
                automat.IsAccepting() :
                automat.IsPreviousStateAccepting();

            automat.Reset();


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

        private Dictionary<int, Dictionary<int, Token>> LexInternal(List<GapBuffer> text, Dictionary<int, Dictionary<int, Zatvorka>> zatvorky, bool parsujRegex, bool parsujKoniecRetazca, Dictionary<int, Token> regexTokeny = null)
        {
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

                        if (jeRetazec && !parsujKoniecRetazca)
                        {
                            rowResult.Add(retazec.Pozicia, new Token()
                            {
                                Typ = TypTokenu.Retazec,
                                Pozicia = retazec.Pozicia,
                                Dlzka = retazec.Dlzka
                            });

                            retazec.Pozicia = 0;
                            retazec.Dlzka = 0;
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
                                    komentar.Dlzka = _zaciatokKomentara.Length;
                                    poziciaHlavy++;
                                    continue;
                                }
                            }
                        }
                    }

                    if (jeRetazec)
                    {
                        if (poziciaHlavy <= r.Length() - 2 && r.CharAt(poziciaHlavy) != '\\'
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
                    else
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

                    if (parsujRegex)
                    {
                        
                        if (poziciaHlavy <= r.Length() - 2 && r.CharAt(poziciaHlavy) == '\\'
                                    && r.CharAt(poziciaHlavy + 1) == '\\')
                        {
                                
                            var regex = new Token()
                            {
                                Pozicia = poziciaHlavy,
                                Dlzka = 2
                            };
                            poziciaHlavy += 2;

                            var tokeny = new Dictionary<int, Token>();
                            while (true)
                            {
                                if(poziciaHlavy == r.Length())
                                {
                                    break;
                                }

                                Dictionary<int, Zatvorka> rz;
                                if (zatvorky.TryGetValue(riadok, out rz))
                                {
                                    if (rz.ContainsKey(poziciaHlavy))
                                    {
                                        poziciaHlavy++;
                                        continue;
                                    }
                                }

                                if (JeBielyZnak(r.CharAt(poziciaHlavy)))
                                {
                                    poziciaHlavy++;

                                    continue;
                                }

                                if (_regexAutomat.ReadSymbol(r.CharAt(poziciaHlavy)))
                                {
                                    var token = VratNasledujuciToken(r, ref poziciaHlavy, _regexAutomat);
                                    if (token.HasValue)
                                    {
                                        tokeny.Add(token.Value.Pozicia, token.Value);
                                    }
                                }
                                else
                                {
                                    _regexAutomat.Reset();

                                    if (poziciaHlavy <= r.Length() - 2 && r.CharAt(poziciaHlavy) == '\\'
                                    && r.CharAt(poziciaHlavy + 1) == '\\')
                                    {
                                        rowResult.Add(regex.Pozicia, new Token()
                                        {
                                            Typ = TypTokenu.Regex,
                                            Pozicia = regex.Pozicia,
                                            Dlzka = poziciaHlavy + 2 - regex.Pozicia
                                        });

                                        foreach (var t in tokeny)
                                        {
                                            regexTokeny.Add(t.Key, t.Value);
                                        }

                                        poziciaHlavy += 2;
                                        break;
                                    }

                                    poziciaHlavy++;
                                }
                            }

                            continue;
                        }
                    }

                    Dictionary<int, Zatvorka> z;
                    if (zatvorky.TryGetValue(riadok, out z))
                    {
                        if (z.ContainsKey(poziciaHlavy))
                        {
                            poziciaHlavy++;
                            continue;
                        }
                    }

                    if (JeBielyZnak(r.CharAt(poziciaHlavy)))
                    {
                        poziciaHlavy++;

                        continue;
                    }

                    if (_dfa.ReadSymbol(r.CharAt(poziciaHlavy)))
                    {
                        var token = VratNasledujuciToken(r, ref poziciaHlavy, _dfa);

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

            return result;
        }

        /// <summary>
        /// Hlavny rozdiel v spracovavani retazcov
        /// editor syntax highlight moze zvyraznit neuzavrete
        /// prikazovy riadok vyuziva typ tokenov na validaciu preto musia byt uzavrete
        /// </summary>
        public LexResult LexPrePrikazovyRiadok(List<GapBuffer> text)
        {
            var bmAlgo = new StackBracketMatching();
            var zatvorky = bmAlgo.GetMatchingBrackets(text);

            var regexTokeny = new Dictionary<int, Token>();

            var tokeny = LexInternal(text, zatvorky, true, true, regexTokeny);

            var lr = new LexResult()
            {
                Tokeny = tokeny,
                Zatvorky = zatvorky,
                RegexTokeny = new Dictionary<int, Dictionary<int, Token>> { { 0, regexTokeny } }
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
                                    komentar.Dlzka = _zaciatokKomentara.Length;
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
                            poziciaHlavy++;
                            continue;
                        }
                    }
                    else
                    {
                        if (poziciaHlavy < r.Length() - 1 && r.CharAt(poziciaHlavy) != '\\'
                            && (r.CharAt(poziciaHlavy + 1) == '"' || r.CharAt(poziciaHlavy + 1) == '\''))
                        {
                            jeRetazec = false;

                            poziciaHlavy += 2;
                            continue;
                        }
                        else
                        {
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