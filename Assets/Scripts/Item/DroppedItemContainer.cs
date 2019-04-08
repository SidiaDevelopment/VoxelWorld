using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemContainer : MonoBehaviour
{
    [SerializeField] public GameObject DroppedItemPrefab;
    [SerializeField] public int Speed = 20;
    [SerializeField] public float DroppedItemScale = 0.25f;

    void Start()
    {
        GameObject instance = Instantiate(DroppedItemPrefab, transform.position, Quaternion.identity, transform);
        instance.name = "block";
        instance.transform.localScale = new Vector3(DroppedItemScale, DroppedItemScale, DroppedItemScale);
        instance.layer = 9;
        instance.transform.localPosition = new Vector3(0f, Random.Range(0f, 0.01f), 0f);

        foreach (Transform child in instance.transform)
        {
            child.gameObject.layer = 9;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).Rotate(Vector3.up, Speed * Time.deltaTime);
    }
}
