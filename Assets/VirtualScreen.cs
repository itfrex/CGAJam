using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class VirtualScreen : GraphicRaycaster
{
    public Camera screenCamera; // Reference to the camera responsible for rendering the virtual screen's rendertexture
 
    public GraphicRaycaster screenCaster; // Reference to the GraphicRaycaster of the canvas displayed on the virtual screen
    public Image debugElement;
    // Called by Unity when a Raycaster should raycast because it extends BaseRaycaster.
    public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
    {
        screenCaster.Raycast(eventData, resultAppendList);
        //EventSystem.current.RaycastAll(eventData, resultAppendList);
    }
}