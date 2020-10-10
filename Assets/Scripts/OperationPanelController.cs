using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// 控制用户操作面板 的 动画
/// </summary>
public class OperationPanelController : MonoBehaviour, IPointerEnterHandler
{
    /// <summary>
    /// 主操作面板
    /// </summary>
    [Tooltip("需要被鼠标控制位置的操作Panel")]
    public GameObject MainPanel;

    /// <summary>
    /// 移动速度
    /// </summary>
    public float Speed;
    /// <summary>
    /// 激活位置
    /// </summary>
    public Vector2 EnablePosition;
    /// <summary>
    /// 休眠位置
    /// </summary>
    public Vector2 DisablePosition;
    /// <summary>
    /// 目标位置
    /// </summary>
    private Vector2 targetPosition;
    /// <summary>
    /// UI坐标组件
    /// </summary>
    private RectTransform rectTransform;

    public bool isEnter { get; set; } = false;

    private void Start()
    {
        rectTransform = MainPanel.GetComponent<RectTransform>();

        targetPosition = DisablePosition;

    }

    private void Update()
    {
        rectTransform.anchoredPosition = Vector2.Lerp(rectTransform.anchoredPosition, targetPosition, Speed * Time.unscaledDeltaTime);
    }
    /// <summary>
    /// 鼠标在内
    /// </summary>
    /// <param name="eventData"></param>
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        isEnter = !isEnter;
        if (isEnter)
        {
            targetPosition = EnablePosition;
        }
        else
        {
            targetPosition = DisablePosition;
        }
    }


}
