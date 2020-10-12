using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 控制点
/// </summary>
public class Point : MonoBehaviour
{

    public Text positionText;

    public PointType pointType { get; set; }

    public HighLightType highLightType { get; set; }

    public string pName { get; set; }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (IsShowPosition)
        {
            positionText.gameObject.SetActive(true);
            positionText.text = pName + (Vector2)transform.position;
            positionText.rectTransform.position = Camera.main.WorldToScreenPoint(transform.position);
        }
        else
        {
            positionText.gameObject.SetActive(false);
        }
    }

    private void OnMouseEnter()
    {
        IsContained = true && !isForbidened;
    }

    private void OnMouseExit()
    {
        IsContained = false;
    }

    public void setHighlight(HighLightType type)
    {
        highLightType = type;
        switch (type)
        {
            case HighLightType.NONE:
                spriteRenderer.color = new Color(0, 1.0f, 1.0f, 0.65f);
                break;
            case HighLightType.SINGLW:
                spriteRenderer.color = new Color(1.0f, 0, 0f, 0.65f);
                break;
            case HighLightType.GROUP:
                spriteRenderer.color = new Color(1.0f, 0, 0f, 0.65f);
                break;
            default:
                break;
        }
    }

    public void setColor(Color color)
    {
        spriteRenderer.color = color;
    }

    /// <summary>
    /// 禁止拖拽
    /// </summary>
    /// <param name="isForbiden"></param>
    public void ForbidenDrag(bool isForbiden)
    {
        isForbidened = isForbiden;
    }

    public bool IsShowPosition { get; set; }

    public bool IsContained { get; set; }

    public bool isForbidened;

    private SpriteRenderer spriteRenderer;
}
/// <summary>
/// 控制点类型
/// </summary>
public enum PointType
{
    Normal, // 普通
    Edge    // 端点
}

public enum HighLightType
{
    NONE,   //无高光
    SINGLW, //单个高光
    GROUP   //组高光
}

/// <summary>
/// 曲线上的点
/// </summary>
public struct Vertex
{
    public VertexType vertexType;
    public Vector2 pos;
    public Color color;
}
/// <summary>
/// 曲线点类型
/// </summary>
public enum VertexType
{
    Normal, // 普通
    Knot    // 节点    
}