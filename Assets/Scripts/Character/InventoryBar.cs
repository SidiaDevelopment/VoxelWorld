using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryBar : MonoBehaviour
{
    [SerializeField] public List<Texture2D> Images = new List<Texture2D>();

    private enum InventorySlotUI
    {
        ITEM_SELECTED = 0,
        ITEM_ICON,
        ITEM_COUNT
    }

    void Start()
    {
        GetComponentInParent<Inventory>().ItemsUpdated += InventoryUpdated;
        GetComponentInParent<Inventory>().PositionUpdated += PositionUpdated;
        PositionUpdated(null, GetComponentInParent<Inventory>().CurrentlySelected);
    }

    private void InventoryUpdated(object sender, InventoryEventArgs e)
    {
        Transform inventoryPanel = transform.Find("Panel");

        foreach (Transform slot in inventoryPanel)
        {
            Image itemImage = slot.GetChild((int)InventorySlotUI.ITEM_ICON).GetComponent<Image>();
            TextMeshProUGUI itemCount = slot.GetChild((int)InventorySlotUI.ITEM_COUNT).GetComponent<TextMeshProUGUI>();

            itemImage.enabled = false;
            itemCount.enabled = false;
        }

        Inventory inventory = sender as Inventory;

        for (int i = 0; i < inventory.ItemCount; i++)
        {
            InventoryItem item = inventory.GetItem(i);

            if (item == null) continue;

            Image itemImage = inventoryPanel.GetChild(i).GetChild((int)InventorySlotUI.ITEM_ICON).GetComponent<Image>();
            TextMeshProUGUI itemCount = inventoryPanel.GetChild(i).GetChild((int)InventorySlotUI.ITEM_COUNT).GetComponent<TextMeshProUGUI>();

            itemImage.enabled = true;
            itemCount.enabled = true;

            Texture2D spriteImage = Images[(int)item.BlockType - 1];
            itemImage.sprite = Sprite.Create(spriteImage, new Rect(0, 0, spriteImage.width, spriteImage.height), new Vector2(0.5f, 0.5f));

            itemCount.text = item.GetCount().ToString();
        }
    }

    private void PositionUpdated(object sender, int position)
    {
        Transform inventoryPanel = transform.Find("Panel");

        int i = 0;
        foreach (Transform slot in inventoryPanel)
        {
            Image selectedImage = slot.GetChild((int)InventorySlotUI.ITEM_SELECTED).GetComponent<Image>();

            selectedImage.enabled = position == i++;
        }
    }
}
