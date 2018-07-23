using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Api;
using UnityEngine;

public class MazePathFinder
{
    private Cell[,] cells;

    public Action<Point> CheckCells;

    public Action FinishFinding;

    private Queue<Point> searchPoints4Bfs;

    private Stack<Point> searchPoints4Dfs;

    // container for astar
    private List<Point> openPtList;
    private List<Point> closePtList;

    //private bool[,] searchPointMarks;

    private readonly Point startPoint;

    private readonly Point endPoint;

    private Point curPoint;

    public FindAlgo FindAlgo;

    public MazeCreator creator;

    public MazePathFinder(MazeCreator creator, Action<Point> cellAction = null, Action finishAction = null)
    {
        this.cells = creator.cells;

        searchPoints4Bfs = new Queue<Point>();
        searchPoints4Dfs = new Stack<Point>();

        startPoint = creator.StartPoint;
        endPoint = creator.EndPoint;

        CheckCells = cellAction;
        FinishFinding = finishAction;
    }

    /// <summary>
    /// 广度优先搜索
    /// </summary>
    public IEnumerator Finding_BFS()
    {
        Debug.Log("Start Bfs");

        searchPoints4Bfs = new Queue<Point>();

        curPoint = startPoint;
        //endPoint = new Point(cells.GetLength(0) - 1, cells.GetLength(1) - 1);

        if (CheckCells != null) CheckCells(curPoint);

        GetNextCell();

        curPoint = searchPoints4Bfs.Dequeue();

        if (CheckCells != null) CheckCells(curPoint);

        while (curPoint != endPoint)
        {
            GetNextCell();

            if(searchPoints4Bfs.Count != 0)
                curPoint = searchPoints4Bfs.Dequeue();
            else
            {
                Debug.LogError("no point in queue");
                yield break;
            }

            if (CheckCells != null) CheckCells(curPoint);

            //Debug.Log(curPoint);

            yield return null;
        }

        if (FinishFinding != null)
            FinishFinding();
    }

    /// <summary>
    /// 深度优先搜索
    /// </summary>
    public IEnumerator Finding_DFS()
    {
        Debug.Log("Start Dfs");

        searchPoints4Dfs = new Stack<Point>();

        curPoint = startPoint;

        if (CheckCells != null) CheckCells(curPoint);

        while (curPoint != endPoint)
        {
            GetNextCell();

            if (CheckCells != null) CheckCells(curPoint);

            //Debug.Log(curPoint);

            yield return null;
        }

        if (FinishFinding != null)
            FinishFinding();
    }

    public IEnumerator Finding_AStar()
    {
        Debug.Log("Start Astar");

        openPtList = new List<Point>();
        closePtList = new List<Point>();

        curPoint = startPoint;

        cells[curPoint.x, curPoint.y].Gweight = 0;

        openPtList.Add(curPoint);

        while (openPtList.Count != 0 )
        {
            curPoint = GetMinFWeight();

            openPtList.Remove(curPoint);

            closePtList.Add(curPoint);

            if (CheckCells != null) CheckCells(curPoint);

            if (curPoint == endPoint) break;

            GetNextCell_AStar();

            yield return null;
        }

        if (FinishFinding != null)
            FinishFinding();
    }

    private void GetNextCell()
    {
        switch (FindAlgo)
        {
            case FindAlgo.Bfs:
                GetNextCell_Bfs();
                break;
            case FindAlgo.Dfs:
                GetNextCell_Dfs();
                break;
            case FindAlgo.AStar:
                GetNextCell_AStar();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void GetNextCell_Bfs()
    {
        Cell temp = cells[curPoint.x, curPoint.y];

        Point targetPoint;

        for (int i = 0; i < Point.DirPoints.Length; i++)
        {
            targetPoint = curPoint + Point.DirPoints[i];

            if (targetPoint.x < 0 || targetPoint.x > cells.GetLength(0) - 1 || targetPoint.y < 0 ||
                targetPoint.y > cells.GetLength(1) - 1)
                continue;

            if (cells[targetPoint.x, targetPoint.y].LastCell != null)
                continue;

            if (!temp.links[i]) continue;

            cells[targetPoint.x, targetPoint.y].LastCell = curPoint;

            searchPoints4Bfs.Enqueue(targetPoint);
        }
    }

    private void GetNextCell_Dfs()
    {
        Cell temp = cells[curPoint.x, curPoint.y];

        Point targetPoint = new Point(0, 0);

        bool marked = false;

        for (int i = 0; i < Point.DirPoints.Length; i++)
        {
            targetPoint = curPoint + Point.DirPoints[i];

            if (targetPoint.x < 0 || targetPoint.x > cells.GetLength(0) - 1 || targetPoint.y < 0 ||
                targetPoint.y > cells.GetLength(1) - 1)
                continue;

            if (cells[targetPoint.x, targetPoint.y].LastCell != null)
                continue;

            if (!temp.links[i]) continue;

            marked = true;
            break;
        }

        if (marked)
        {
            cells[targetPoint.x, targetPoint.y].LastCell = curPoint;

            searchPoints4Dfs.Push(curPoint);

            curPoint = targetPoint;
        }
        else
        {
            curPoint = searchPoints4Dfs.Pop();
        }
    }

    private void GetNextCell_AStar()
    {
        Cell temp = cells[curPoint.x, curPoint.y];

        for (int i = 0; i < Point.DirPoints.Length; i++)
        {
            Point targetPoint = curPoint + Point.DirPoints[i];

            if (targetPoint.x < 0 || targetPoint.x > cells.GetLength(0) - 1 || targetPoint.y < 0 ||
                targetPoint.y > cells.GetLength(1) - 1)
                continue;

            if (openPtList.Contains(targetPoint) || closePtList.Contains(targetPoint)) continue;

            // if (cells[targetPoint.x, targetPoint.y].LastCell != null) continue;

            if (!temp.links[i]) continue;

            // 如果当前到达的路径更短，更新路径
            if (cells[targetPoint.x, targetPoint.y].Gweight > cells[curPoint.x, curPoint.y].Gweight + 1)
            {
                cells[targetPoint.x, targetPoint.y].LastCell = curPoint;
                cells[targetPoint.x, targetPoint.y].Gweight = cells[curPoint.x, curPoint.y].Gweight + 1;

                openPtList.Add(targetPoint);
            }
        }
    }

    public Point GetMinFWeight()
    {
        int f = Int32.MaxValue;
        Point temp;
        Point minPoint = new Point(0, 0);
        // todo
        for (int i = 0; i < openPtList.Count; i++)
        {
            temp = openPtList[i];
            if (cells[temp.x, temp.y].Gweight + Dis2End(temp, endPoint) < f)
            {
                f = cells[temp.x, temp.y].Gweight + Dis2End(temp, endPoint);
                minPoint = temp;
            }
        }

        return minPoint;
    }

    /// <summary>
    /// 计算当前点到出口的曼哈顿距离
    /// </summary>
    public int Dis2End(Point curPt, Point endPt)
    {
        return Mathf.Abs(endPt.x - curPt.x) + Mathf.Abs(endPt.y - curPt.y);
    }

    public void CleanStore()
    {
        //searchPoints4Bfs = new Queue<Point>();
        //searchPoints4Dfs = new Stack<Point>();
        //openPtDict = new Dictionary<Point, bool>();
        //closePtDict = new Dictionary<Point, bool>();
    }

}

public enum FindAlgo
{
    Bfs,
    Dfs,
    AStar,
}
