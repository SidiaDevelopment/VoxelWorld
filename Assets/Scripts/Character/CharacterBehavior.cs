using UnityEngine;

public class CharacterBehavior : MonoBehaviour
{

    [SerializeField] public float moveSpeed = 2;
    [SerializeField] public float jumpForce = 1;
    [SerializeField] public string DroppedItemTag = "DroppedItem";

    private float m_currentV = 0;
    private float m_currentH = 0;
    private float m_speedMult = 1f;
    private readonly float m_interpolation = 10;

    void Update()
    {
        UpdateMovement();
        UpdateJump();
        UpdateSprint();
    }

    private void UpdateMovement()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        transform.position += transform.forward * m_currentV * moveSpeed * m_speedMult * Time.deltaTime;
        transform.position += transform.right * m_currentH * moveSpeed * m_speedMult * Time.deltaTime;
    }

    private void UpdateJump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * 300 * jumpForce);
        }
    }

    private void UpdateSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            m_speedMult = 2f;
        }
        else
        {
            m_speedMult = 1f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == DroppedItemTag)
        {
            Inventory inventory = GetComponent<Inventory>();
            if (!inventory.CanAddItem()) return;

            inventory.AddItem(other.gameObject.GetComponent<DroppedItem>().BlockType);
            Destroy(other.gameObject);
        }
    }
}
