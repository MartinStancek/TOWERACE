using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerPointerUI : MonoBehaviour
{
    public Image skewLine;
    public Image verticalLine;
    public Image narrowLine;
    public Image circlePointer;

    public RectTransform selectTower;
    public RectTransform selectSpot;
    public RectTransform buySpot;
    public RectTransform tower;

    public float horizontalOffsetWidth = 20f;
    public float verticalOffsetWidth = 5f;

    public int actualSnapIndex = -1;

    public float horizontalOffsetWidthVertical = 20f;
    public float verticalOffsetWidthVertical = 5f;

    public Vector2 offset = new Vector2(10f, 10f);

    private TMP_Text towerName;

    public RectTransform leftArrow;
    public RectTransform rightArrow;
    public RectTransform upArrow;
    public RectTransform downArrow;

    void Start()
    {
        SetPanel(null);
        towerName = transform.Find("Tower/NamePanel/Name").GetComponent<TMP_Text>();
    }

    public void SetTowerName(string text)
    {
        towerName.text = text;
    }

    public void SetPanel(RectTransform panel)
    {
        selectTower.gameObject.SetActive(false);
        selectSpot.gameObject.SetActive(false);
        buySpot.gameObject.SetActive(false);
        tower.gameObject.SetActive(false);

        if(panel != null)
        {
            panel.gameObject.SetActive(true);
            circlePointer.gameObject.SetActive(true);
            narrowLine.gameObject.SetActive(true);
        }
        else
        {
            circlePointer.gameObject.SetActive(false);
            narrowLine.gameObject.SetActive(false);
        }
    }

    public void SetPointer(GameObject target)
    {
        var pointerRect = GetComponent<RectTransform>();
        actualSnapIndex = target.transform.GetSiblingIndex();

        //this is your object that you want to have the UI element hovering over
        GameObject WorldObject = target.gameObject;

        //this is the ui element
        RectTransform UI_Element = circlePointer.rectTransform;

        //first you need the RectTransform component of your canvas
        RectTransform CanvasRect = GameController.Instance.towerPointerParent.parent.GetComponent<RectTransform>();

        //then you calculate the position of the UI element
        //0,0 for the canvas is at the center of the screen, whereas WorldToViewPortPoint treats the lower left corner as 0,0. Because of this, you need to subtract the height / width of the canvas * 0.5 to get the correct position.

        Vector2 ViewportPosition = GameController.Instance.mapCamera.WorldToViewportPoint(WorldObject.transform.position);
        Vector2 WorldObject_ScreenPosition = new Vector2(
        ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * pointerRect.pivot.x)),
        ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * pointerRect.pivot.y)));

        //now you can set the position of the ui element
        UI_Element.anchoredPosition = WorldObject_ScreenPosition - offset;
        var height = Mathf.Abs(circlePointer.rectTransform.anchoredPosition.y) + narrowLine.rectTransform.anchoredPosition.y;
        var length = height / Mathf.Sin(0.25f * Mathf.PI);
        //Debug.Log(height);
        skewLine.rectTransform.sizeDelta = new Vector2(Mathf.Abs(length+ verticalOffsetWidth), skewLine.rectTransform.sizeDelta.y);
        var width = height / Mathf.Tan(0.25f * Mathf.PI);
        var horizontalWidth = (Mathf.Abs(circlePointer.rectTransform.anchoredPosition.x) - width + horizontalOffsetWidth);
        if(horizontalWidth > selectTower.rect.width)
        {
            verticalLine.gameObject.SetActive(false);
            skewLine.gameObject.SetActive(true);
            narrowLine.rectTransform.sizeDelta = new Vector2(horizontalWidth, skewLine.rectTransform.sizeDelta.y);
        } 
        else
        {
            verticalLine.gameObject.SetActive(true);
            skewLine.gameObject.SetActive(false);
            horizontalWidth = Mathf.Abs(circlePointer.rectTransform.anchoredPosition.x) + horizontalOffsetWidthVertical;
            var verticalWidth = Mathf.Abs(circlePointer.rectTransform.anchoredPosition.y) + verticalOffsetWidthVertical;
            narrowLine.rectTransform.sizeDelta = new Vector2(horizontalWidth, skewLine.rectTransform.sizeDelta.y);
            verticalLine.rectTransform.sizeDelta = new Vector2(verticalWidth, verticalLine.rectTransform.sizeDelta.y);
        }

        SetArrow(upArrow, Vector2.up);
        SetArrow(downArrow, Vector2.down);
        SetArrow(rightArrow, Vector2.right);
        SetArrow(leftArrow, Vector2.left);
    }

    private void SetArrow(RectTransform arrow, Vector2 direction)
    {
        var player = GameController.Instance.players[transform.GetSiblingIndex()];
        var snaps = GameController.Instance.GetFreeTowerSnapsInDirection(actualSnapIndex, direction, player);
        if(snaps.Count > 0)
        {
            arrow.gameObject.SetActive(true);
            var positioNext = GameController.Instance.snapsUI[snaps[0].transform.GetSiblingIndex()].position;
            var position = GameController.Instance.snapsUI[actualSnapIndex].position;
            var dir = positioNext - (position + (Vector2.one * 35 * direction));

            arrow.eulerAngles = new Vector3(0, 0f, Vector2.SignedAngle(Vector2.right, dir));

        }
        else
        {
            arrow.gameObject.SetActive(false);
        }
    }

    public void SetColor(Color c)
    {
        skewLine.color = c;
        verticalLine.color = c;
        narrowLine.color = c;
        circlePointer.color = c;
        selectTower.GetComponent<Image>().color = c;
        selectSpot.GetComponent<Image>().color = c;
        buySpot.GetComponent<Image>().color = c;
        tower.GetComponent<Image>().color = c;
        upArrow.GetComponentInChildren<Image>().color = c;
        downArrow.GetComponentInChildren<Image>().color = c;
        rightArrow.GetComponentInChildren<Image>().color = c;
        leftArrow.GetComponentInChildren<Image>().color = c;
    }
}
