using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_DancingLinks
{
    public class Sudoku
    {
        private List<int[]> _values;
        /// <summary>
        /// 数独数组
        /// </summary>
        public List<int[]> Values { get => _values; }

        /// <summary>
        /// 数独行数
        /// </summary>
        public int Size { get => _values.Count; }

        private Tuple<List<bool[]>, List<Tuple<int, int, int>>> _matrix;
        /// <summary>
        /// 数独矩阵
        /// </summary>
        public Tuple<List<bool[]>, List<Tuple<int, int, int>>> Matrix { get => _matrix; }

        /// <summary>
        /// 构造器
        /// </summary>
        /// <param name="sList"></param>
        public Sudoku(List<int[]> sList)
        {
            _values = new List<int[]>(sList);
            CreateMatrix();
        }

        /// <summary>
        /// 构建数独矩阵
        /// </summary>
        private void CreateMatrix()
        {
            List<bool[]> M = new List<bool[]>();
            List<Tuple<int, int, int>> R = new List<Tuple<int, int, int>>();

            for (int row = 0; row < Size; row++)
            {
                for (int column = 0; column < Size; column++)
                {
                    if(Values[row][column] == 0)
                    {
                        for (int value = 1; value <= Size; value++)
                        {
                            M.Add(new bool[Size * Size * 4]);
                            SetMatrixValues(M[M.Count - 1], Size, row, column, value);
                            R.Add(new Tuple<int, int, int>(row, column, value));
                        }
                    }
                    else
                    {
                        M.Add(new bool[Size * Size * 4]);
                        SetMatrixValues(M[M.Count - 1], Size, row, column, Values[row][column]);
                        R.Add(new Tuple<int, int, int>(row, column, Values[row][column]));
                    }
                }
            }
            _matrix = new Tuple<List<bool[]>, List<Tuple<int, int, int>>>(M, R);
        }

        /// <summary>
        /// 设置矩阵值
        /// </summary>
        /// <param name="mRow"></param>
        /// <param name="size"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="value"></param>
        private void SetMatrixValues(bool[] mRow ,int size ,int row ,int column,int value)
        {
            int regionSize = (int)Math.Sqrt(size);
            int regionNum = (int)Math.Floor((double)row / (double)regionSize) * regionSize + (int)Math.Floor((double)column / (double)regionSize);
            int regionConstraint = size * size * 3 + regionNum * size + (value - 1);

            mRow[row * size + column] = true;
            mRow[size * size + row * size + (value - 1)] = true;
            mRow[size * size * 2 + column * size + (value - 1)] = true;
            mRow[regionConstraint] = true;
        }
    }
}
