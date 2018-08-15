using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_DancingLinks
{
    // 基础节点
    public class Node<T>
    {
        // 值？？
        private T _value;
        // 序号
        private int _index;
        // 四向指针
        private Node<T> _left, _right, _up, _down;
        // ？？
        private ColumnNode<T> _columnNode;

        public T Value { get => _value; set => _value = value; }
        public int Index { get => _index;}
        public Node<T> Left { get => _left; set => _left = value; }
        public Node<T> Right { get => _right; set => _right = value; }
        public Node<T> Up { get => _up; set => _up = value; }
        public Node<T> Down { get => _down; set => _down = value; }
        public ColumnNode<T> ColumnNode { get => _columnNode; set => _columnNode = value; }

        public Node(int index)
        {
            _index = index;
        }

        // 在列中移除本元素
        public void RemoveVertical()
        {
            Up.Down = Down;
            Down.Up = Up;

            ColumnNode.DecSize();
        }

        // 在行中移除本元素
        public void RemoveHorizontal()
        {
            Right.Left = Left;
            Left.Right = Right;
        }

        // 列中恢复本元素
        public void ReplaceVertical()
        {
            Up.Down = this;
            Down.Up = this;

            ColumnNode.IncSize();
        }

        // 行中恢复本元素
        public void ReplaceHorizontal()
        {
            Right.Left = this;
            Left.Right = this;
        }
    }

    // 这个是什么元素？
    // 首行，用来判断对应列有没有元素？？
    public class ColumnNode<T> : Node<T>
    {
        int id;
        int size = 0;

        public int ID { get { return id; } }
        public int Size { get { return size; } }

        public ColumnNode(int id) : base(-1)
        {
            this.id = id;
            Up = this;
            Down = this;
            ColumnNode = this;
        }

        internal void IncSize()
        {
            size++;
        }

        internal void DecSize()
        {
            size--;
        }
    }

    // 双向环形链
    public class TorodialDoubleLinkList<T>
    {
        private ColumnNode<T> h = new ColumnNode<T>(-1);
        public ColumnNode<T> H{ get { return h; }}

        List<ColumnNode<T>> columns = new List<ColumnNode<T>>();

        public TorodialDoubleLinkList(int noColumns)
        {
            for (int i = 0; i < noColumns; i++)
            {
                columns.Add(new ColumnNode<T>(i));
            }


            h.Right = columns[0];
            columns[0].Left = h;
            h.Left = columns[noColumns - 1];
            columns[noColumns - 1].Right = h;

            for (int i = 0; i < noColumns - 1; i++)
            {
                columns[i].Right = columns[i + 1];
                columns[i + 1].Left = columns[i];
            }
        }

        void ProcessMatrixRow(List<KeyValuePair<int, Node<T>>> nodes)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].Value.Left = nodes[WrapIndex(i - 1, nodes.Count)].Value;
                nodes[i].Value.Right = nodes[WrapIndex(i + 1, nodes.Count)].Value;

                AddToColumn(nodes[i].Key, nodes[i].Value);
            }
        }

        public TorodialDoubleLinkList<T> ProcessMatrix(List<bool[]> matrix)
        {
            for (int y = 0; y < matrix.Count; y++)
            {
                List<KeyValuePair<int, Node<T>>> nodes = new List<KeyValuePair<int, Node<T>>>();

                for (int x = 0; x < columns.Count; x++)
                {
                    if (matrix[y][x])
                    {
                        nodes.Add(new KeyValuePair<int, Node<T>>(x, new Node<T>(y)));
                    }
                }

                ProcessMatrixRow(nodes);
            }
            return this;
        }

        int WrapIndex(int val, int length)
        {
            if (val >= length) return val - length;
            if (val < 0) return val + length;

            return val;
        }

        public void AddToColumn(int index, Node<T> node)
        {
            Node<T> lowestNode = columns[index].Up;

            lowestNode.Down = node;
            node.Up = lowestNode;
            columns[index].Up = node;
            node.Down = columns[index];
            node.ColumnNode = columns[index];

            columns[index].IncSize();
        }
    }
}
