using Lexer.Algoritmy;
using PisaciStroj.Lexer;
using System.Collections.Generic;

namespace PSMat.Testy.Lexer.Stubs
{
    public static class DfaAutomatonStubs
    {
        public static DfaAutomaton JednoduchyRegexAutomat()
        {
            return new DfaAutomaton()
            {
                AkceptovanyToken = TypTokenu.Identifikator,
                States = new DfaState[]
                {
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>
                        {
                            { 'a', 1 },
                            { 'b', 2 },
                            { 'c', 3 }
                        }
                    },
                    new DfaState
                    {
                        IsFinal = true,
                        Transitions = new Dictionary<char, int>()
                        {
                            { 'a', 3 },
                            { 'b', 3 },
                            { 'c', 3 }
                        }
                    },
                    new DfaState
                    {
                        IsFinal = true,
                        Transitions = new Dictionary<char, int>
                        {
                            { 'a', 3 },
                            { 'b', 3 },
                            { 'c', 2 }
                        }
                    },
                    new DfaState
                    {
                        IsFinal = false,
                        Transitions = new Dictionary<char, int>
                        {
                            { 'a', 3 },
                            { 'b', 3 },
                            { 'c', 3 }
                        }
                    }
                }
            };
        }

        public static DfaAutomaton DruhyJednoduchyRegexAutomat()
        {
            return new DfaAutomaton()
            {
                AkceptovanyToken = TypTokenu.KlucoveSlovo,
                States = new DfaState[]
                {
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>
                        {
                            { 'a', 1 },
                            { 'b', 2 },
                            { 'c', 3 }
                        }
                    },
                    new DfaState
                    {
                        IsFinal = true,
                        Transitions = new Dictionary<char, int>()
                        {
                            { 'a', 3 },
                            { 'b', 3 },
                            { 'c', 3 }
                        }
                    },
                    new DfaState
                    {
                        IsFinal = true,
                        Transitions = new Dictionary<char, int>
                        {
                            { 'a', 3 },
                            { 'b', 2 },
                            { 'c', 2 }
                        }
                    },
                    new DfaState
                    {
                        IsFinal = false,
                        Transitions = new Dictionary<char, int>
                        {
                            { 'a', 3 },
                            { 'b', 3 },
                            { 'c', 3 }
                        }
                    }
                }
            };
        }

        public static DfaAutomaton DruhyJednoduchyRegexAutomat2()
        {
            return new DfaAutomaton()
            {
                AkceptovanyToken = TypTokenu.KlucoveSlovo,
                States = new DfaState[]
                {
                    //initial state
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>
                        {
                            { 'f', 1 },
                            { 'n', 2 },
                            { 'e', 2 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>
                        {
                            { 'f', 2 },
                            { 'n', 3 },
                            { 'e', 2 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 4 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },
                    new DfaState
                    {
                        IsDead = true,
                        Transitions = new Dictionary<char, int>
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 2 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },

                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 5 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 2 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 6 },
                            { 'v', 2 }
                        }
                    },
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 2 },
                            { 'x', 7 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },
                    
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>()
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 8 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>()
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 2 },
                            { 'x', 2 },
                            { 't', 9 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },
                    new DfaState
                    {
                        Transitions = new Dictionary<char, int>()
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 2 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 9 }
                        }
                    },
                    new DfaState
                    {
                        IsFinal = true,
                        Transitions = new Dictionary<char, int>()
                        {
                            { 'f', 2 },
                            { 'n', 2 },
                            { 'e', 2 },
                            { 'x', 2 },
                            { 't', 2 },
                            { 'p', 2 },
                            { 'r', 2 },
                            { 'v', 2 }
                        }
                    },
                }
            };
        }
    }
}
