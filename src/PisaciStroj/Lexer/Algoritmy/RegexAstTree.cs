using System;
using System.Collections.Generic;
using System.Text;

namespace Lexer.Algoritmy
{
    public class RegexAstTree
    {
        public int CurrentLeafPos { get; private set; }

        /// <summary>
        /// DFA construction properties
        /// </summary>
        public Dictionary<int, HashSet<int>> FollowPos { get; private set; }

        public Dictionary<int, char> SymbolPos { get; private set; }

        public HashSet<char> Symbols { get; private set; }

        public RegexAstNode Root { get; set; }

        public RegexAstTree()
        {
            FollowPos = new Dictionary<int, HashSet<int>>();
            SymbolPos = new Dictionary<int, char>();
            Symbols = new HashSet<char>();
        }

        public void PostorderTraversalToConstructFollowpos()
        {
            PostorderTraversalToConstructFollowpos(Root);
        }

        private void PostorderTraversalToConstructFollowpos(RegexAstNode node)
        {
            if (node == null)
            {
                return;
            }

            if (node.Left == null && node.Right == null)
            {
                CurrentLeafPos++;
                node.Pos = CurrentLeafPos;
                node.FirstPos = new HashSet<int> { CurrentLeafPos };
                node.LastPos = new HashSet<int> { CurrentLeafPos };

                SymbolPos.Add(CurrentLeafPos, node.Key);
                Symbols.Add(node.Key);
            }
            else
            {
                PostorderTraversalToConstructFollowpos(node.Left);
                PostorderTraversalToConstructFollowpos(node.Right);

                //first pos
                if (node.Key == '|')
                {
                    node.FirstPos = new HashSet<int>();
                    foreach (var pos in node.Left.FirstPos)
                    {
                        node.FirstPos.Add(pos);
                    }
                    foreach (var pos in node.Right.FirstPos)
                    {
                        node.FirstPos.Add(pos);
                    }
                }

                if (node.Key == '.')
                {
                    node.FirstPos = new HashSet<int>();
                    foreach (var pos in node.Left.FirstPos)
                    {
                        node.FirstPos.Add(pos);
                    }

                    if (node.Left.IsNullable)
                    {
                        foreach (var pos in node.Right.FirstPos)
                        {
                            node.FirstPos.Add(pos);
                        }
                    }
                }

                if (node.Key == '*')
                {
                    node.FirstPos = new HashSet<int>();
                    foreach (var pos in node.Left.FirstPos)
                    {
                        node.FirstPos.Add(pos);
                    }
                }

                //lastpos
                if (node.Key == '|')
                {
                    node.LastPos = new HashSet<int>();
                    foreach (var pos in node.Left.LastPos)
                    {
                        node.LastPos.Add(pos);
                    }
                    foreach (var pos in node.Right.LastPos)
                    {
                        node.LastPos.Add(pos);
                    }
                }

                if (node.Key == '.')
                {
                    node.LastPos = new HashSet<int>();
                    foreach (var pos in node.Right.LastPos)
                    {
                        node.LastPos.Add(pos);
                    }

                    if (node.Right.IsNullable)
                    {
                        foreach (var pos in node.Left.LastPos)
                        {
                            node.LastPos.Add(pos);
                        }
                    }
                }

                if (node.Key == '*')
                {
                    node.LastPos = new HashSet<int>();
                    foreach (var pos in node.Left.LastPos)
                    {
                        node.LastPos.Add(pos);
                    }
                }

                //follow pos
                if (node.Key == '*')
                {
                    foreach (var p in SymbolPos)
                    {
                        if (node.LastPos.Contains(p.Key))
                        {
                            foreach (var po in node.FirstPos)
                            {
                                if (!FollowPos.ContainsKey(p.Key))
                                {
                                    FollowPos.Add(p.Key, new HashSet<int>() { po });
                                }
                                else
                                {
                                    FollowPos[p.Key].Add(po);
                                }
                            }
                        }
                    }
                }

                if (node.Key == '.')
                {
                    foreach (var p in SymbolPos)
                    {
                        if (node.Left.LastPos.Contains(p.Key))
                        {
                            foreach (var po in node.Right.FirstPos)
                            {
                                if (!FollowPos.ContainsKey(p.Key))
                                {
                                    FollowPos.Add(p.Key, new HashSet<int>() { po });
                                }
                                else
                                {
                                    FollowPos[p.Key].Add(po);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public class RegexAstNode
    {
        public char Key { get; set; }

        public RegexAstNode Left { get; set; }

        public RegexAstNode Right { get; set; }

        /// <summary>
        /// DFA construction properties
        /// </summary>
        public bool IsNullable { get; set; }

        public int Pos { get; set; }

        public HashSet<int> FirstPos { get; set; }

        public HashSet<int> LastPos { get; set; }
    }
}
