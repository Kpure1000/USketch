using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour
{

    public Text positionText;

    public PointType pointType { get; set; }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (IsShowPosition)
        {
            positionText.gameObject.SetActive(true);
            positionText.text = string.Format("{0}", (Vector2)transform.position);
            positionText.rectTransform.position = Camera.main.WorldToScreenPoint(transform.position);
        }
        else
        {
            positionText.gameObject.SetActive(false);
        }
    }

    private void OnMouseEnter()
    {
        if (!isForbidened)
            IsContained = true;
        //Debug.Log("啊我进来了");
    }

    private void OnMouseExit()
    {
        if (!isForbidened)
            IsContained = false;
        //Debug.Log("啊我又出来了");
    }

    public void setHighlight(bool isInRange)
    {
        if (isInRange)
        {
            spriteRenderer.color = Color.cyan;
        }
        else
        {
            spriteRenderer.color = new Color(0, 0, 1.0f, 0.65f);
        }
    }

    public void setColor(Color color)
    {
        spriteRenderer.color = color;
    }

    public void ForbidenDrag(bool isForbiden)
    {
        isForbidened = isForbiden;
    }

    public bool IsShowPosition { get; set; }

    public bool IsContained { get; set; }

    public bool isForbidened;

    private SpriteRenderer spriteRenderer;
}

public enum PointType
{
    Normal, // 普通
    Edge    // 端点
}

public struct Vertex
{
    public VertexType vertexType;
    public Vector2 pos;
    public Color color;
}

public enum VertexType
{
    Normal, // 普通
    Knot    // 节点    
}