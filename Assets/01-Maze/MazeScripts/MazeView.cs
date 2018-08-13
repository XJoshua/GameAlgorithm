using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.UI;

public class MazeView : MonoBehaviour
{
    public Sprite[] CellSprites;

    public int Width = 200;
    public int Height = 100;

    public int cellSize = 20;

    public Image[,] CellImage;
    public Dictionary<Point, Image> CellMarks;

    public Transform CellParent;

    public Dictionary<Point, Image> cellDict;

    public Transform InitBtn;
    public Transform BfsBtn;
    public Transform DfsBtn;
    public Transform AstarBtn;

    private MazePathFinder finder;

    private MazeCreator creator;

    public Point StartPoint;
    public Point EndPoint;

    void Start()
    {
        ColorCells = new Color[Width, Height];

        creator = new MazeCreator(Width, Height, UseReferImage);

        CellImage = new Image[Width, Height];
        CellMarks = new Dictionary<Point, Image>();

        MazeInit();

        // 显示单元设置
        creator.SetLinkAct += ShowCell;

    }

    #region ------------------- Btn Events -------------------

    public void OnBfsClick()
    {
        finder.FindAlgo = FindAlgo.Bfs;
        CleanStore();
        StartCoroutine(finder.Finding_BFS());
    }

    public void OnDfsClick()
    {
        finder.FindAlgo = FindAlgo.Dfs;
        CleanStore();
        StartCoroutine(finder.Finding_DFS());
    }

    public void OnAstarClick()
    {
        finder.FindAlgo = FindAlgo.AStar;
        CleanStore();
        StartCoroutine(finder.Finding_AStar());
    }

    // 重新生成迷宫
    public void OnMazeInit()
    {
        StartCreate();

        if (UseReferImage)
        {
            ReadImage();
            FoundStartAndEnd();
        }
        else
        {
            creator.StartPoint = new Point(0, 0);
            creator.EndPoint = new Point(Width - 1, Height - 1);
        }

        StartCoroutine(creator.CreatMazeWithAnima(ShowMaze));

        finder = new MazePathFinder(creator, ShowCheckedCell, ShowPath);

        //FinishCreate();
    }

    #endregion ------------------- Btn Events -------------------

    public void ShowMaze(Cell[,] cells)
    {
        FinishCreate();
        return;
        StartCoroutine(CreatMazeImage(cells));
    }

    IEnumerator CreatMazeImage(Cell[,] cells)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                //Image temp = Instantiate(new GameObject(), CellParent);
                GameObject go = new GameObject();
                Image temp = go.AddComponent<Image>();
                temp.transform.SetParent(CellParent);
                temp.rectTransform.anchoredPosition = new Vector2( - (Width * 0.5f * cellSize) + i * cellSize, -(Height * 0.5f * cellSize) + j * cellSize);
                CellImage[i, j] = temp;
                ShowCell(cells[i, j], new Point(i, j));
                yield return null;
            }
        }
    }

    /// <summary>
    /// 初始化迷宫
    /// </summary>
    public void MazeInit()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                GameObject go = new GameObject();
                Image temp = go.AddComponent<Image>();
                temp.transform.SetParent(CellParent);
                temp.rectTransform.anchoredPosition = new Vector2(-(Width * 0.5f * cellSize) + i * cellSize, -(Height * 0.5f * cellSize) + 30 + j * cellSize);
                CellImage[i, j] = temp;
                temp.sprite = CellSprites[0];
                temp.SetNativeSize();
                temp.transform.localScale = new Vector3(cellSize * 0.1f, cellSize * 0.1f, 0);
            }
        }
    }

    public void ShowCell(Cell cell, Point point)
    {
        int linkCount = 0;

        Image temp = CellImage[point.x, point.y];
        temp.transform.rotation = Quaternion.identity;

        foreach (var item in cell.links)
        {
            if (item) linkCount++;
        }

        // 犯了个蠢，不应该用上下左右的顺序
        switch (linkCount)
        {
            case 0:
                temp.sprite = CellSprites[0];
                break;
            case 1:
                temp.sprite = CellSprites[1];
                if(cell.links[1]) temp.transform.localEulerAngles = Vector3.forward * 180;
                if(cell.links[2]) temp.transform.localEulerAngles = Vector3.forward * 90;
                if(cell.links[3]) temp.transform.localEulerAngles = Vector3.forward * -90;
                break;
            case 2:
                if(cell.links[0] && cell.links[1])
                    temp.sprite = CellSprites[2];
                else if (cell.links[2] && cell.links[3])
                {
                    temp.sprite = CellSprites[2];
                    temp.transform.localEulerAngles = Vector3.forward * 90;
                }
                else
                {
                    temp.sprite = CellSprites[5];
                    if (cell.links[3] && cell.links[1]) temp.transform.localEulerAngles = Vector3.forward * -90;
                    if (cell.links[1] && cell.links[2]) temp.transform.localEulerAngles = Vector3.forward * 180;
                    if (cell.links[2] && cell.links[0]) temp.transform.localEulerAngles = Vector3.forward * 90;
                }
                break;
            case 3:
                temp.sprite = CellSprites[3];
                if (!cell.links[0]) temp.transform.localEulerAngles = Vector3.forward * 180;
                if (!cell.links[2]) temp.transform.localEulerAngles = Vector3.forward * -90;
                if (!cell.links[3]) temp.transform.localEulerAngles = Vector3.forward * 90;
                break;
            case 4:
                temp.sprite = CellSprites[4];
                break;
        }
        temp.SetNativeSize();
        temp.transform.localScale = new Vector3(cellSize * 0.1f, cellSize * 0.1f, 0);
        if(UseReferImage)
            temp.color = ColorCells[point.x, point.y];
    }

    private void StartCreate()
    {
        MazeImageReset();

        InitBtn.gameObject.SetActive(false);
        BfsBtn.gameObject.SetActive(false);
        DfsBtn.gameObject.SetActive(false);
        AstarBtn.gameObject.SetActive(false);
    }

    public void FinishCreate()
    {
        InitBtn.gameObject.SetActive(true);
        BfsBtn.gameObject.SetActive(true);
        DfsBtn.gameObject.SetActive(true);
        AstarBtn.gameObject.SetActive(true);
    }

    public void MazeImageReset()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                CellImage[i, j].sprite = CellSprites[0];
            }
        }
    }

    public void ShowCheckedCell(Point point)
    {
        Image temp;

        if (CellMarks.ContainsKey(point))
        {
            temp = CellMarks[point];
        }
        else
        {
            GameObject go = new GameObject();
            temp = go.AddComponent<Image>();
            temp.transform.SetParent(CellParent);
            temp.transform.SetAsFirstSibling();
            temp.rectTransform.anchoredPosition = new Vector2(-(Width * 0.5f * cellSize) + point.x * cellSize, -(Height * 0.5f * cellSize) + 30 + point.y * cellSize);
            CellMarks[point] = temp;
            temp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 10);
            temp.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 10);
            temp.transform.localScale = new Vector3(cellSize * 0.1f, cellSize * 0.1f, 0);
        }
        
        temp.color = Color.red;
    }

    public void ShowPath()
    {
        StartCoroutine(ShowPathAnima());
    }

    public IEnumerator ShowPathAnima()
    {
        foreach (var item in CellMarks)
        {
            item.Value.color = new Color(0, 0, 0, 0);
        }

        Point temp = creator.EndPoint;

        CellMarks[temp].color = Color.red;

        while (creator.cells[temp.x, temp.y].LastCell != null)
        {
            temp = (Point)creator.cells[temp.x, temp.y].LastCell;

            CellMarks[temp].color = Color.red;

            yield return null;
        }
    }

    public void CleanStore()
    {
        finder.CleanStore();

        for (int i = 0; i < creator.cells.GetLength(0); i++)
        {
            for (int j = 0; j < creator.cells.GetLength(1); j++)
            {
                creator.cells[i, j].LastCell = null;
                creator.cells[i, j].Gweight = Int32.MaxValue;
                if (CellMarks.ContainsKey(new Point(i, j)))
                    CellMarks[new Point(i, j)].color = new Color(0, 0, 0, 0);
            }
        }
    }

    public bool UseReferImage;

    public Texture2D ReferImage;

    public bool[,] UsableCells;
    public Color[,] ColorCells;

    /// <summary>
    /// 读取图片设置迷宫节点
    /// </summary>
    public void ReadImage()
    {
        UsableCells = new bool[Width, Height];
        creator.UseCells = new bool[Width, Height];

        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                int tempW = (int)(i * ReferImage.width / Width);
                int tempH = (int)(j * ReferImage.height / Height);
                //Debug.Log(tempW + " | " + tempH);
                Color temp = ReferImage.GetPixel(tempW, tempH);
                
                if (temp.a >= 0.1) creator.UseCells[i, j] = true;
                ColorCells[i,j] = temp;

                CellImage[i, j].color = temp;

                //Debug.Log(string.Format("({0},{1}|{2}|{3})",i, j, creator.UseCells[i, j], temp));

            }
        }
    }

    public void FoundStartAndEnd()
    {
        bool getStartPoint = false;
        for (int i = 0; i < Width; i++)
        {
            if (getStartPoint) break;
            for (int j = 0; j < Height; j++)
            {
                if (!creator.UseCells[i, j]) continue;
                creator.StartPoint = new Point(i, j);
                getStartPoint = true;
                break;
            }
        }

        bool getEndPoint = false;
        for (int i = Width - 1; i > 0; i--)
        {
            if (getEndPoint) break;
            for (int j = Height - 1; j > 0; j--)
            {
                if (!creator.UseCells[i, j]) continue;
                creator.EndPoint = new Point(i, j);
                getEndPoint = true;
                break;
            }
        }

        Debug.Log("StartPos: " + creator.StartPoint + " | EndPos: " + creator.EndPoint);
    }


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Captured");
            Application.CaptureScreenshot("screen.png", 0);
        }
    }

}

public enum WallType
{
    NoWay = 0,
    OneWay = 1,
    Channel = 2,
    ThreeWay = 3,
    CrossRoad = 4,
    Corner = 5,
}
