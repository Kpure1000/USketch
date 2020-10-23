using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

public class BSplineDrawer : MonoBehaviour
{
    [DllImport("DeBoor_Cox")]
    static extern float BaseFunc_RE(int i, int k, float u, float[] knot);
    [DllImport("DeBoor_Cox")]
    static extern float BaseFunc(int i, int k, float u, float[] knot, float[] uArray, int[] tArray);

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

    private void Start()
    {
        knot = new List<float>();

        vertexs = new Vertex[pointManager.lampCount];

        tArray = new int[pow2(maxDegree) - 1];
        //初始化递归树
        tArray[0] = 0;
        for (int i = 1; i < tArray.Length; i++)
        {
            tArray[i] = i % 2 != 0 ? tArray[(i - 1) / 2] : tArray[(i - 2) / 2] + 1;
        }

        uArray = new float[pow2(maxDegree - 1)];

        knotPoints = new List<Point>();

        RestartDraw();
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
                UpdateKnotVector(pointManager.points.Count + degree);
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
                        N_i_k = deBoor_Cox(j, degree - 1, tMin + (i * dt)); //  C#实现的迭代基函数
                        //N_i_k = BaseFunc_RE(j, degree - 1, tMin + (i * dt), knot.ToArray()); //  C实现的递归基函数
                        //N_i_k = BaseFunc(j, degree - 1, tMin + (i * dt), knot.ToArray(), uArray, tArray); //  C实现的迭代基函数
                        
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
    /// deBoor-Cox 算法，利用递归树消除递归
    /// </summary>
    /// <param name="i">控制点index</param>
    /// <param name="de">曲线次数</param>
    /// <param name="u">插值</param>
    /// <returns></returns>
    private float deBoor_Cox(int i, int de, float u)
    {
        treeLineNum = pow2(de);
        rk = 0;
        ri = 0;
        for (int it = 0; it < treeLineNum; it += 1)
        {
            ri = tArray[indexer(de, it)];
            uArray[it] = (u >= knot[i + ri]
                && u < knot[i + ri + 1]) ? 1.0f : 0.0f;
        }
        rk++;
        while (rk <= de)
        {
            for (int it = 0; it < treeLineNum; it += 2)
            {
                ri = tArray[indexer(de - rk, it / 2)];
                div1 = knot[i + ri + rk]
                    - knot[i + ri];

                div2 = knot[i + ri + rk + 1]
                    - knot[i + ri + 1];

                U1 = (Mathf.Abs(div1) < 1e-3f) ? 1.0f
                    : (u - knot[i + ri]) / div1;

                U2 = (Mathf.Abs(div2) < 1e-3f) ? 1.0f
                    : (knot[i + ri + rk + 1] - u) / div2;

                uArray[it / 2] = U1 * uArray[it] + U2 * uArray[it + 1];

            }
            treeLineNum /= 2;
            rk++;
        }
        return uArray[0];
    }

    void outKnot(string att, int i, int k, float val)
    {
        Debug.Log(att + "[" + i + ", " + k + "]: " + val);
    }

    private int indexer(int k, int it)
    {
        return (int)Mathf.Pow(2, k) - 1 + it;
    }

    private int pow2(int pow)
    {
        return (int)Mathf.Pow(2, pow);
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
    private void UpdateKnotVector(int knotNum)
    {
        if (knot == null)
        {
            knot = new List<float>();
        }
        if (knot.Count > 0)
        {
            knot.Clear();
        }
        string knotStr = "";
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
            //knotStr += knot[i].ToString() + " ";
            // 0 1 2 3 4, 2,3
            // 0 0 1 2 2
        }
        Debug.Log(knotStr);
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
        UpdateKnotVector(pointManager.points.Count + degree);
        //由自己实现控制点的回调
        pointManager.updateCurveData = new PointManager.UpdateCurveDataCall(UpdateBSpline);
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

    private float div1, div2, U1, U2;

    private int rk, ri, treeLineNum;

    private bool isShowPolygon;

    private bool isShowConvexHull;

    private bool isShowKnot;

    private float tMin, tMax, dt, N_i_k;

    private Vertex[] vertexs;

    private int[] tArray;

    private float[] uArray;

    private Vector2 tmpPos;

}
