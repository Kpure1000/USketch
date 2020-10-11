using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawAxis : MonoBehaviour
{
    private void OnPostRender()
    {
        //GL.Begin(GL.LINE_STRIP);
        //GL.Color(Color.gray);
        //GL.Vertex(new Vector3(-1.5f, 0, 0));
        //GL.Color(Color.gray);
        //GL.Vertex(new Vector3(0, 0, 0));
        //GL.Color(Color.gray);
        //GL.Vertex(new Vector3(1.5f, 0, 0));
        //GL.Color(Color.gray);
        //GL.Vertex(new Vector3(0, 0, 0));
        //GL.Color(Color.gray);
        //GL.Vertex(new Vector3(0, 1.5f, 0));
        //GL.Color(Color.gray);
        //GL.Vertex(new Vector3(0, -1.5f, 0));
        //GL.End();
        BSplineDrawer.DrawVirtualLine(new Vector3(-0.8f, 0, 0), new Vector3(0.8f, 0, 0),Color.magenta, 0.03f);
        BSplineDrawer.DrawVirtualLine(new Vector3(0, -0.8f, 0), new Vector3(0, 0.8f, 0), Color.magenta, 0.03f);
    }
}
