using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// takes player input, shows the pointer and passes the controls on if required
// created 31/8/23
// last modified 1/9/23

public class PlayerControls : MonoBehaviour
{
    [SerializeField] private PlayerPawn player;
    [SerializeField] private UIPointer pointer;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private List<RaycastResult> GetUIObjects(Vector2 pos)
    {
        // check if the object is the interactionmenu
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = pos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results;
    }

    private void ShowPointer(Vector2 touchPos, bool touch)
    {
        pointer.ShowPointer(touchPos);

        if (player && touch)
        {
            List<RaycastResult> objectsUI = GetUIObjects(touchPos);

            foreach (RaycastResult objectUI in objectsUI)
            {
                Button objectButton = objectUI.gameObject.GetComponentInChildren<Button>();
                if (objectButton) return;
            }

            player.SetMove(touchPos);
        }
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            ShowPointer(Input.touches[0].position, true);
        }
        else if (Input.mousePresent)
        {
            ShowPointer(Input.mousePosition, Input.GetMouseButton(0));
        }
    }
}
