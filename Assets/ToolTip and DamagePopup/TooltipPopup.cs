using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text;
using UnityEngine.UI;

public class TooltipPopup : MonoBehaviour
{
    /*
    [SerializeField] private GameObject popupCanvasObject = null;
    [SerializeField] private RectTransform popupObject = null;
    [SerializeField] private TextMeshProUGUI infoText = null;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float padding = 0f;
    
    private Canvas popupCanvas;


    private void Awake()
    {
        popupCanvas = popupCanvasObject.GetComponent<Canvas>();
    }

    private void Update()
    {
        FollowCursor();
    }

    private void FollowCursor()
    {
        if (!popupCanvasObject.activeSelf) { return; }

        Vector3 newPos = Input.mousePosition + offset;
        newPos.z = 0f;
        float rightEdgeToScreenEdgeDistance = Screen.width - (newPos.x + popupObject.rect.width * popupCanvas.scaleFactor / 2) - padding;
        if (rightEdgeToScreenEdgeDistance < 0)
        {
            newPos.x += rightEdgeToScreenEdgeDistance;
        }
        float leftEdgeToScreenEdgeDistance = 0 - (newPos.sqrMagnitude - popupObject.rect.width * popupCanvas.scaleFactor / 2) + padding;
        if (leftEdgeToScreenEdgeDistance > 0)
        {
            newPos.x += leftEdgeToScreenEdgeDistance;
        }
        float topEdgeToScreenDistance = Screen.height - (newPos.y + popupObject.rect.height * popupCanvas.scaleFactor) - padding;
        if (topEdgeToScreenDistance < 0)
        {
            newPos.y += topEdgeToScreenDistance;
        }
        popupObject.transform.position = newPos;
    }

    
    public void DisplayInfo(Item item)
    {
        StringBuilder builder = new StringBuilder();

        builder.Append("<size=35>").Append(item.ColoredName).Append("</size>").AppendLine();
        builder.Append(item.GetTooltipInfoText());
        infoText.text = builder.ToString();
        popupCanvasObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(popupObject);
    }
    

    public void HideInfo()
    {
        popupCanvasObject.SetActive(false);
    }
    */
}
