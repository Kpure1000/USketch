using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BezierDrawer : MonoBehaviour
{

    public PointManager pointManager;

    [Range(100, 20000)]
    public int lampCount = 1000;

    public void Start()
    {
        controlVertexs = new List<Vector2>();

        vertexs = new Vector2[lampCount];

        pointManager.BezierLine = new PointManager.UpdateLineData(UpdateBezier);
    }

    private void Update()
    {
        
    }

    /// <summary>
    /// 更新点集
    /// </summary>
    public void UpdateBezier()
    {
        if (pointManager.IsUpdated)
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
            for (int i = 0; i < pointManager.points.Count; i++)
            {
                if (i >= controlVertexs.Count)
                {
                    controlVertexs.Add(pointManager.points[i].transform.position);
                }
                else
                {
                    controlVertexs[i] = pointManager.points[i].transform.position;
                }
            }
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
                pointManager.tmpPoints[j] =
                    (1 - t) * pointManager.tmpPoints[j] +
                    t * pointManager.tmpPoints[j + 1];
            }
        }
        if (pointManager.tmpPoints.Count > 0)
        {
            return pointManager.tmpPoints[0];
        }
        return Vector3.zero;
    }

    private void OnPostRender()
    {
        //GL.LoadOrtho();

        GL.Begin(GL.LINE_STRIP);
        for (int i = 0; i < lampCount; i++)
        {
            GL.Color(Color.black);
            GL.Vertex((vertexs[i]));
        }
        GL.End();
        if (isShowPolygon)
        {
            GL.Begin(GL.LINE_STRIP);
            for (int i = 0; i < controlVertexs.Count; i++)
            {
                GL.Color(Color.red);
                GL.Vertex(controlVertexs[i]);
            }
            GL.End();
        }
    }

    private bool isShowPolygon;

    public void ShowPolygon()
    {
        isShowPolygon = !isShowPolygon;
    }

    private float dt;

    private Vector2[] vertexs;

    private List<Vector2> controlVertexs;
}
