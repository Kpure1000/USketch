using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PointManager : MonoBehaviour
{

    [Tooltip("控制点预制体")]
    public Point controlPointOrg;
    [Tooltip("节点预制体")]
    public Point knotPointOrg;

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

    public bool IsAddRect { get; private set; } = false;

    /**********************************************************/

    private void Awake()
    {
        points = new List<Point>();
        tmpPoints = new List<Vector2>();
        convexHull = new List<Vector2>();
        tmpConvex = new List<Vector2>();
        dragPointsIndex = new List<PointIndex>();
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
        IsAddRect = false;
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
        setPolygon = null;
        setKnotPoint = null;
        setConvexHull = null;
    }

    /// <summary>
    /// 重新开始绘制控制点
    /// </summary>
    public void RestartCallBack()
    {
        ShowPosition(isShowPosition);
        setPolygon(isShowPolygon);
        setConvexHull(isShowConvexHull);
        setKnotPoint?.Invoke(isShowKnotPoint);
    }

    /// <summary>
    /// 更新控制点(添加和拖动)
    /// </summary>
    /// <param name="isLeftPressed">是否按下鼠标左键</param>
    /// <param name="isRightPressd">是否按下鼠标右键</param>
    void UpdatePoint(bool isLeftPressed, bool isRightPressd)
    {
        if(Input.GetKeyDown(KeyCode.Delete))
        {
            RemovePoint();
            return;
        }
        m_mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (isRightPressd)
        {
            if (!IsAddRect)
            {
                dragPointsIndex.Clear();
                IsAddRect = true;
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
        else if (IsAddRect)
        {
            IsAddRect = false;
            dragRect.size = m_mousePosition;
            //框选
            for (int i = 0; i < points.Count; i++)
            {
                if (pointContains(dragRect, points[i].transform.position))
                {
                    if (!indexContains(dragPointsIndex, i))
                    {
                        dragPointsIndex.Add(new PointIndex(i, points[i].transform.position));
                    }
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

                    isInRect = indexContains(dragPointsIndex, i);

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
                    SetPointPosition(points[dragPointsIndex[i].index], dragPointsIndex[i].originPos +
                        (Vector2)(m_mousePosition - m_mousePosition_Start));
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
            //更新index点的初始位置
            for (int i = 0; i < dragPointsIndex.Count; i++)
            {
                dragPointsIndex[i].originPos = (Vector2)points[dragPointsIndex[i].index].transform.position;
            }
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
        Point newPoint = Instantiate(controlPointOrg);
        newPoint.pointType = type;
        newPoint.pName = 'P' + points.Count.ToString();
        SetPointPosition(newPoint, m_mousePosition);
        points.Add(newPoint);
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
        IsUpdated = true;
        ShowPosition(isShowPosition);
    }

    public void InsertPoint(Vector2 pos, PointType type)
    {
        Point newPoint = Instantiate(controlPointOrg);
        newPoint.pointType = type;
        newPoint.pName = 'P' + points.Count.ToString();
        SetPointPosition(newPoint, pos);
        points.Add(newPoint);
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
        IsUpdated = true;
        ShowPosition(isShowPosition);
    }

    /// <summary>
    /// 删除控制点
    /// </summary>
    private void RemovePoint()
    {
        //  降序
        dragPointsIndex.Sort((x, y) => { return -x.index.CompareTo(y.index); });
        for (int i = 0; i < dragPointsIndex.Count; i++)
        {
            RemoveAt(dragPointsIndex[i].index);
        }
        dragPointsIndex.Clear();
        IsUpdated = true;
    }

    public void RemoveAt(int index)
    {
        Destroy(points[index].gameObject);
        points.RemoveAt(index);
        tmpPoints.RemoveAt(index);
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
        IsUpdated = true;
        ShowPosition(isShowPosition);
    }

    public void ForbidenInsert(bool isForbiden)
    {
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
            if (tmpConvex.Count <= 0 || !tmpConvex.Contains((Vector2)points[i].transform.position)) 
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

    private bool indexContains(List<PointIndex> pointIndex, int index)
    {
        for (int i = 0; i < pointIndex.Count; i++)
        {
            if (index == pointIndex[i].index) return true;
        }
        return false;
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

    public void ShowKnotPoint()
    {
        isShowKnotPoint = !isShowKnotPoint;
        setKnotPoint?.Invoke(isShowKnotPoint);
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
    private bool isShowKnotPoint = false;

    private bool isDraged = false;
    private bool isInRange = false;
    private bool isInserted = false;
    private bool isInRect = false;

    private bool isForbidenInsert = false;

    private Vector3 m_mousePosition;
    private Vector3 m_mousePosition_Start;

    private Point dragPoint;

    private List<PointIndex> dragPointsIndex;

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

    private Comparison<Vector2> comp = ((X, Y) =>
    {
        return (X.x < Y.x) || (X.x == Y.x && X.y < Y.y) ? -1 : 1;
    });

}


public class PointIndex
{
    public PointIndex(int Index, Vector2 pos)
    {
        index = Index;
        originPos = pos;
    }
    public int index { get; set; }
    public Vector2 originPos { get; set; }
}