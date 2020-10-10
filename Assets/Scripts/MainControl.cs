using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainControl : MonoBehaviour
{
    [Tooltip("曲线选择下拉框")]
    public Dropdown curveDrop;

    private BezierDrawer bezierDrawer;

    private BSplineDrawer bSplineDrawer;

    private void Start()
    {
        bezierDrawer = GetComponent<BezierDrawer>();
        bSplineDrawer = GetComponent<BSplineDrawer>();
        selectCurve(true);
    }

    public void OnValueChanged()
    {
        selectCurve(curveDrop.captionText.text.Equals("Bezier曲线"));
    }

    private void selectCurve(bool isBezier)
    {
        if (isBezier)
        {
            Debug.Log("切换至贝塞尔曲线");
            bSplineDrawer.CloseDraw();
            bSplineDrawer.enabled = false;
            bezierDrawer.enabled = true;
            bezierDrawer.RestartDraw();
        }
        else
        {
            Debug.Log("切换至B样条曲线");
            bezierDrawer.CloseDraw();
            bezierDrawer.enabled = false;
            bSplineDrawer.enabled = true;
            bSplineDrawer.RestartDraw();
        }
    }

}
