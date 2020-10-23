using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

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
        knot = new List<float>();

        vertexs = new Vertex[pointManager.lampCount];

        tArray = new TNode[pow2(maxDegree) - 1];
        //初始化递归树
        tArray[0].offset = 0;
        tArray[0].val = 0.0f;
        for (int i = 1; i < tArray.Length; i++)
        {
            tArray[i].offset = i % 2 != 0 ? tArray[(i - 1) / 2].offset : tArray[(i - 2) / 2].offset + 1;
            tArray[i].val = 0.0f;
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
                        N_i_k = deBoor_Cox(j, degree - 1, tMin + (i * dt));
                        //outKnot("递归结果: ", j, degree - 1, N_i_k);
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
    /// <param name="i"></param>
    /// <param name="de"></param>
    /// <param name="u"></param>
    /// <returns></returns>
    private float deBoor_Cox(int i, int de, float u)
    {
        treeLineNum = pow2(de);
        rk = 0;
        for (int it = 0; it < treeLineNum; it += 1)
        {
            //uArray[it] = (u >= knot[i + it / 2] && u < knot[i + 1 + it / 2]) ? 1.0f : 0.0f;
            //uArray[it + 1] = (u >= knot[i + 1 + it / 2] && u < knot[i + 2 + it / 2]) ? 1.0f : 0.0f;            

            uArray[it] = (u >= knot[i + tArray[indexer(de, it)].offset]
                && u < knot[i + tArray[indexer(de, it)].offset + 1]) ? 1.0f : 0.0f;

            //outKnot("r", i + tArray[indexer(de, it)].offset, rk, uArray[it]);

            //uArray[it + 1] = (u >= knot[i + tArray[indexer(de, it + 1)].offset]
            //    && u < knot[i + tArray[indexer(de, it + 1) + 1].offset + 1]) ? 1.0f : 0.0f;
        }
        rk++;
        while (rk <= de)
        {
            for (int it = 0; it < treeLineNum; it += 2)
            {
                //div1 = knot[i + it / 2 + rk] - knot[i + it / 2];
                //div2 = knot[i + it / 2 + rk + 1] - knot[i + it / 2 + 1];

                div1 = knot[tArray[i + indexer(de - rk, it / 2)].offset + rk]
                    - knot[tArray[i + indexer(de - rk, it / 2)].offset];

                div2 = knot[tArray[i + indexer(de - rk, it / 2)].offset + rk + 1]
                    - knot[tArray[i + indexer(de - rk, it/2)].offset + 1];


                //U1 = (Mathf.Abs(div1) < 1e-3f) ? 1.0f : (u - knot[i + it / 2]) / div1;
                //U2 = (Mathf.Abs(div2) < 1e-3f) ? 1.0f : (knot[i + it / 2 + rk + 1] - u) / div2;

                U1 = (Mathf.Abs(div1) < 1e-3f) ? 1.0f
                    : (u - knot[tArray[i + indexer(de - rk, it/2)].offset]) / div1;

                U2 = (Mathf.Abs(div2) < 1e-3f) ? 1.0f
                    : (knot[tArray[i + indexer(de - rk, it/2)].offset + rk + 1] - u) / div2;

                //uArray[it / 2] = U1 * uArray[it] + U2 * uArray[it + 1];

                uArray[it / 2] = U1 * uArray[it] + U2 * uArray[it + 1];

                //outKnot("", i + tArray[indexer(de - rk, it)].offset, de - rk, uArray[it]);
            }
            treeLineNum /= 2;
            rk++;
        }
        return uArray[0];
    }

    void outKnot(string att,int i,int k,float val)
    {
        Debug.Log(att + "[" + i + ", " + k + "]: " + val);
    }

    private int indexer(int k,int it)
    {
        return pow2(k) - 1 + it;
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

    private int rk, treeLineNum;

    private bool isShowPolygon;

    private bool isShowConvexHull;

    private bool isShowKnot;

    private float tMin, tMax, dt, N_i_k;

    private Vertex[] vertexs;

    private TNode[] tArray;

    private float[] uArray;

    public struct TNode
    {
        public int offset;
        public float val;
    }

    private Vector2 tmpPos;

}
