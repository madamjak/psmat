using PisaciStroj.Navigacia;
using PisaciStroj.Pamat;
using System.Collections.Generic;

namespace PisaciStroj.Lexer.Algoritmy
{
    public interface IBracketMatching
    {
        Dictionary<int, Dictionary<int, Zatvorka>> GetMatchingBrackets(List<GapBuffer> text);
    }

    public class StackBracketMatching : IBracketMatching
    {
        public static HashSet<char> Zatvorky = new HashSet<char> { '(', ')', '{', '}', '[', ']' };

        private Stack<Pozicia> _stack1 = new Stack<Pozicia>(); // ( )
        private Stack<Pozicia> _stack2 = new Stack<Pozicia>(); // { }
        private Stack<Pozicia> _stack3 = new Stack<Pozicia>(); // [ ]
        
        public Dictionary<int, Dictionary<int, Zatvorka>> GetMatchingBrackets(List<GapBuffer> text)
        {
            var result = new Dictionary<int, Dictionary<int, Zatvorka>>();
            for (int i = 0; i < text.Count; i++)
            {
                var index = 0;
                var riadok = text[i];
                while (true)
                {
                    if(index == riadok.Length())
                    {
                        break;
                    }

                    if (riadok.CharAt(index) == '(')
                    {
                        _stack1.Push(new Pozicia() 
                        {
                            Riadok = i,
                            Stlpec = index
                        });
                    }
                    if (riadok.CharAt(index) == '{')
                    {
                        _stack2.Push(new Pozicia()
                        {
                            Riadok = i,
                            Stlpec = index
                        });
                    }
                    if (riadok.CharAt(index) == '[')
                    {
                        _stack3.Push(new Pozicia()
                        {
                            Riadok = i,
                            Stlpec = index
                        });
                    }

                    if (riadok.CharAt(index) == ')' && _stack1.Count > 0)
                    {
                        var z = new Zatvorka()
                        {
                            Start = _stack1.Pop(),
                            End = new Pozicia()
                            {
                                Riadok = i,
                                Stlpec = index
                            }
                        };

                        PridajZDoVysl(result, z);
                    }

                    if (riadok.CharAt(index) == '}' && _stack2.Count > 0)
                    {
                        var z = new Zatvorka()
                        {
                            Start = _stack2.Pop(),
                            End = new Pozicia()
                            {
                                Riadok = i,
                                Stlpec = index
                            }
                        };

                        PridajZDoVysl(result, z);
                    }

                    if (riadok.CharAt(index) == ']' && _stack3.Count > 0)
                    {
                        var z = new Zatvorka()
                        {
                            Start = _stack3.Pop(),
                            End = new Pozicia()
                            {
                                Riadok = i,
                                Stlpec = index
                            }
                        };

                        PridajZDoVysl(result, z);
                    }

                    index++;
                }
            }

            return result;
        }

        public static void PridajZDoVysl(Dictionary<int, Dictionary<int, Zatvorka>> vysl, Zatvorka z)
        {
            Dictionary<int, Zatvorka> r = null;
            if(vysl.TryGetValue(z.Start.Riadok, out r))
            {
                r.Add(z.Start.Stlpec, z);
            }
            else
            {
                vysl.Add(z.Start.Riadok, new Dictionary<int, Zatvorka>()
                {
                    { z.Start.Stlpec, z }
                });
            }

            if (vysl.TryGetValue(z.End.Riadok, out r))
            {
                r.Add(z.End.Stlpec, z);
            }
            else
            {
                vysl.Add(z.End.Riadok, new Dictionary<int, Zatvorka>()
                {
                    { z.End.Stlpec, z }
                });
            }
        }

        public static Dictionary<int, Zatvorka> GetMatchingBrackets(GapBuffer riadok, int i)
        {
            Stack<Pozicia> _stack1 = new Stack<Pozicia>(); // ( )
            Stack<Pozicia> _stack2 = new Stack<Pozicia>(); // { }
            Stack<Pozicia> _stack3 = new Stack<Pozicia>(); // [ ]
            var index = 0;
            var r = new Dictionary<int, Zatvorka>();
            while (true)
            {
                if (index == riadok.Length())
                {
                    break;
                }

                if (riadok.CharAt(index) == '(')
                {
                    _stack1.Push(new Pozicia()
                    {
                        Riadok = i,
                        Stlpec = index
                    });
                }
                if (riadok.CharAt(index) == '{')
                {
                    _stack2.Push(new Pozicia()
                    {
                        Riadok = i,
                        Stlpec = index
                    });
                }
                if (riadok.CharAt(index) == '[')
                {
                    _stack3.Push(new Pozicia()
                    {
                        Riadok = i,
                        Stlpec = index
                    });
                }

                if (riadok.CharAt(index) == ')' && _stack1.Count > 0)
                {
                    var z = new Zatvorka()
                    {
                        Start = _stack1.Pop(),
                        End = new Pozicia()
                        {
                            Riadok = i,
                            Stlpec = index
                        }
                    };

                    r.Add(index, z);
                    r.Add(z.Start.Stlpec, z);
                }

                if (riadok.CharAt(index) == '}' && _stack2.Count > 0)
                {
                    var z = new Zatvorka()
                    {
                        Start = _stack2.Pop(),
                        End = new Pozicia()
                        {
                            Riadok = i,
                            Stlpec = index
                        }
                    };

                    r.Add(index, z);
                    r.Add(z.Start.Stlpec, z);
                }

                if (riadok.CharAt(index) == ']' && _stack3.Count > 0)
                {
                    var z = new Zatvorka()
                    {
                        Start = _stack3.Pop(),
                        End = new Pozicia()
                        {
                            Riadok = i,
                            Stlpec = index
                        }
                    };

                    r.Add(index, z);
                    r.Add(z.Start.Stlpec, z);
                }

                index++;
            }

            return r;
        }
    }
}
