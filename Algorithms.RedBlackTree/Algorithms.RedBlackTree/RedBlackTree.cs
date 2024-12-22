using System;

namespace Algorithms.RedBlackTree
{
    public enum NodeColor
    {
        Red,
        Black
    }

    public class RedBlackTreeNode<T> where T : IComparable
    {
        public T Value { get; set; }
        public NodeColor Color { get; set; }
        public RedBlackTreeNode<T> Left { get; set; }
        public RedBlackTreeNode<T> Right { get; set; }
        public RedBlackTreeNode<T> Parent { get; set; }

        public RedBlackTreeNode(T value)
        {
            Value = value;
            Color = NodeColor.Red;
            Left = null;
            Right = null;
            Parent = null;
        }
    }

    public class RedBlackTree<T> where T : IComparable
    {
        private RedBlackTreeNode<T> _root;
        private readonly RedBlackTreeNode<T> _nullNode;

        public RedBlackTree()
        {
            _nullNode = new RedBlackTreeNode<T>(default(T))
            {
                Color = NodeColor.Black
            };
            _root = _nullNode;
        }

        public void Insert(T value)
        {
            var newNode = new RedBlackTreeNode<T>(value)
            {
                Left = _nullNode,
                Right = _nullNode,
                Parent = null
            };

            if (_root == _nullNode)
            {
                _root = newNode;
                _root.Color = NodeColor.Black;
                return;
            }

            var current = _root;
            RedBlackTreeNode<T> parent = null;

            while (current != _nullNode)
            {
                parent = current;
                if (newNode.Value.CompareTo(current.Value) < 0)
                {
                    current = current.Left;
                }
                else
                {
                    current = current.Right;
                }
            }

            newNode.Parent = parent;
            if (newNode.Value.CompareTo(parent.Value) < 0)
            {
                parent.Left = newNode;
            }
            else
            {
                parent.Right = newNode;
            }

            FixInsert(newNode);
        }

        private void FixInsert(RedBlackTreeNode<T> node)
        {
            while (node != _root && node.Parent.Color == NodeColor.Red) // 1, 2
            {
                if (node.Parent == node.Parent.Parent.Left) // 3
                {
                    var uncle = node.Parent.Parent.Right;
                    if (uncle.Color == NodeColor.Red) // 3.1
                    {
                        node.Parent.Color = NodeColor.Black;
                        uncle.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        node = node.Parent.Parent;
                    }
                    else // 3.2
                    {
                        if (node == node.Parent.Right) // 3.2.1
                        {
                            node = node.Parent;
                            LeftRotate(node);
                        }

                        // 3.2.2
                        node.Parent.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        RightRotate(node.Parent.Parent);
                    }
                }
                else // 4 
                {
                    var uncle = node.Parent.Parent.Left;
                    if (uncle.Color == NodeColor.Red)
                    {
                        // Case 1: Uncle is red
                        node.Parent.Color = NodeColor.Black;
                        uncle.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        node = node.Parent.Parent;
                    }
                    else
                    {
                        if (node == node.Parent.Left)
                        {
                            // Case 2: Uncle is black, node is left child
                            node = node.Parent;
                            RightRotate(node);
                        }

                        // Case 3: Uncle is black, node is right child
                        node.Parent.Color = NodeColor.Black;
                        node.Parent.Parent.Color = NodeColor.Red;
                        LeftRotate(node.Parent.Parent);
                    }
                }
            }

            _root.Color = NodeColor.Black;
        }

        public void Delete(T value)
        {
            var nodeToDelete = Search(value);
            if (nodeToDelete == _nullNode) return;

            var y = nodeToDelete;
            var yOriginalColor = y.Color;
            RedBlackTreeNode<T> x;

            if (nodeToDelete.Left == _nullNode)
            {
                x = nodeToDelete.Right;
                Transplant(nodeToDelete, nodeToDelete.Right);
            }
            else if (nodeToDelete.Right == _nullNode)
            {
                x = nodeToDelete.Left;
                Transplant(nodeToDelete, nodeToDelete.Left);
            }
            else
            {
                y = Minimum(nodeToDelete.Right);
                yOriginalColor = y.Color;
                x = y.Right;
                if (y.Parent == nodeToDelete)
                {
                    x.Parent = y;
                }
                else
                {
                    Transplant(y, y.Right);
                    y.Right = nodeToDelete.Right;
                    y.Right.Parent = y;
                }

                Transplant(nodeToDelete, y);
                y.Left = nodeToDelete.Left;
                y.Left.Parent = y;
                y.Color = nodeToDelete.Color;
            }

            if (yOriginalColor == NodeColor.Black)
            {
                FixDelete(x);
            }
        }

        private void FixDelete(RedBlackTreeNode<T> node)
        {
            while (node != _root && node.Color == NodeColor.Black)
            {
                if (node == node.Parent.Left)
                {
                    var sibling = node.Parent.Right;
                    if (sibling.Color == NodeColor.Red)
                    {
                        sibling.Color = NodeColor.Black;
                        node.Parent.Color = NodeColor.Red;
                        LeftRotate(node.Parent);
                        sibling = node.Parent.Right;
                    }

                    if (sibling.Left.Color == NodeColor.Black && sibling.Right.Color == NodeColor.Black)
                    {
                        sibling.Color = NodeColor.Red;
                        node = node.Parent;
                    }
                    else
                    {
                        if (sibling.Right.Color == NodeColor.Black)
                        {
                            sibling.Left.Color = NodeColor.Black;
                            sibling.Color = NodeColor.Red;
                            RightRotate(sibling);
                            sibling = node.Parent.Right;
                        }

                        sibling.Color = node.Parent.Color;
                        node.Parent.Color = NodeColor.Black;
                        sibling.Right.Color = NodeColor.Black;
                        LeftRotate(node.Parent);
                        node = _root;
                    }
                }
                else
                {
                    var sibling = node.Parent.Left;
                    if (sibling.Color == NodeColor.Red)
                    {
                        sibling.Color = NodeColor.Black;
                        node.Parent.Color = NodeColor.Red;
                        RightRotate(node.Parent);
                        sibling = node.Parent.Left;
                    }

                    if (sibling.Left.Color == NodeColor.Black && sibling.Right.Color == NodeColor.Black)
                    {
                        sibling.Color = NodeColor.Red;
                        node = node.Parent;
                    }
                    else
                    {
                        if (sibling.Left.Color == NodeColor.Black)
                        {
                            sibling.Right.Color = NodeColor.Black;
                            sibling.Color = NodeColor.Red;
                            LeftRotate(sibling);
                            sibling = node.Parent.Left;
                        }

                        sibling.Color = node.Parent.Color;
                        node.Parent.Color = NodeColor.Black;
                        sibling.Left.Color = NodeColor.Black;
                        RightRotate(node.Parent);
                        node = _root;
                    }
                }
            }

            node.Color = NodeColor.Black;
        }

        private void Transplant(RedBlackTreeNode<T> u, RedBlackTreeNode<T> v)
        {
            if (u.Parent == null)
            {
                _root = v;
            }
            else if (u == u.Parent.Left)
            {
                u.Parent.Left = v;
            }
            else
            {
                u.Parent.Right = v;
            }

            v.Parent = u.Parent;
        }

        private RedBlackTreeNode<T> Minimum(RedBlackTreeNode<T> node)
        {
            while (node.Left != _nullNode)
            {
                node = node.Left;
            }

            return node;
        }

        public RedBlackTreeNode<T> Search(T value)
        {
            var current = _root;
            while (current != _nullNode)
            {
                if (value.CompareTo(current.Value) == 0)
                {
                    return current;
                }
                current = value.CompareTo(current.Value) < 0 ? current.Left : current.Right;
            }

            return _nullNode;
        }

        public void InOrderTraversal(Action<T> action)
        {
            InOrderTraversal(_root, action);
        }

        private void InOrderTraversal(RedBlackTreeNode<T> node, Action<T> action)
        {
            if (node == null || node == _nullNode) return;

            InOrderTraversal(node.Left, action);
            action(node.Value);
            InOrderTraversal(node.Right, action);
        }

        private void LeftRotate(RedBlackTreeNode<T> node)
        {
            var temp = node.Right;
            node.Right = temp.Left;

            if (temp.Left != _nullNode)
            {
                temp.Left.Parent = node;
            }

            temp.Parent = node.Parent;

            if (node.Parent == null)
            {
                _root = temp;
            }
            else if (node == node.Parent.Left)
            {
                node.Parent.Left = temp;
            }
            else
            {
                node.Parent.Right = temp;
            }

            temp.Left = node;
            node.Parent = temp;
        }

        private void RightRotate(RedBlackTreeNode<T> node)
        {
            var temp = node.Left;
            node.Left = temp.Right;

            if (temp.Right != _nullNode)
            {
                temp.Right.Parent = node;
            }

            temp.Parent = node.Parent;

            if (node.Parent == null)
            {
                _root = temp;
            }
            else if (node == node.Parent.Right)
            {
                node.Parent.Right = temp;
            }
            else
            {
                node.Parent.Left = temp;
            }

            temp.Right = node;
            node.Parent = temp;
        }
    }
}