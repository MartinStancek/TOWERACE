using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PointerCameraDebug : MonoBehaviour
{
    public GameObject snapUIPrefab;

    private TowerPointerUI towerPointerUI;

    private List<SnapUI> snapsUI;

    void Start()
    {
        towerPointerUI = GetComponent<TowerPointerUI>();
        snapsUI = new List<SnapUI>();
        ShowSnaps();

        for (var i = 0; i < snapsUI.Count; i++)
        {
            Debug.Log("" + snapsUI[i].index + " " + i);
        }
    }

    public void ShowSnaps()
    {
        for (var i = 0; i < GameController.Instance.towersSnapParent.transform.childCount; i++)
        {
            var target = GameController.Instance.towersSnapParent.transform.GetChild(i);
            var pointerRect = GetComponent<RectTransform>();

            //this is your object that you want to have the UI element hovering over
            GameObject WorldObject = target.gameObject;

            var go = Instantiate(snapUIPrefab, transform);
            //this is the ui element
            RectTransform UI_Element = go.GetComponent<RectTransform>();

            //first you need the RectTransform component of your canvas
            RectTransform CanvasRect = GameController.Instance.towerPointerParent.parent.GetComponent<RectTransform>();

            //then you calculate the position of the UI element
            //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.

            Vector2 ViewportPosition = GameController.Instance.mapCamera.WorldToViewportPoint(WorldObject.transform.position);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * pointerRect.pivot.x)),
            ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * pointerRect.pivot.y)));

            //now you can set the position of the ui element
            UI_Element.anchoredPosition = WorldObject_ScreenPosition - towerPointerUI.offset;

            var newSnap = new SnapUI();
            newSnap.index = i;
            newSnap.position = WorldObject_ScreenPosition - towerPointerUI.offset;
            newSnap.go = go;
            Debug.Log(newSnap.ToString());
            snapsUI.Add(newSnap);
        }
    }

    void Update()
    {
        if(towerPointerUI.actualSnapIndex == -1)
        {
            return;
        }
        var direction = Vector2.down;
        foreach(var snapUI in snapsUI)
        {
            var p0 = snapsUI[towerPointerUI.actualSnapIndex].position;
            var dist = 1000f;
            var y_offset = Mathf.Tan(0.25f * Mathf.PI) * dist;
            Vector2 p1, p2;
            if (direction == Vector2.right)
            {
                p1 = new Vector2(p0.x + dist, p0.y + y_offset);
                p2 = new Vector2(p0.x + dist, p0.y - y_offset);
            } 
            else if( direction == Vector2.left)
            {
                p1 = new Vector2(p0.x - dist, p0.y + y_offset);
                p2 = new Vector2(p0.x - dist, p0.y - y_offset);
            }
            else if (direction == Vector2.up)
            {
                p1 = new Vector2(p0.x + y_offset, p0.y + dist);
                p2 = new Vector2(p0.x - y_offset, p0.y + dist);
            }
            else if (direction == Vector2.down)
            {
                p1 = new Vector2(p0.x + y_offset, p0.y - dist);
                p2 = new Vector2(p0.x - y_offset, p0.y - dist);
            }
            else
            {
                throw new System.Exception("Unknown direction vector: " + direction);
            }

            if (PointInTriangle(snapUI.position, p0, p1, p2))
            {
                snapUI.go.GetComponent<Image>().color = Color.cyan;
            } 
            else
            {
                snapUI.go.GetComponent<Image>().color = Color.white;

            }
        }
        snapsUI[towerPointerUI.actualSnapIndex].go.GetComponent<Image>().color = Color.red;



    }

    public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        var s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * p.x + (p0.x - p2.x) * p.y;
        var t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * p.x + (p1.x - p0.x) * p.y;

        if ((s < 0) != (t < 0))
            return false;

        var A = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

        return A < 0 ?
                (s <= 0 && s + t >= A) :
                (s >= 0 && s + t <= A);
    }
}
