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
                var rowResult = new Dictionary<int, Zatvorka>();
                result.Add(i, rowResult);

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

                        rowResult.Add(index, z);
                        result[z.Start.Riadok].Add(z.Start.Stlpec, z);
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

                        rowResult.Add(index, z);
                        result[z.Start.Riadok].Add(z.Start.Stlpec, z);
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

                        rowResult.Add(index, z);
                        result[z.Start.Riadok].Add(z.Start.Stlpec, z);
                    }

                    index++;
                }
            }

            return result;
        }
    }
}
