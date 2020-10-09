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
        positionText.text = string.Format("{0}", (Vector2)transform.position);
        positionText.rectTransform.position = Camera.main.WorldToScreenPoint(transform.position);
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