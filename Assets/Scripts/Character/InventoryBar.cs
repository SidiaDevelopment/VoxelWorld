using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryBar : MonoBehaviour
{
    [SerializeField] public List<Texture2D> Images = new List<Texture2D>();

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
            Image itemImage = slot.GetChild(1).GetComponent<Image>();
            TextMeshProUGUI itemCount = slot.GetChild(2).GetComponent<TextMeshProUGUI>();

            itemImage.enabled = false;
            itemCount.enabled = false;
        }

        int i = 0;

        foreach (InventoryItem item in (sender as Inventory).Items)
        {
            Image itemImage = inventoryPanel.GetChild(i).GetChild(1).GetComponent<Image>();
            TextMeshProUGUI itemCount = inventoryPanel.GetChild(i++).GetChild(2).GetComponent<TextMeshProUGUI>();

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
            Image selectedImage = slot.GetChild(0).GetComponent<Image>();

            selectedImage.enabled = position == i++;
        }
    }
}
