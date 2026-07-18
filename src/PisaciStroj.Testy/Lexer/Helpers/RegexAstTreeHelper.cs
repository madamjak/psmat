using Lexer.Algoritmy;
using System.Collections.Generic;
using System.Text;

namespace PSMat.Testy.Lexer.Helpers
{
    public static class RegexAstTreeHelper
    {
        public static void InorderTraversalPrint(RegexAstNode node, StringBuilder sb)
        {
            if (node == null)
            {
                return;
            }

            sb.Append('(');

            InorderTraversalPrint(node.Left, sb);
            sb.Append(node.Key);
            InorderTraversalPrint(node.Right, sb);

            sb.Append(')');
        }

        public static void InorderTraversalNullablePrint(RegexAstNode node, StringBuilder sb)
        {
            if (node == null)
            {
                return;
            }

            sb.Append('[');

            InorderTraversalNullablePrint(node.Left, sb);
            sb.Append(node.Key);
            if (node.IsNullable)
            {
                sb.Append("-nullable");
            }
            InorderTraversalNullablePrint(node.Right, sb);

            sb.Append(']');
        }

        public static void InorderTraversalPositionsPrint(RegexAstNode node, StringBuilder sb)
        {
            if (node == null)
            {
                return;
            }

            sb.Append('[');

            InorderTraversalPositionsPrint(node.Left, sb);

            if (node.Pos > 0)
            {
                sb.Append(string.Format("position: {0}; ", node.Pos));
            }

            sb.Append("firstPos: ");
            foreach (var p in node.FirstPos)
            {
                sb.Append(string.Format("{0}, ", p));
            }

            sb.Append("lastPos: ");
            foreach (var p in node.LastPos)
            {
                sb.Append(string.Format("{0}, ", p));
            }

            InorderTraversalPositionsPrint(node.Right, sb);

            sb.Append(']');
        }

        public static bool PorovnajFollowPos(RegexAstTree tree, Dictionary<int, char> ocakavanySymbolPos, Dictionary<int, HashSet<int>> ocakavanyFollowPos)
        {
            var symbolPosSpravne = ocakavanySymbolPos.Count == tree.SymbolPos.Count;
            if (symbolPosSpravne)
            {
                foreach (var p in tree.SymbolPos)
                {
                    if (ocakavanySymbolPos[p.Key] != p.Value)
                    {
                        symbolPosSpravne = false;
                        break;
                    }
                }
            }

            var followPosSpravne = ocakavanyFollowPos.Count == tree.FollowPos.Count;
            if (followPosSpravne)
            {
                foreach (var p in tree.FollowPos)
                {
                    if (ocakavanyFollowPos[p.Key].Count != p.Value.Count)
                    {
                        followPosSpravne = false;
                        break;
                    }

                    foreach (var pos in p.Value)
                    {
                        if (!ocakavanyFollowPos[p.Key].Contains(pos))
                        {
                            followPosSpravne = false;
                            break;
                        }
                    }
                }
            }

            return symbolPosSpravne && followPosSpravne;
        }
    }
}
