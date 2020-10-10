using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class BezierDrawer : MonoBehaviour
{
    [Tooltip("控制点管理器")]
    public PointManager pointManager;

    /**********************************************************/

    //private void Awake()
    //{
    //    vertexs = new Vertex[pointManager.lampCount];
    //    RestartDraw();
    //}

    public void Start()
    {
        vertexs = new Vertex[pointManager.lampCount];
        RestartDraw();
    }


    //private void Update()
    //{

    //}

    /// <summary>
    /// 更新点集
    /// </summary>
    private void UpdateBezier()
    {
        if (pointManager.IsUpdated)
        {
            dt = 0;
            for (int i = 0; i < pointManager.lampCount; i++)
            {
                if (pointManager.points.Count >= 2)
                {
                    vertexs[i].pos = findTarget(dt);
                }
                else
                {
                    vertexs[i].pos = Vector2.zero;
                }
                vertexs[i].color = Color.blue;
                dt += (float)(1.0f / pointManager.lampCount);
            }
            pointManager.IsUpdated = false;
        }

    }

    /// <summary>
    /// 计算插值点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private Vector2 findTarget(float t)
    {
        for (int i = pointManager.tmpPoints.Count; i >= 1; i--)
        {
            for (int j = 0; j + 1 < i; j++)
            {
                pointManager.tmpPoints[j] =
                    (1 - t) * pointManager.tmpPoints[j] +
                    t * pointManager.tmpPoints[j + 1];
            }
        }
        if (pointManager.tmpPoints.Count > 0)
        {
            return pointManager.tmpPoints[0];
        }
        return Vector2.zero;
    }

    /// <summary>
    /// 升阶
    /// </summary>
    private void UpDgree()
    {

    }

    /// <summary>
    /// 降阶
    /// </summary>
    private void DownDgree()
    {

    }

    private void getBezierInfo(ref string type, ref string dgree,
        ref string times, ref string num, ref string lamp)
    {
        type = "曲线类型: 贝塞尔曲线";
        dgree = "曲线阶数: " + Mathf.Max(0, (pointManager.points.Count - 1));
        times = "曲线次数: " + Mathf.Max(0, (pointManager.points.Count - 1));
        num = "控制点个数: " + pointManager.points.Count;
        lamp = "插值数量: " + pointManager.lampCount;
    }

    /// <summary>
    /// GL绘制曲线和多边形
    /// </summary>
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
    //回调

    /// <summary>
    /// 重置绘制
    /// </summary>
    public void RestartDraw()
    {
        pointManager.ForbidenInsert(false);

        //由自己实现控制点的回调
        pointManager.updateCurveData = new PointManager.UpdateCurveDataCall(UpdateBezier);
        pointManager.getCurveInfo = new PointManager.GetCurveInfoCall(getBezierInfo);
        pointManager.setPolygon = (isShow) => { isShowPolygon = isShow; };
        pointManager.setConvexHull = (isShow) => { isShowConvexHull = isShow; };
        pointManager.upDgree = (isUp) => { if (isUp) UpDgree(); else DownDgree(); };
        pointManager.RestartCallBack();
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

    private bool isShowPolygon;
    private bool isShowConvexHull;

    private float dt;

    private Vertex[] vertexs;
}
