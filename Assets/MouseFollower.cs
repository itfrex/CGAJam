using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseFollower : MonoBehaviour
{
    public RectTransform rect;
    void Update()
    {
        rect.anchoredPosition = new Vector3(Mathf.Clamp(rect.anchoredPosition.x + Input.GetAxis("Mouse X"), -160, 160), Mathf.Clamp(rect.anchoredPosition.y + Input.GetAxis("Mouse Y"), -100, 100), 10);
    }
}
