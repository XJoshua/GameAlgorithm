using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace My_DancingLinks
{
    class Program
    {
        static void Main(string[] args)
        {
            List<int[]> sList = new List<int[]>()
            {
                new int[] { 0, 0, 5, 3, 0, 0, 0, 0, 0 },
                new int[] { 8, 0, 0, 0, 0, 0, 0, 2, 0 },
                new int[] { 0, 7, 0, 0, 1, 0, 5, 0, 0 },
                new int[] { 4, 0, 0, 0, 0, 5, 3, 0, 0 },
                new int[] { 0, 1, 0, 0, 7, 0, 0, 0, 6 },
                new int[] { 0, 0, 3, 2, 0, 0, 0, 8, 0 },
                new int[] { 0, 6, 0, 5, 0, 0, 0, 0, 9 },
                new int[] { 0, 0, 4, 0, 0, 0, 0, 3, 0 },
                new int[] { 0, 0, 0, 0, 0, 9, 7, 0, 0 }
            };

            Sudoku sudoku = new Sudoku(sList);

            List<Node<bool>> results = DancingLinks(new TorodialDoubleLinkList<bool>(sudoku.Size * sudoku.Size * 4).ProcessMatrix(sudoku.Matrix.Item1));

            foreach (Node<bool> result in results)
            {
                Tuple<int, int, int> rcv = sudoku.Matrix.Item2[result.Index];
                sudoku.Values[rcv.Item1][rcv.Item2] = rcv.Item3;
            }

            Console.WriteLine("\n\n");

            for (int row = 0; row < sudoku.Size; row++)
            {
                for (int col = 0; col < sudoku.Size; col++)
                {
                    Console.Write(sudoku.Values[row][col] + " ");
                    if (col % 3 == 2 && col + 1 != sudoku.Size)
                    {
                        Console.Write("| ");
                    }
                        
                }
                Console.WriteLine();
                if (row % 3 == 2 && row + 1 != sudoku.Size)
                {
                    Console.WriteLine(new string('-', (sudoku.Size + 2) * 2));
                }
            }

            Console.ReadLine();
        }

        static List<Node<bool>> DancingLinks(TorodialDoubleLinkList<bool> list)
        {
            List<Node<bool>> solutions = new List<Node<bool>>();
            ColumnNode<bool> column = list.H;

            return Search(list, column, solutions);
        }

        static List<Node<bool>> Search(TorodialDoubleLinkList<bool> list, ColumnNode<bool> column, List<Node<bool>> solutions)
        {
            if (list.H.Right == list.H)
            {
                foreach (Node<bool> result in solutions)
                {
                    Console.Write(result.ColumnNode.ID + "," + result.Index + " ");
                }
                Console.WriteLine();
                return solutions;
            }
            else
            {
                column = getNextColumn(list);
                Cover(column);

                Node<bool> rowNode = column;

                while (rowNode.Down != column)
                {
                    rowNode = rowNode.Down;

                    solutions.Add(rowNode);

                    Node<bool> rightNode = rowNode;

                    while (rightNode.Right != rowNode)
                    {
                        rightNode = rightNode.Right;

                        Cover(rightNode);
                    }

                    List<Node<bool>> result = Search(list, column, solutions);

                    if (result != null) return result;

                    solutions.Remove(rowNode);
                    column = rowNode.ColumnNode;

                    Node<bool> leftNode = rowNode;

                    while (leftNode.Left != rowNode)
                    {
                        leftNode = leftNode.Left;

                        Uncover(leftNode);
                    }
                }

                Uncover(column);
            }

            return null;
        }

        static ColumnNode<bool> getNextColumn(TorodialDoubleLinkList<bool> list)
        {
            ColumnNode<bool> node = list.H;
            ColumnNode<bool> chosenNode = null;

            while (node.Right != list.H)
            {
                node = (ColumnNode<bool>)node.Right;

                if (chosenNode == null || node.Size < chosenNode.Size) chosenNode = node;
            }

            return chosenNode;
        }

        static void Cover(Node<bool> node)
        {
            ColumnNode<bool> column = node.ColumnNode;

            column.RemoveHorizontal();

            Node<bool> verticalNode = column;

            while (verticalNode.Down != column)
            {
                verticalNode = verticalNode.Down;

                Node<bool> removeNode = verticalNode;

                while (removeNode.Right != verticalNode)
                {
                    removeNode = removeNode.Right;

                    removeNode.RemoveVertical();
                }
            }
        }

        static void Uncover(Node<bool> node)
        {
            ColumnNode<bool> column = node.ColumnNode;
            Node<bool> verticalNode = column;

            while (verticalNode.Up != column)
            {
                verticalNode = verticalNode.Up;

                Node<bool> removeNode = verticalNode;

                while (removeNode.Left != verticalNode)
                {
                    removeNode = removeNode.Left;

                    removeNode.ReplaceVertical();
                }
            }

            column.ReplaceHorizontal();
        }
    }
}
