using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipUI : MonoBehaviour
{
    /*
    List<Item> items = new List<Item>();
    public TMP_Text header;
    public TMP_Text itemText;

    Inventory inventory;
    EquipmentManager equip;
    */
    /*
    private void Start()
    {
        equip = EquipmentManager.instance;
        inventory = Inventory.instance;
 
        inventory.onItemChangedCallback += UpdateTooltip;
        equip.onEquipmentChangedUI += UpdateTooltip;
    }
    */
    private void Update()
    {
        
    }
    
    /*
    public void UpdateTooltip()
    {
        //items = inventory.items;
        Debug.Log("Updating tooltip");

        for (int i = 0; i < inventory.items.Count; i++)
        {
            if (inventory.items[i] != null)
            {
                header.text = inventory.items[i].name;
                itemText.text = inventory.items[i].tooltipText;
                Debug.Log(inventory.items[i] + " " + inventory.items[i].name);
                RectTransform rt = (RectTransform)transform;
                transform.position = Input.mousePosition + new Vector3(-rt.rect.width / 1.8f, rt.rect.height / 2, 0);
            }
            else
            {
                Debug.Log("Got into else items is null");
                gameObject.SetActive(false);
            }
        }
       
     //   Debug.Log("position ist " + transform.position);
    }
    */
}
