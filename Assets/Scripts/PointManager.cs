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
    [Range(20, 10000)]
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
        dragPointsIndex = new List<int>();
        dragRect = Rect.zero;
    }

    private void Update()
    {
        //更新控制点
        UpdatePoint(Input.GetMouseButton((int)MouseButton.LeftMouse),
            Input.GetMouseButton((int)MouseButton.RightMouse));
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
        dragPointsIndex.Clear();
        dragRect = Rect.zero;

        IsUpdated = false;
        isDraged = false;
        isInRange = false;
        isInserted = false;
        isAddRect = false;
        isInRect = false;

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
    public void RestartCallBack()
    {
        ShowPosition(isShowPosition);
        setPolygon(isShowPolygon);
        setConvexHull(isShowConvexHull);
    }

    /// <summary>
    /// 更新控制点(添加和拖动)
    /// </summary>
    /// <param name="isLeftPressed">是否按下鼠标左键</param>
    /// <param name="isRightPressd">是否按下鼠标右键</param>
    void UpdatePoint(bool isLeftPressed, bool isRightPressd)
    {
        m_mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (isRightPressd)
        {
            if (!isAddRect)
            {
                dragPointsIndex.Clear();
                isAddRect = true;
                dragRect.position = m_mousePosition;
            }
            dragRect.size = m_mousePosition;
            for (int i = 0; i < points.Count; i++)
            {
                if (pointContains(dragRect, points[i].transform.position))
                {
                    points[i].setHighlight(HighLightType.GROUP);
                }
                else
                {
                    points[i].setHighlight(HighLightType.NONE);
                }
            }
        }
        else if (isAddRect)
        {
            isAddRect = false;
            dragRect.size = m_mousePosition;
            for (int i = 0; i < points.Count; i++)
            {
                if (pointContains(dragRect, points[i].transform.position))
                {
                    if (!dragPointsIndex.Contains(i))
                        dragPointsIndex.Add(i);
                }
            }
        }
        if (!isDraged)
        {
            for (int i = 0; i < points.Count; i++)
            {
                //是否在某点的检测范围内
                isInRange = points[i].IsContained;

                //设置高光,仅当不在选矿时有效
                if (points[i].highLightType != HighLightType.GROUP)
                {
                    if (isInRange)
                    {
                        points[i].setHighlight(HighLightType.SINGLW);
                    }
                    else
                    {
                        points[i].setHighlight(HighLightType.NONE);
                    }
                }
                #region 开始拖动
                if (isInRange && isLeftPressed && !isDraged)
                {
                    //Debug.Log("开始拖动");
                    isDraged = true;
                    dragPoint = points[i];
                    m_mousePosition_Start = dragPoint.transform.position;

                    isInRect = dragPointsIndex.Contains(i);

                    //插入了
                    isInserted = true;
                }
                #endregion
            }
        }
        else
        {
            #region 拖动时更新点位置

            if (isInRect)
            {
                //一起拖动
                for (int i = 0; i < dragPointsIndex.Count; i++)
                {
                    //TODO 这里有问题，不能直接赋值鼠标
                    SetPointPosition(points[dragPointsIndex[i]],
                        m_mousePosition_Start + );
                }
            }
            else
            {
                //只拖自己
                SetPointPosition(dragPoint, m_mousePosition);
            }

            //更新了控制点
            IsUpdated = true;
            #endregion
        }
        if (isLeftPressed)
        {
            #region INSERT point
            if (!isForbidenInsert && !isInserted
                && points.Count < maxControlPointNumber &&
                 !operatorUIManager.isEnter)
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
            #endregion
        }
        else
        {
            //Reset 状态

            isDraged = false;
            isInserted = false;
            dragPoint = null;
        }

        #region 复制到缓冲区内
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
        #endregion

        #region 求凸包
        if (isShowConvexHull)
        {
            updateConvexHull();
        }
        #endregion
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
        //Debug.Log("添加了一个控制点,控制点个数: " + points.Count);
        IsUpdated = true;
        ShowPosition(isShowPosition);
    }

    public void InsertPoint(Vector2 pos, PointType type)
    {
        Point newPoint = Instantiate(pointOrg);
        newPoint.pointType = type;
        newPoint.pName = 'P' + points.Count.ToString();
        SetPointPosition(newPoint, pos);
        points.Add(newPoint);
        //Debug.Log("添加了一个控制点,控制点个数: " + points.Count);
        IsUpdated = true;
        //ShowPosition(isShowPosition);
    }

    public void ForbidenInsert(bool isForbiden)
    {
        Debug.LogWarning("禁止插入控制点");
        isForbidenInsert = isForbiden;
    }

    public bool IsForbiden { get { return isForbidenInsert; } }

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
        tmpConvex.Clear();
        for (int i = 0; i < points.Count; i++)
        {
            if (tmpConvex.Count <= 0 || tmpConvex[tmpConvex.Count - 1] != (Vector2)points[i].transform.position)
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

    private bool pointContains(Rect range, Vector2 target)
    {
        judgeRect.x = Mathf.Min(range.x, range.width);
        judgeRect.y = Mathf.Min(range.y, range.height);
        judgeRect.width = Mathf.Abs(range.x - range.width);
        judgeRect.height = Mathf.Abs(range.y - range.height);
        return judgeRect.Contains(target);
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
        IsUpdated = true;
        upDgree(isUp);
    }

    /**********************************************************/

    private bool isShowPosition = false;
    private bool isShowPolygon = false;
    private bool isShowConvexHull = false;

    private bool isDraged = false;
    private bool isInRange = false;
    private bool isInserted = false;
    private bool isAddRect = false;
    private bool isInRect = false;

    private bool isForbidenInsert = false;

    private Vector3 m_mousePosition;
    private Vector3 m_mousePosition_Start;

    private Point dragPoint;

    private List<int> dragPointsIndex;

    string type, dgree, times, num, lamp;

    /**********************************************************/

    [NonSerialized]
    public List<Point> points;

    [NonSerialized]
    public List<Vector2> tmpPoints;

    public List<Vector2> convexHull;

    public List<Vector2> tmpConvex;

    public Rect dragRect;

    public Rect judgeRect;

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
        //if (X.x < Y.x) return -1;
        //if (X.x == Y.x) { if (X.y < Y.y) return -1; }
        //return 0;

        return (X.x < Y.x)
               || (X.x == Y.x && X.y < Y.y) ? -1 : 1;
    });

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(dragRect.position, new Vector2(dragRect.width, dragRect.y));
        Gizmos.DrawLine(dragRect.position, new Vector2(dragRect.x, dragRect.height));
        Gizmos.DrawLine(dragRect.size, new Vector2(dragRect.width, dragRect.y));
        Gizmos.DrawLine(dragRect.size, new Vector2(dragRect.x, dragRect.height));
    }

}
