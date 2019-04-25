using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
    public Camera m_Camera;

    private static readonly string MOUSE_SCROLLWHEEL = "Mouse ScrollWheel";
    private static readonly float ZOOM_SPEED = 10.0f;

    // Update is called once per frame
    void Update() {
        WheelMove();
        CameraMovement();
    }

    private void WheelMove() {
        float scroll = Input.GetAxis(MOUSE_SCROLLWHEEL);
        m_Camera.orthographicSize += scroll * ZOOM_SPEED;
    }

    private void CameraMovement() {
        //transform.LookAt(m_Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, m_Camera.nearClipPlane)), Vector3.up);
        /*if (Input.GetButton("Fire1")) {
            //Hay que modificar estos valores del transform
            Debug.Log(ToString() + Input.mousePosition);
            m_Camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, m_Camera.transform.position.z, Input.mousePosition.z));
        }*/
    }
}
