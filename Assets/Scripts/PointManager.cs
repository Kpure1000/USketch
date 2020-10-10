using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PointManager : MonoBehaviour
{

    [Tooltip("控制点预制体")]
    public Point pointOrg;

    [Tooltip("插值数目")]
    [Range(2000, 10000)]
    public int lampCount = 1000;

    [Tooltip("控制点数量最大值")]
    public int maxControlPointNumber = 30;

    [Tooltip("操作UI面班管理器")]
    public OperationPanelController operatorUIManager;

    [Header("信息界面")]
    public Text typeT;
    public Text degreeT;
    public Text timesT;
    public Text numT;
    public Text lampT;

    /// <summary>
    /// 是否实际更新了控制点(包括添加和拖动)
    /// </summary>
    public bool IsUpdated { get; set; } = false;

    /**********************************************************/

    private void Awake()
    {
        points = new List<Point>();
        tmpPoints = new List<Vector2>();
        convexHull = new List<Vector2>();
        tmpConvex = new List<Vector2>();
    }

    //private void Start()
    //{
    //    points = new List<Point>();
    //    tmpPoints = new List<Vector2>();
    //}

    private void Update()
    {
        //更新控制点
        UpdatePoint(Input.GetMouseButton((int)MouseButton.LeftMouse));
        //更新曲线数据
        updateCurveData();
        //获取曲线信息
        getCurveInfo(ref type, ref dgree, ref times, ref num, ref lamp);
        //显示曲线信息
        typeT.text = type;
        degreeT.text = dgree;
        timesT.text = times;
        numT.text = num;
        lampT.text = lamp;
    }

    /// <summary>
    /// 重置控制点
    /// </summary>
    public void ResetPoint()
    {
        Vector3 m_mousePosition = Vector2.zero;
        dragPoint = null;
        IsUpdated = false;
        isDraged = false;
        isInRange = false;
        isInserted = false;
        for (int i = 0; i < points.Count; i++)
        {
            Destroy(points[i].gameObject);
        }
        points.Clear();
        tmpPoints.Clear();
        convexHull.Clear();
        tmpConvex.Clear();
        updateCurveData = null;
        getCurveInfo = null;
        //setKnotPoint(isShowPosition);
    }

    /// <summary>
    /// 重新开始绘制控制点
    /// </summary>
    public void RestartPaint()
    {
        setPolygon(isShowPolygon);
        setConvexHull(isShowConvexHull);
    }

    /// <summary>
    /// 更新控制点(添加和拖动)
    /// </summary>
    /// <param name="isPressed">是否按下鼠标左键</param>
    void UpdatePoint(bool isPressed)
    {
        m_mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!isDraged)
        {
            for (int i = 0; i < points.Count; i++)
            {
                //是否在某点的检测范围内
                isInRange = points[i].IsContained;

                //设置高光
                points[i].setHighlight(isInRange);
                if (isInRange && isPressed && !isDraged)
                {
                    //Debug.Log("开始拖动");
                    isDraged = true;
                    dragPoint = points[i];
                    //插入了
                    isInserted = true;
                }
            }
        }
        else
        {
            SetPointPosition(dragPoint, m_mousePosition);
            //更新了控制点
            IsUpdated = true;
        }
        if (isPressed)
        {
            if (points.Count < maxControlPointNumber &&
                !isInserted && !operatorUIManager.isEnter)
            {
                isInserted = true;
                if (points.Count >= 2)
                {
                    InsertPoint(PointType.Normal);
                }
                else
                {
                    InsertPoint(PointType.Edge);
                }
            }
        }
        else
        {
            isDraged = false;
            isInserted = false;
            dragPoint = null;
        }
        //复制到缓冲区内
        for (int i = 0; i < points.Count; i++)
        {
            if (i < tmpPoints.Count)
            {
                tmpPoints[i] = points[i].transform.position;
            }
            else
            {
                tmpPoints.Add(points[i].transform.position);
            }
        }
        //求凸包
        if(isShowConvexHull)
        {
            updateConvexHull();
        }
    }

    /// <summary>
    /// 添加控制点
    /// </summary>
    /// <param name="type">控制点类型</param>
    private void InsertPoint(PointType type)
    {
        Point newPoint = Instantiate(pointOrg);
        newPoint.pointType = type;
        newPoint.pName = 'P' + points.Count.ToString();
        SetPointPosition(newPoint, m_mousePosition);
        points.Add(newPoint);
        Debug.Log("添加了一个控制点,控制点个数: " + points.Count);
        IsUpdated = true;
        ShowPosition(isShowPosition);
    }

    /// <summary>
    /// 设置控制点坐标
    /// </summary>
    /// <param name="point">控制点引用</param>
    /// <param name="pos">新坐标</param>
    private static void SetPointPosition(Point point, Vector3 pos)
    {
        point.transform.position = new Vector3(pos.x, pos.y, 0.0f);
    }

    private void ShowPosition(bool isShow)
    {
        for (int i = 0; i < points.Count; i++)
        {
            points[i].IsShowPosition = isShowPosition;
        }
    }

    /// <summary>
    /// 求凸包
    /// </summary>
    private void updateConvexHull()
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (i < tmpConvex.Count)
            {
                tmpConvex[i] = points[i].transform.position;
            }
            else
            {
                tmpConvex.Add(points[i].transform.position);
            }
        }

        convexHull.Clear();

        if (points.Count <= 3)
        {
            foreach (var item in tmpConvex)
            {
                convexHull.Add(item);
            }
            return;
        }

        //控制点排序
        tmpConvex.Sort(comp);
        
        //下半凸包
        for (int i = 0; i < tmpConvex.Count; i++) 
        {
            while (convexHull.Count > 1 && Cross(convexHull[convexHull.Count - 2],
                convexHull[convexHull.Count - 1], tmpConvex[i]) < 0)
            {
                convexHull.RemoveAt(convexHull.Count - 1);
            }
            convexHull.Add(tmpConvex[i]);
        }
        int k = convexHull.Count;
        //上半凸包
        for (int i = tmpConvex.Count - 2; i >= 0; i--)
        {
            while (convexHull.Count > k && Cross(convexHull[convexHull.Count - 1],
                convexHull[convexHull.Count - 2], tmpConvex[i]) > 0)
            {
                convexHull.RemoveAt(convexHull.Count - 1);
            }
            convexHull.Add(tmpConvex[i]);
        }
        //移除首尾重复点
        convexHull.RemoveAt(convexHull.Count - 1);
    }

    static private float Cross(Vector2 a, Vector2 b, Vector2 c)
    {
        return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    }

    /**********************************************************/
    //外部回调

    public void ShowPosition()
    {
        isShowPosition = !isShowPosition;
        for (int i = 0; i < points.Count; i++)
        {
            points[i].IsShowPosition = isShowPosition;
        }
    }

    /// <summary>
    /// 显示特征多边形
    /// </summary>
    public void ShowPolygon()
    {
        isShowPolygon = !isShowPolygon;
        setPolygon(isShowPolygon);
    }

    /// <summary>
    /// 显示多边形凸包
    /// </summary>
    public void ShowConvexHull()
    {
        isShowConvexHull = !isShowConvexHull;
        setConvexHull(isShowConvexHull);
    }

    public void UpDgree(bool isUp)
    {
        upDgree(isUp);
    }

    /**********************************************************/

    private bool isShowPosition = false;
    private bool isShowPolygon = false;
    private bool isShowConvexHull = false;

    private bool isDraged = false;
    private bool isInRange = false;
    private bool isInserted = false;

    string type, dgree, times, num, lamp;

    [NonSerialized]
    public List<Point> points;

    [NonSerialized]
    public List<Vector2> tmpPoints;

    public List<Vector2> convexHull;

    public List<Vector2> tmpConvex;

    private Vector3 m_mousePosition;

    private Point dragPoint;


    /**********************************************************/
    //委托

    public delegate void UpdateCurveDataCall();

    public delegate void GetCurveInfoCall(ref string type, ref string degree,
        ref string times, ref string num, ref string lamp);

    public delegate void SetPolygonCall(bool isShow);

    public delegate void SetConvexHullCall(bool isShow);

    public delegate void SetKnotPointCall(bool isShow);

    public delegate void UpDgreeCall(bool isUp);

    /// <summary>
    /// 更新曲线的数据，包括曲线顶点和一些附加属性
    /// </summary>
    public UpdateCurveDataCall updateCurveData { get; set; }

    /// <summary>
    /// 获取曲线各种属性的信息
    /// </summary>
    public GetCurveInfoCall getCurveInfo { get; set; }

    /// <summary>
    /// 设置是否显示特征多边形
    /// </summary>
    public SetPolygonCall setPolygon { get; set; }

    /// <summary>
    /// 设置是否显示多边形凸包
    /// </summary>
    public SetConvexHullCall setConvexHull { get; set; }

    /// <summary>
    /// 设置是否显示节点
    /// </summary>
    public SetKnotPointCall setKnotPoint { get; set; }

    /// <summary>
    /// 设置升/降阶
    /// </summary>
    public UpDgreeCall upDgree { get; set; }

    Comparison<Vector2> comp = ((X, Y) =>
    {
        return (X.x < Y.x)
               || (Mathf.Abs(X.x - Y.x) < 1e-6 && X.y < Y.y) ? -1 : 1;
    });

}
