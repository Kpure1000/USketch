using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierDrawer : MonoBehaviour
{

    public PointManager pointManager;

    [Range(100,20000)]
    public int lampCount=1000;

    public void Start()
    {
        vertexs = new Vector2[lampCount];

        pointManager.BezierLine = new PointManager.UpdateLineData(UpdateBezier);
    }

    /// <summary>
    /// 更新点集
    /// </summary>
    public void UpdateBezier()
    {
        if(pointManager.IsUpdated)
        {
            dt = 0;
            for (int i = 0; i < lampCount; i++)
            {
                if (pointManager.points.Count >= 2)
                {
                    vertexs[i] = findTarget(dt);
                }
                else
                {
                    vertexs[i] = Vector2.zero;
                }
                dt += (float)(1.0f / lampCount);
            }
            pointManager.IsUpdated = false;
        }
    }

    /// <summary>
    /// 计算插值点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private Vector3 findTarget(float t)
    {
        for (int i = pointManager.tmpPoints.Count; i >= 1; i--)
        {
            for (int j = 0; j + 1 < i; j++)
            {
                pointManager.tmpPoints[j]=
                    (1 - t) * pointManager.tmpPoints[j]+
                    t * pointManager.tmpPoints[j + 1];
            }
        }
        if (pointManager.tmpPoints.Count > 0)
        {
            return pointManager.tmpPoints[0];
        }
        return Vector3.zero;
    }

    /// <summary>
    /// 绘制插值点集
    /// </summary>
    public void OnPostRender()
    {
        GL.LoadOrtho();
        GL.Color(Color.white);

        GL.Begin(GL.LINE_STRIP);
        for (int i = 0; i < lampCount; i++)
        {
            GL.Vertex((vertexs[i]));
        }
        GL.End();
    }

    private float dt;

    private Vector2[] vertexs;
}
