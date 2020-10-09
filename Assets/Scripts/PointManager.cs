using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class PointManager : MonoBehaviour
{
    public Point pointOrg;

    public OperationPanelController operatorUIManager;
    private void Start()
    {
        points = new List<Point>();
        tmpPoints = new List<Point>();
    }

    private void Update()
    {
        UpdatePoint(Input.GetMouseButton((int)MouseButton.LeftMouse));
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
            if (!isInserted && !operatorUIManager.isEnter)
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
        for (int i = 0; i < points.Count; i++)
        {
            if (i < tmpPoints.Count)
            {
                tmpPoints[i] = points[i];
            }
            else
            {
                tmpPoints.Add(points[i]);
            }
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
        SetPointPosition(newPoint, m_mousePosition);
        points.Add(newPoint);
        //Debug.Log("添加了一个控制点");
        IsUpdated = true;
    }

    /// <summary>
    /// 设置控制点坐标
    /// </summary>
    /// <param name="point">控制点引用</param>
    /// <param name="pos">新坐标</param>
    public static void SetPointPosition(Point point, Vector3 pos)
    {
        point.transform.position = new Vector3(pos.x, pos.y, 0.0f);
    }

    /// <summary>
    /// 是否实际更新了控制点(包括添加和拖动)
    /// </summary>
    public bool IsUpdated { get; set; } = false;

    /*TMP*/

    private bool isDraged = false;
    private bool isInRange = false;
    private bool isInserted = false;

    [NonSerialized]
    public List<Point> points;

    [NonSerialized]
    public List<Point> tmpPoints;

    private Vector3 m_mousePosition;

    private Point dragPoint;
}
