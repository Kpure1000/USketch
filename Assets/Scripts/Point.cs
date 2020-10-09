using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Point : MonoBehaviour
{
    public PointType pointType { get; set; }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseEnter()
    {
        IsContained = true;
        //Debug.Log("啊我进来了");
    }

    private void OnMouseExit()
    {
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
            spriteRenderer.color = Color.blue;
        }
    }

    public bool IsContained { get; set; }

    private SpriteRenderer spriteRenderer;
}

public enum PointType
{
    Normal, // 普通
    Edge    // 端点
}