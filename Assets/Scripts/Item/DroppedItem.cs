using UnityEngine;
using System.Collections;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] public GameObject DroppedItemPrefab;
    [SerializeField] public GameObject DroppedItemContainerPrefab;
    [SerializeField] public BlockTypes BlockType;

    private void Start()
    {
        GameObject instance = Instantiate(DroppedItemContainerPrefab, transform.position, Quaternion.identity, transform);
        instance.name = "itemContainer";
        instance.GetComponent<DroppedItemContainer>().DroppedItemPrefab = DroppedItemPrefab;

        GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-40f, 40f), 200f, Random.Range(-40f, 40f)));

        StartCoroutine(WaitForPickupInit());
    }

    IEnumerator WaitForPickupInit()
    {
        yield return new WaitForSeconds(1);
        GetComponent<SphereCollider>().enabled = true;
    }
}