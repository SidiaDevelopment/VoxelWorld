using UnityEngine;

public class DroppedItem : MonoBehaviour
{
    [SerializeField] public int Speed = 10;
    [SerializeField] public float DroppedItemScale = 0.25f;
    [SerializeField] public GameObject DroppedItemPrefab;

    private void Start()
    {
        GameObject instance = Instantiate(DroppedItemPrefab, transform.position, Quaternion.identity, transform);
        instance.name = "droppedItem";
        instance.transform.localScale = new Vector3(DroppedItemScale, DroppedItemScale, DroppedItemScale);
        instance.layer = 9;
        instance.transform.localPosition = new Vector3(0f, Random.Range(0f, 0.01f), 0f);

        foreach (Transform child in instance.transform)
        {
            child.gameObject.layer = 9;
        }

        GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(0.0f, 40f), 200f, Random.Range(0.0f, 40f)));
    }

    void Update()
    {
        transform.GetChild(0).Rotate(Vector3.up, Speed * Time.deltaTime);
    }
}
