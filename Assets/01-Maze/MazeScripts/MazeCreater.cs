using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Mono.Cecil;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Cell
{
    // 四周的连通性
    // 0 上， 1 下， 2 左， 3 右
    public bool[] links = new bool[4];

    // 到开始点最近路线的上一个点
    public Point? LastCell = null;
    
    // 当前点到起点的距离
    public int Gweight = int.MaxValue;

    /// <summary>
    /// 判断连通性
    /// </summary>
    public void CheckLink()
    {
        // todo: not finish
    }

    /// <summary>
    /// 设置连通性
    /// </summary>
    public void SetLink(Dir dir)
    {
        //Debug.Log(dir);
        links[(int) dir] = true;

        // 修改连通，设置修改图片动画
        //if (action != null) action();
    }
}

public enum Dir
{
    Up = 0,
    Down = 1,
    Left = 2,
    Right = 3,
}

public class Maze
{
    public int height;
    public int width;
}

public struct Point
{
    public int x;
    public int y;

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public void Add(Point point)
    {
        this.x += point.x;
        this.y += point.y;
    }

    public static Point operator +(Point a, Point b)
    {
        return new Point(a.x + b.x , a.y + b.y);
    }

    public static Point operator -(Point a, Point b)
    {
        return new Point(b.x - a.x, b.y - a.y);
    }

    public static bool operator ==(Point a, Point b)
    {
        return a.x == b.x && a.y == b.y;
    }

    public static bool operator !=(Point a, Point b)
    {
        return a.x != b.x || a.y != b.y;
    }

    public static Point Up()
    {
        return new Point(0, 1);
    }
    public static Point Down()
    {
        return new Point(0, -1);
    }
    public static Point Left()
    {
        return new Point(-1, 0);
    }
    public static Point Right()
    {
        return new Point(1, 0);
    }

    public static Point[] DirPoints = new Point[] {Point.Up(), Point.Down(), Point.Left(), Point.Right()};

    public override string ToString()
    {
        return string.Format("({0}, {1})", x, y);
    }
}


public class MazeCreator
{
    public int Height;
    public int Width;
    public bool[,] UseCells;
    // 所有的格子
    public Cell[,] cells;
    // 标记矩阵
    public bool[,] MarkedArr;
    // 当前的格子
    public Point curIndex;
    // 访问过的格子栈
    public Stack<Point> cellStack;

    public Action<Cell, Point> SetLinkAct;

    // 开始位置和结束位置
    public Point StartPoint;

    public Point EndPoint;

    public MazeCreator(int x, int y)
    {
        this.Height = y;
        this.Width = x;
        //this.UseCells = useCells;
    }

    /// <summary>
    /// 开始创建迷宫 Old
    /// </summary>
    public void CreatMaze(Action<Cell[,]> args = null)
    {
        // 初始化
        cells = new Cell[Width, Height];
        MarkedArr = new bool[Width, Height];
        // 初始化初始坐标
        curIndex = new Point(0, 0);
        // 访问过的格子栈
        cellStack = new Stack<Point>();

        CellForSelect = new List<Dir>();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                cells[i,j] = new Cell();
            }
        }

        while (!CheckAllMarked())
        {
            CheckCurCell(curIndex);
            //System.Threading.Thread.Sleep(20);
        }

        Debug.Log("Creator Finish");

        CheckCellLink();

        if (args != null)
            args(cells);
    }

    public IEnumerator CreatMazeWithAnima(Action<Cell[,]> args = null)
    {
        // 初始化
        cells = new Cell[Width, Height];
        MarkedArr = new bool[Width, Height];
        // 初始化初始坐标
        curIndex = StartPoint;
        // 访问过的格子栈
        cellStack = new Stack<Point>();

        CellForSelect = new List<Dir>();

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                cells[i, j] = new Cell();
            }
        }

        while (!CheckAllMarked())
        {
            CheckCurCell(curIndex);
            yield return null;
        }

        Debug.Log("Creator Finish");

        CheckCellLink();

        if (args != null)
            args(cells);

    }

    private readonly System.Random rand = new System.Random();

    public void CheckCurCell(Point curPoint)
    {
        // 标记当前单元
        MarkedArr[curPoint.x, curPoint.y] = true;

        // 检查周边元素
        CheckNearbyMarked(curPoint);

        if (CellForSelect.Count != 0)
        {
            // 如果存在未访问的单元
            int nextPoint = rand.Next(0, CellForSelect.Count);
            // 当前元素入栈
            cellStack.Push(curPoint);
            // 移除当前单元墙壁
            cells[curPoint.x, curPoint.y].SetLink(CellForSelect[nextPoint]);
            // 移除目标单元墙壁
            Point target = curPoint + dirPoints[(int)CellForSelect[nextPoint]];
            //Debug.Log(curPoint + " " + CellForSelect[nextPoint]);
            cells[target.x, target.y].SetLink(antiDirs[(int)CellForSelect[nextPoint]]);
            // 修改图片
            SetLinkAct(cells[curPoint.x, curPoint.y], curPoint);
            SetLinkAct(cells[target.x, target.y], target);
            // 目标单元作为当前单元
            curIndex = target;
        }
        else if(cellStack.Count != 0)
        {
            curIndex = cellStack.Pop();
        }
        else
        {
            Debug.LogError("Something wrong");
            Assert.IsTrue(true);
        }
    }

    /// <summary>
    /// 是否全部被标记
    /// </summary>
    public bool CheckAllMarked()
    {
        for (int i = 0; i < MarkedArr.GetLength(0); i++)
        {
            for (int j = 0; j < MarkedArr.GetLength(1); j++)
            {
                if (!UseCells[i, j]) continue;
                if (!MarkedArr[i, j]) return false;
            }
        }
        return true;
    }

    public List<Dir> CellForSelect;
    
    public Point[] dirPoints = new Point[4] {new Point(0, 1), new Point(0, -1), new Point(-1, 0), new Point(1, 0)};

    public Dir[] dirs = new Dir[4] { Dir.Up, Dir.Down, Dir.Left, Dir.Right };

    public Dir[] antiDirs = new Dir[4] { Dir.Down, Dir.Up, Dir.Right, Dir.Left };
 
    /// <summary>
    /// 判断周边是否被标记
    /// </summary>
    public List<Dir> CheckNearbyMarked(Point index)
    {
        CellForSelect.Clear();
        for (int q = 0; q < 4; q++)
        {
            Point targetPIndex = index + dirPoints[q];
            // 判断越界
            if(targetPIndex.x < 0 || targetPIndex.x >= Width) continue;
            if(targetPIndex.y < 0 || targetPIndex.y >= Height) continue;
            if (!UseCells[targetPIndex.x, targetPIndex.y]) continue;
            if (!CheckMarked(targetPIndex)) CellForSelect.Add(dirs[q]);
        }
        return CellForSelect;
    }

    public bool CheckMarked(Point point)
    {
        return MarkedArr[point.x, point.y];
    }

    public void CheckCellLink()
    {
        string temp = "";
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                bool[] links = cells[i, j].links;
                temp += string.Format("({0},{1})|({2},{3},{4},{5})      ", i, j, b2int(links[0]), b2int(links[1]), b2int(links[2]), b2int(links[3]));
            }
            temp += "\n";
        }
        Debug.Log(temp);
    }

    private int b2int(bool x)
    {
        return x ? 1 : 0;
    }

}
