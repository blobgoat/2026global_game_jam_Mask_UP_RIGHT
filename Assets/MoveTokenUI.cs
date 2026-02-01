using UnityEngine;
using UnityEngine.EventSystems;

public class MoveTokenUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Transform originalParent;
    public Vector3 originalPos;
    public PlanAction action;
    [HideInInspector] public bool wasDroppedInSlot = false;
    public bool isPaletteToken = true; // true if this token is in the palette
    MoveTokenUI dragInstance; // the clone we actually drag

    Canvas rootCanvas;
    CanvasGroup cg;
    RectTransform rootCanvasRect;
    RectTransform rt;



    void Awake()
    {
        rootCanvas = GetComponentInParent<Canvas>();
        cg = gameObject.GetComponent<CanvasGroup>();
        rootCanvasRect = rootCanvas.GetComponent<RectTransform>();
        rt = GetComponent<RectTransform>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (wasDroppedInSlot == false)
        {
            GameObject cloneGo = Instantiate(gameObject, rootCanvas.transform);
            cloneGo.transform.localScale = Vector3.one;

            RectTransform src = GetComponent<RectTransform>();
            RectTransform dst = cloneGo.GetComponent<RectTransform>();

            dst.anchorMin = dst.anchorMax = new Vector2(0.5f, 0.5f); // stop stretching
            dst.sizeDelta = src.rect.size;                           // copy size
            dst.localScale = Vector3.one;


            dragInstance = cloneGo.GetComponent<MoveTokenUI>();
            dragInstance.isPaletteToken = false;
            dragInstance.wasDroppedInSlot = false;
            dragInstance.rootCanvas = rootCanvas;
            dragInstance.rootCanvasRect = rootCanvasRect;
            if (dragInstance.cg == null) dragInstance.cg = cloneGo.GetComponent<CanvasGroup>();
            dragInstance.rt = dst;



            cloneGo.transform.SetAsLastSibling(); // make sure it's on top of other UI
            dragInstance.SetDraggedPosition(eventData);
            // Important: make the clone be the dragged object
            eventData.pointerDrag = cloneGo;

            // Clone should not block raycasts while dragging
            dragInstance.cg.blocksRaycasts = false;
        }
        else
        {
            Debug.Log("Dragging existing token");
            // dragInstance = this;
            // originalParent = transform.parent;
            // originalPos = transform.position;
            // cg.blocksRaycasts = false;
        }

    }

    public void OnDrag(PointerEventData eventData)
    {
        if (wasDroppedInSlot == false)
        {

            var go = eventData.pointerDrag;
            if (go == null) return;

            var ui = go.GetComponent<MoveTokenUI>();
            if (ui == null) return;
            ui.SetDraggedPosition(eventData);
        }
        else
        {
            //do nothing
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (wasDroppedInSlot == true)
        {
            return;
        }
        var go = eventData.pointerDrag;
        if (go == null) return;

        var ui = go.GetComponent<MoveTokenUI>();
        if (ui == null) return;

        if (ui.cg != null) ui.cg.blocksRaycasts = true;


        // if (dragInstance == null) return;

        // dragInstance.cg.blocksRaycasts = true;

        // If not dropped into a slot, destroy the clone
        if (ui.isPaletteToken == false && ui.wasDroppedInSlot == false)
        {
            Destroy(go);
        }

        dragInstance = null;
    }
    void SetDraggedPosition(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvasRect,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPos
        );
        rt.anchoredPosition = localPos;
    }
}