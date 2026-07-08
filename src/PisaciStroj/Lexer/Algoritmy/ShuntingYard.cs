using System.Collections.Generic;

namespace Lexer.Algoritmy
{
    public class Operand
    {
        public char Symbol { get; set; }

        public RegexAstNode TreeNode { get; set; }
    }

    public class ShuntingYard
    {
        private Stack<char> _operatorStack;
        private Stack<Operand> _operandStack;

        public ShuntingYard()
        {
            _operatorStack = new Stack<char>();
            _operandStack = new Stack<Operand>();
        }

        public RegexAstTree BuildTree(string regex)
        {
            var poziciaHlavy = 0;

            while (regex.Length > poziciaHlavy)
            {
                var token = regex[poziciaHlavy];

                switch (token)
                {
                    case '(':
                        _operatorStack.Push(token);
                        break;
                    case '*':
                        var operand = _operandStack.Pop();
                        _operandStack.Push(new Operand()
                        {
                            TreeNode = new RegexAstNode()
                            {
                                Key = '*',
                                IsNullable = true,
                                Left = operand.TreeNode ?? new RegexAstNode()
                                {
                                    Key = operand.Symbol
                                }
                            }
                        });
                        break;
                    case '.':
                        _operatorStack.Push(token);
                        break;
                    case '|':
                        if (_operatorStack.Count == 0)
                        {
                            _operatorStack.Push(token);
                        }
                        else
                        {
                            var op = _operatorStack.Pop();
                            if (op == '.')
                            {
                                var operandRight = _operandStack.Pop();
                                var operandLeft = _operandStack.Pop();
                                var treeNode = new RegexAstNode()
                                {
                                    Key = op,
                                    Left = operandLeft.TreeNode ?? new RegexAstNode()
                                    {
                                        Key = operandLeft.Symbol
                                    },
                                    Right = operandRight.TreeNode ?? new RegexAstNode()
                                    {
                                        Key = operandRight.Symbol
                                    }
                                };

                                if (treeNode.Left.IsNullable && treeNode.Right.IsNullable)
                                {
                                    treeNode.IsNullable = true;
                                }

                                _operandStack.Push(new Operand()
                                {
                                    TreeNode = treeNode
                                });

                                _operatorStack.Push(token);
                            }
                            else
                            {
                                _operatorStack.Push(op);
                                _operatorStack.Push(token);
                            }
                        }
                        break;
                    case ')':

                        while (true)
                        {
                            var ope = _operatorStack.Pop();
                            var operandRight = _operandStack.Pop();
                            var operandLeft = _operandStack.Pop();
                            var treeNode = new RegexAstNode()
                            {
                                Key = ope,
                                Left = operandLeft.TreeNode ?? new RegexAstNode()
                                {
                                    Key = operandLeft.Symbol
                                },
                                Right = operandRight.TreeNode ?? new RegexAstNode()
                                {
                                    Key = operandRight.Symbol
                                }
                            };

                            if (ope == '.' && treeNode.Left.IsNullable && treeNode.Right.IsNullable)
                            {
                                treeNode.IsNullable = true;
                            }

                            if (ope == '|' && (treeNode.Left.IsNullable || treeNode.Right.IsNullable))
                            {
                                treeNode.IsNullable = true;
                            }

                            _operandStack.Push(new Operand()
                            {
                                TreeNode = treeNode
                            });

                            if (_operatorStack.Peek() == '(')
                            {
                                _operatorStack.Pop();
                                break;
                            }
                        }

                        break;
                    default:

                        if (token == '\\')
                        {
                            poziciaHlavy += 1;
                            token = regex[poziciaHlavy];
                        }

                        _operandStack.Push(new Operand()
                        {
                            Symbol = token
                        });
                        break;
                }

                poziciaHlavy++;
            }

            var result = BuildTree();

            return result;
        }

        private RegexAstTree BuildTree()
        {
            if (_operatorStack.Count == 0)
            {
                var ope = _operandStack.Pop();

                return new RegexAstTree()
                {
                    Root = ope.TreeNode ?? new RegexAstNode()
                    {
                        Key = ope.Symbol
                    }
                };
            }

            var op = _operatorStack.Pop();
            var operandRight = _operandStack.Pop();
            var operandLeft = _operandStack.Pop();

            var tree = new RegexAstTree()
            {
                Root = new RegexAstNode()
                {
                    Key = op,
                    Left = operandLeft.TreeNode ?? new RegexAstNode()
                    {
                        Key = operandLeft.Symbol
                    },
                    Right = operandRight.TreeNode ?? new RegexAstNode()
                    {
                        Key = operandRight.Symbol
                    }
                }
            };

            if (op == '.' && tree.Root.Left.IsNullable && tree.Root.Right.IsNullable)
            {
                tree.Root.IsNullable = true;
            }

            if (op == '|' && (tree.Root.Left.IsNullable || tree.Root.Right.IsNullable))
            {
                tree.Root.IsNullable = true;
            }

            while (_operatorStack.Count > 0)
            {
                var rightSubTree = tree.Root;

                op = _operatorStack.Pop();
                operandLeft = _operandStack.Pop();

                tree.Root = new RegexAstNode()
                {
                    Key = op,
                    Left = operandLeft.TreeNode ?? new RegexAstNode()
                    {
                        Key = operandLeft.Symbol
                    },
                    Right = rightSubTree
                };

                if (op == '.' && tree.Root.Left.IsNullable && tree.Root.Right.IsNullable)
                {
                    tree.Root.IsNullable = true;
                }

                if (op == '|' && (tree.Root.Left.IsNullable || tree.Root.Right.IsNullable))
                {
                    tree.Root.IsNullable = true;
                }
            }

            return tree;
        }
    }
}
