using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MainControl : MonoBehaviour
{
    [Tooltip("曲线选择下拉框")]
    public Dropdown curveDrop;

    [Header("相机缩放设置")]
    public Vector2 cameraSizeRange = new Vector2(1.5f, 15.0f);
    [Range(100, 1500)]
    public float scaleSpeed = 1000;
    public float scrollRespons = 0.2f;

    [Header("相机拖动设置")]
    public float dragSpeed = 0.1f;

    private BezierDrawer bezierDrawer;

    private BSplineDrawer bSplineDrawer;

    private Resolution[] resolutions;

    private void Awake()
    {

        resolutions = Screen.resolutions;
#if !UNITY_EDITOR
        Screen.SetResolution(resolutions[resolutions.Length - 1].height * 16 / 9,
            resolutions[resolutions.Length - 1].height, true);
#endif

    }

    private void Start()
    {
        bezierDrawer = GetComponent<BezierDrawer>();
        bSplineDrawer = GetComponent<BSplineDrawer>();
        selectCurve(true);
        tarScale = Camera.main.orthographicSize;
        startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    private void Update()
    {
        ScaleControl();

        PositionControl();
    }

    
    void ScaleControl()
    {
        wheelVal = -Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * scaleSpeed;
        curScale = Camera.main.orthographicSize;
        Camera.main.orthographicSize = Mathf.SmoothDamp(
            curScale, tarScale, ref dt_smooth, scrollRespons);
        tarScale = Mathf.Clamp(tarScale + wheelVal, cameraSizeRange.x, cameraSizeRange.y);
    }

    void PositionControl()
    {
        if (Input.GetMouseButton((int)MouseButton.MiddleMouse))
        {
            if (!isDraged)
            {
                isDraged = true;
                startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            tarPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            curPos = transform.position;
            curPos = Vector2.SmoothDamp(curPos,
                curPos + startPos - tarPos, ref dv_smooth, dragSpeed);
            transform.position = new Vector3(curPos.x, curPos.y, transform.position.z);
        }
        else
        {
            isDraged = false;
        }
    }

    public void OnValueChanged()
    {
        selectCurve(curveDrop.captionText.text.Equals("Bezier曲线"));
    }

    public void OnGameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
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

    /**********************************************************/

    float dt_smooth;
    float curScale;
    float tarScale;
    float wheelVal;

    Vector2 startPos;
    Vector2 curPos;
    Vector2 tarPos;
    Vector2 dv_smooth;
    bool isDraged = false;
}
