using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class BSplineDrawer : MonoBehaviour
{
    //[DllImport("DeBoor_Cox.dll", CallingConvention = CallingConvention.Cdecl)]
    //extern static float BaseFunc(int i, int k, float u, float[] knotArray);

    //[DllImport("libMyLibTest.dylib", CallingConvention = CallingConvention.Cdecl)]
    //extern static float BaseFunction(int i, int k, float u, float[] knotArray);

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
        knot = new List<float>();

        vertexs = new Vertex[pointManager.lampCount];

        pTmp = new Vector2[pointManager.maxControlPointNumber];

        knotPoints = new List<Point>();

        uArray = new List<float>();

        RestartDraw();
    }



    /// <summary>
    /// 求基函数值(递归,不过最多递归三层，无大碍)
    /// </summary>
    /// <param name="i">控制点索引</param>
    /// <param name="k">次数（阶数-1）</param>
    /// <param name="u">节点索引</param>
    /// <returns>基函数的值</returns>
    private float deBoor_Cox_RE(int i, int k, float u)
    {
        //if (k == 0)
        //{
        //    //Debug.Log(string.Format("u({0}) >= [{1}]({2}) && u({3}) < [{4}]({5})",
        //    //    u, i, knot[i], u, i + 1, knot[i + 1]));
        //    //Debug.Log(u >= knot[i] && u < knot[i + 1]);
        //    return (u >= knot[i] && u < knot[i + 1]) ? 1.0f : 0.0f;
        //}

        //div1 = knot[i + k] - knot[i];
        //div2 = knot[i + k + 1] - knot[i + 1];

        //U1 = (Mathf.Abs(div1) < 1.0e-6f) ? 1.0f : (u - knot[i]) / div1;
        //U2 = (Mathf.Abs(div2) < 1.0e-6f) ? 1.0f : (knot[i + k + 1] - u) / div2;

        //return U1 * deBoor_Cox_RE(i, k - 1, u) + U2 * deBoor_Cox_RE(i + 1, k - 1, u);
        int k_2 = k * k;

        int rk = 0;

        for (int it = 0; it < k_2; it++)
        {
            if(it>=uArray.Count)
            {
                uArray.Add(0);
            }
        }

        for (int it = 0; it < k_2; it += 2)
        {
            if (it < uArray.Count)
            {
                if (it < k_2 / 2)
                {
                    uArray[it] = (u >= knot[i + it / 2] && u < knot[i + 1 + it / 2]) ? 1.0f : 0.0f;
                    uArray[it + 1] = (u >= knot[i + 1 + it / 2] && u < knot[i + 2 + it / 2]) ? 1.0f : 0.0f;
                }
                else
                {
                    uArray[it] = (u >= knot[i + it / 2 - 1] && u < knot[i + it / 2]) ? 1.0f : 0.0f;
                    uArray[it + 1] = (u >= knot[i + it / 2] && u < knot[i + 1 + it / 2]) ? 1.0f : 0.0f;
                }
            }
        }
        rk++;
        while (rk <= k) 
        {
            for (int it = 0; it < k_2; it += 2)
            {
                div1 = knot[i + it / 2 + rk] - knot[i + it / 2];
                div2 = knot[i + it / 2 + rk + 1] - knot[i + it / 2 + 1];

                U1 = (Mathf.Abs(div1) < 1.0e-6f) ? 1.0f : (u - knot[i + it / 2]) / div1;
                U2 = (Mathf.Abs(div2) < 1.0e-6f) ? 1.0f : (knot[i + it / 2 + rk + 1] - u) / div2;

                uArray[it / 2] = U1 * uArray[it] + U2 * uArray[it + 1];

            }
            k_2 /= 2;
            rk++;
        }
        return uArray[0];
    }

    /// <summary>
    /// 更新点集
    /// </summary>
    private void UpdateBSpline_RE()
    {
        if (pointManager.IsUpdated)
        {
            if (knot.Count != pointManager.points.Count + degree)
            {
                SetKnotVector(pointManager.points.Count + degree);
            }

            tMin = knot[degree - 1];
            tMax = knot[knot.Count - degree];

            //tMin = knot[0];
            //tMax = knot[knot.Count - 1];

            dt = (tMax - tMin) / (pointManager.lampCount - 1);

            for (int i = 0; i < pointManager.lampCount; i++)
            {
                if (pointManager.points.Count >= 2)
                {
                    tmpPos = Vector2.zero;
                    for (int j = 0; j < pointManager.points.Count; j++)
                    {
                        //N_i_k = BaseFunc(j, degree - 1, tMin + (i * dt), knot.ToArray());
                        //N_i_k = BaseFunction(j, degree - 1, tMin + (i * dt), knot.ToArray());
                        N_i_k = deBoor_Cox_RE(j, degree - 1, tMin + (i * dt));
                        Debug.Log("递归: i:" + i + ", j: " + j + ", n: " + N_i_k);
                        tmpPos += N_i_k * (Vector2)pointManager.points[j].transform.position;
                    }
                    vertexs[i].pos = tmpPos;
                }
                else
                {
                    vertexs[i].pos = Vector2.zero;
                }
                vertexs[i].color = Color.blue;
            }
            ShowKnotPoint();
            pointManager.IsUpdated = false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="de">次数</param>
    /// <param name="i">节点索引</param>
    /// <param name="u">节点插值</param>
    /// <returns></returns>
    [Obsolete]
    private Vector2 deBoor_Cox(int de, int i, float u)
    {
        int k, j;
        double t1, t2;
        for (j = i - de + 1; j <= i + 1; j++)
        {
            pTmp[j] = pointManager.points[j].transform.position;
        }
        for (k = 1; k <= de; k++)
        {
            for (j = i + 1; j >= i - de + k + 1; j--)
            {
                t1 = (float)(knot[j + de - k] - u) / (knot[j + de - k] - knot[j - 1]);
                t2 = 1.0f - t1;
                pTmp[j] = (float)t1 * pTmp[j - 1] + (float)t2 * pTmp[j];
            }
        }
        return pTmp[i + 1];
    }

    [Obsolete]
    private void UpdateBSpline()
    {
        if (pointManager.IsUpdated)
        {
            if (knot.Count != pointManager.points.Count + degree)
            {
                SetKnotVector(pointManager.points.Count + degree);
            }
            int i, ii;
            float u;
            int subLamp = (pointManager.lampCount) / (knot.Count - 1);
            int verIndex = 0;
            for (i = degree; i < pointManager.points.Count - 1; i++)
            //for (i = 0; i + 1 < knot.Count; i++)
            {
                if (knot[i + 1] > knot[i])
                {
                    Debug.Log("绘制");
                    for (ii = 0; ii < subLamp; ii++)
                    {
                        u = (float)(knot[i] + ii * (knot[i + 1] - knot[i]) / subLamp);
                        vertexs[verIndex].pos = deBoor_Cox(degree - 1, i, u);
                        vertexs[verIndex].color = Color.blue;
                        verIndex++;
                    }
                }
            }
            Debug.Log("应有顶点: " + pointManager.lampCount +
                "个， 实际顶点: " + verIndex + "个");
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
            knot = new List<float>();
        }
        if (knot.Count > 0)
        {
            knot.Clear();
        }
        for (int i = 0; i < knotNum; i++)
        {
            knot.Add((float)i / (knotNum - 1));
            //if (i < degree)
            //{
            //    knot.Add(0);
            //}
            //else if (i <= knotNum - degree)
            //{
            //    knot.Add(knot[i - 1] + 1);
            //}
            //else
            //{
            //    knot.Add(knot[i - 1]);
            //}
            // 0 1 2 3 4, 2,3
            // 0 0 1 2 2
        }
    }

    public void ShowKnotPoint()
    {
        //Debug.Log("显示节点: " + isShowKnot);
        if (pointManager.points.Count <= 1) return;
        if (isShowKnot)
        {
            float sum = knot[knot.Count - 1];
            for (int i = 0; i < knot.Count; i++)
            {
                if (i < knotPoints.Count)
                {
                    knotPoints[i].transform.position
                        = vertexs[(int)(knot[i] / sum * (vertexs.Length - 1))].pos;
                }
                else
                {
                    Point newPoint = Instantiate(pointManager.knotPointOrg);
                    newPoint.transform.position
                        = vertexs[(int)(knot[i] / sum * (vertexs.Length - 1))].pos;
                    //newPoint.transform.localScale = new Vector3(0.65f, 0.65f, 1.0f);
                    knotPoints.Add(newPoint);
                }
                knotPoints[i].gameObject.SetActive(true);
            }
            while (knot.Count < knotPoints.Count)
            {
                Destroy(knotPoints[knotPoints.Count - 1].gameObject);
                knotPoints.RemoveAt(knotPoints.Count - 1);
            }
        }
        else
        {
            while (knot.Count < knotPoints.Count)
            {
                Destroy(knotPoints[knotPoints.Count - 1].gameObject);
                knotPoints.RemoveAt(knotPoints.Count - 1);
            }
            for (int i = 0; i < knotPoints.Count; i++)
            {
                knotPoints[i].gameObject.SetActive(false);
            }
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
        if (isShowConvexHull)
        {
            GL.Begin(GL.LINE_STRIP);
            for (int i = 0; i < pointManager.convexHull.Count; i++)
            {
                GL.Color(new Color(0.0f, 0.65f, 0.0f, 1.0f));
                GL.Vertex(pointManager.convexHull[i]);
            }
            if (pointManager.convexHull.Count > 0)
            {
                GL.Color(new Color(0.0f, 0.65f, 0.0f, 1.0f));
                GL.Vertex(pointManager.convexHull[0]);
            }
            GL.End();
        }
        if (pointManager.IsAddRect)
        {
            BSplineDrawer.DrawVirtualLine(pointManager.dragRect.position,
                new Vector2(pointManager.dragRect.width, pointManager.dragRect.y), Color.red, 0.1f);
            BSplineDrawer.DrawVirtualLine(pointManager.dragRect.position,
                new Vector2(pointManager.dragRect.x, pointManager.dragRect.height), Color.red, 0.1f);
            BSplineDrawer.DrawVirtualLine(pointManager.dragRect.size,
                new Vector2(pointManager.dragRect.width, pointManager.dragRect.y), Color.red, 0.1f);
            BSplineDrawer.DrawVirtualLine(pointManager.dragRect.size,
                new Vector2(pointManager.dragRect.x, pointManager.dragRect.height), Color.red, 0.1f);

        }
    }

    public static void DrawRealLine(Vector3 start, Vector3 end, Color color)
    {
        GL.Color(color);
        GL.Vertex(start);
        GL.Color(color);
        GL.Vertex(end);
    }

    /// <summary>
    /// 画虚线
    /// </summary>
    /// <param name="start">起始点</param>
    /// <param name="end">结束点</param>
    /// <param name="color">颜色</param>
    /// <param name="dis">间隔</param>
    public static void DrawVirtualLine(Vector3 start, Vector3 end, Color color, float dis)
    {
        GL.Begin(GL.LINES);
        float Dis = Vector2.Distance(start, end);
        GL.Color(color);
        GL.Vertex(start);
        GL.Color(color);
        GL.Vertex(start);
        float seg = Dis / dis;
        for (float i = 0; i < seg; i += 1.0f)
        {
            GL.Color(color);
            GL.Vertex(start + i * (end - start) / seg);
        }
        GL.Color(color);
        GL.Vertex(end);
        GL.End();
    }

    /**********************************************************/

    /// <summary>
    /// 重置绘制
    /// </summary>
    public void RestartDraw()
    {
        SetKnotVector(pointManager.points.Count + degree);
        //由自己实现控制点的回调
        pointManager.updateCurveData = new PointManager.UpdateCurveDataCall(UpdateBSpline_RE);
        pointManager.getCurveInfo = new PointManager.GetCurveInfoCall(getBSplineInfo);
        pointManager.setPolygon = (isShow) => { isShowPolygon = isShow; };
        pointManager.setConvexHull = (isShow) => { isShowConvexHull = isShow; };
        pointManager.setKnotPoint = (isShow) => { isShowKnot = isShow; ShowKnotPoint(); };
        pointManager.upDgree = (isUp) => { if (isUp) UpDgree(); else DownDgree(); };
        pointManager.RestartCallBack();
    }

    /// <summary>
    /// 关闭当前配置，重置控制点
    /// </summary>
    public void CloseDraw()
    {
        if (vertexs != null)
        {
            for (int i = 0; i < vertexs.Length; i++)
            {
                vertexs[i].pos = Vector2.zero;
            }
        }
        pointManager.ResetPoint();
        //SetKnotVector(degree);
        //isShowKnot = false;
        //ShowKnotPoint();
        if (knotPoints != null)
        {
            for (int i = 0; i < knotPoints.Count; i++)
            {
                Destroy(knotPoints[i].gameObject);
            }
            knotPoints.Clear();
        }
    }

    /**********************************************************/

    /// <summary>
    /// 节点向量
    /// </summary>
    private List<float> knot;

    private List<Point> knotPoints;

    private List<float> uArray;

    private float div1, div2, U1, U2;

    private bool isShowPolygon;

    private bool isShowConvexHull;

    private bool isShowKnot;

    private float tMin, tMax, dt, N_i_k;

    private Vertex[] vertexs;

    private Vector2[] pTmp;

    private Vector2 tmpPos;

}
