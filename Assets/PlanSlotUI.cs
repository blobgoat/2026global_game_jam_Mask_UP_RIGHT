// using System.Threading.Tasks.Dataflow;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlanSlotUI : MonoBehaviour, IDropHandler
{
    [Tooltip("Index in the plan array (0–4)")]
    public int slotIndex;

    [HideInInspector]
    public MoveTokenUI currentToken;

    public void OnDrop(PointerEventData eventData)
    {
        var token = eventData.pointerDrag?.GetComponent<MoveTokenUI>();
        if (token == null) return;
        DestroyAllChildren(transform);
        token.wasDroppedInSlot = true;

        token.transform.SetParent(transform, false);

        token.transform.SetAsLastSibling(); // make sure it's on top of other UI

        if (currentToken != null)
        {
            //delete the token from reference
            Destroy(currentToken.gameObject);
            currentToken = null;
        }
        // Parent token to this slot

        var rt = token.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }


        var img = token.GetComponent<Image>();
        if (img != null)
        {
            var c = img.color;
            c.a = 1f; // make sure it's fully opaque
            img.color = c;
            img.enabled = true;
        }

        currentToken = token;

        if (PlanRunner.Instance == null)
        {
            Debug.LogError("PlanRunner Instance is null!");
            return;
        }
        if (token.action.Equals(default(PlanAction)))
        {
            Debug.LogError("Token action is default (unset)!");
            return;
        }
        if (slotIndex < 0 || slotIndex >= 5)
        {
            Debug.LogError("Slot index out of range: " + slotIndex);
            return;
        }



        // Tell the game logic
        PlanRunner.Instance.SetAction(slotIndex, token.action);
    }

    void DestroyAllChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            var child = parent.GetChild(i);
            GameObject.Destroy(child.gameObject);
        }
    }
}