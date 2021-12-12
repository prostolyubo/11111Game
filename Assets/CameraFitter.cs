using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFitter : MonoBehaviour
{
    public Camera target;
    public float horizontalTarget = 5f;

    void Start()
    {
        Debug.Log($"{Screen.width} {Screen.height}");
        target.orthographicSize = horizontalTarget  / Screen.width * Screen.height;
        var position = target.transform.position;
        position.y = target.orthographicSize;
        target.transform.position = position;
    }
}
