using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BSplineDrawer : MonoBehaviour
{
    public PointManager pointManager;

    [Tooltip("曲线阶数")]
    public int degree = 3;

    [Tooltip("最小阶数")]
    public int minDegree;
    [Tooltip("最大阶数")]
    public int maxDegree;

    /// <summary>
    /// 曲线次数
    /// </summary>
    [NonSerialized]
    public int times;

    /**********************************************************/

    //private void Awake()
    //{
    //    knot = new List<double>();
    //    vertexs = new Vertex[pointManager.lampCount];
    //    knotPoints = new List<Point>();
    //    RestartDraw();
    //}

    private void Start()
    {
        knot = new List<Double>();
        vertexs = new Vertex[pointManager.lampCount];
        knotPoints = new List<Point>();
        RestartDraw();
        isShowKnot = false;
    }

    private void Update()
    {
        if (isShowKnot && knot != null && knotPoints != null)
        {
            for (int i = 0; i < knot.Count; i++)
            {
                if (i < knotPoints.Count)
                    knotPoints[i].transform.position
                        = vertexs[(int)((pointManager.lampCount - 1) * knot[i])].pos;
                else
                {
                    Point newPoint = Instantiate(pointManager.pointOrg);
                    newPoint.transform.position
                        = vertexs[(int)((pointManager.lampCount - 1) * knot[i])].pos;
                    newPoint.transform.localScale = new Vector3(0.65f, 0.65f, 1.0f);
                    newPoint.setColor(Color.green);
                    knotPoints.Add(newPoint);
                }
                knotPoints[i].gameObject.SetActive(true);
            }
        }
        else
        {
            for (int i = 0; i < knotPoints.Count; i++)
            {
                knotPoints[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 求基函数值(递归,不过最多递归三层，无大碍)
    /// </summary>
    /// <param name="i">控制点索引</param>
    /// <param name="k">次数（阶数-1）</param>
    /// <param name="u">节点索引</param>
    /// <returns>基函数的值</returns>
    private double deBoor_Cox_RE(int i, int k, double u)
    {
        if (k == 0)
        {
            return (u >= knot[i] && u < knot[i + 1]) ? 1.0 : 0.0;
        }

        div1 = knot[i + k] - knot[i];
        div2 = knot[i + k + 1] - knot[i + 1];

        U1 = (Math.Abs(div1) < 1e-7) ? 1.0 : (u - knot[i]) / div1;
        U2 = (Math.Abs(div2) < 1e-7) ? 1.0 : (knot[i + k + 1] - u) / div2;

        return U1 * deBoor_Cox_RE(i, k - 1, u) + U2 * deBoor_Cox_RE(i + 1, k - 1, u);
    }

    /// <summary>
    /// 更新点集
    /// </summary>
    private void UpdateBSpline()
    {
        if (pointManager.IsUpdated)
        {
            if (knot.Count != pointManager.points.Count + degree)
            {
                SetKnotVector(pointManager.points.Count + degree);
            }

            //tMin = knot[degree];
            //tMax = knot[knot.Count - degree];

            tMin = knot[0];
            tMin = knot[knot.Count - 1];

            dt = (tMax - tMin) / (pointManager.lampCount - 1);

            for (int i = 0; i < pointManager.lampCount; i++)
            {
                if (pointManager.points.Count >= 2)
                {
                    tmpPos = Vector2.zero;
                    for (int j = 0; j < pointManager.points.Count; j++)
                    {
                        N_i_k = deBoor_Cox_RE(j, degree - 1, tMin + (i * dt));
                        tmpPos.x += (float)N_i_k * pointManager.points[j].transform.position.x;
                        tmpPos.y += (float)N_i_k * pointManager.points[j].transform.position.y;
                    }
                    vertexs[i].pos = tmpPos;
                }
                else
                {
                    vertexs[i].pos = Vector2.zero;
                }
                vertexs[i].vertexType = VertexType.Normal;
                vertexs[i].color = Color.blue;
            }
        }
    }

    private void getBSplineInfo(ref string type, ref string degree,
        ref string times, ref string num, ref string lamp)
    {
        type = "曲线类型: B样条曲线";
        degree = "曲线阶数: " + this.degree;
        times = "曲线次数: " + (this.degree - 1);
        num = "控制点个数: " + pointManager.points.Count;
        lamp = "插值数量: " + pointManager.lampCount;
    }

    /// <summary>
    /// 升阶
    /// </summary>
    private void UpDgree()
    {
        degree = Mathf.Min(maxDegree, degree + 1);
    }

    /// <summary>
    /// 降阶
    /// </summary>
    private void DownDgree()
    {
        degree = Mathf.Max(minDegree, degree - 1);
    }

    /// <summary>
    /// 生成均匀递增的节点向量
    /// </summary>
    /// <param name="knotNum"></param>
    private void SetKnotVector(int knotNum)
    {
        if (knot == null)
        {
            knot = new List<double>();
        }
        if (knot.Count > 0)
        {
            knot.Clear();
        }
        for (int i = 0; i < knotNum; i++)
        {
            //if (i < degree)
            //{
            //    knot.Add(0.0);
            //}
            //else if (i > knotNum - degree)
            //{
            //    knot.Add((double)knot[i - 1]);
            //}
            //else
            //{
            //    knot.Add((double)knot[i - 1] + 1.0);
            //}
            knot.Add((double)i);
        }
    }

    private void OnPostRender()
    {
        GL.Begin(GL.LINE_STRIP);
        for (int i = 0; i < vertexs.Length; i++)
        {
            GL.Color(vertexs[i].color);
            GL.Vertex(vertexs[i].pos);
        }
        GL.End();
        if (isShowPolygon)
        {
            GL.Begin(GL.LINE_STRIP);
            for (int i = 0; i < pointManager.points.Count; i++)
            {
                GL.Color(new Color(0.9f, 0.0f, 0.0f, 0.5f));
                GL.Vertex(pointManager.points[i].transform.position);
            }
            //if (pointManager.points.Count > 0)
            //{
            //    GL.Color(Color.red);
            //    GL.Vertex(pointManager.points[0].transform.position);
            //}
            GL.End();
        }
        if(isShowConvexHull)
        {
            GL.Begin(GL.LINE_STRIP);
            for (int i = 0; i < pointManager.convexHull.Count; i++)
            {
                GL.Color(new Color(0.0f, 0.9f, 0.0f, 0.6f));
                GL.Vertex(pointManager.convexHull[i]);
            }
            if (pointManager.convexHull.Count > 0)
            {
                GL.Color(new Color(0.0f, 0.9f, 0.0f, 0.6f));
                GL.Vertex(pointManager.convexHull[0]);
            }
            GL.End();
        }
    }

    /**********************************************************/

    /// <summary>
    /// 重置绘制
    /// </summary>
    public void RestartDraw()
    {
        SetKnotVector(pointManager.points.Count + degree);
        Debug.Log("B样条创建回调");
        //由自己实现控制点的回调
        pointManager.updateCurveData = new PointManager.UpdateCurveDataCall(UpdateBSpline);
        pointManager.getCurveInfo = new PointManager.GetCurveInfoCall(getBSplineInfo);
        pointManager.setPolygon = (isShow) => { isShowPolygon = isShow; };
        pointManager.setConvexHull = (isShow) => { isShowConvexHull = isShow; };
        pointManager.setKnotPoint = (isShow) => { isShowKnot = isShow; };
        pointManager.upDgree = (isUp) => { if (isUp) UpDgree(); else DownDgree(); };
        pointManager.RestartPaint();
    }

    /// <summary>
    /// 关闭当前配置，重置控制点
    /// </summary>
    public void CloseDraw()
    {
        for (int i = 0; i < vertexs.Length; i++)
        {
            vertexs[i].pos = Vector2.zero;
        }
        pointManager.ResetPoint();
    }

    /**********************************************************/

    /// <summary>
    /// 节点向量
    /// </summary>
    private List<double> knot;

    private List<Point> knotPoints;

    private double div1 = 0.0, div2 = 0.0, U1 = 0.0, U2 = 0.0;

    private bool isShowPolygon;

    private bool isShowConvexHull;

    private bool isShowKnot;

    private double tMin, tMax, dt, N_i_k;

    private Vertex[] vertexs;

    private Vector2 tmpPos;
}
