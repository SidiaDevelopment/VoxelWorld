﻿using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    [SerializeField] public float sensitivityX = 10f;
    [SerializeField] public float sensitivityY = 10f;
    [SerializeField] public float minimumX = -85f;
    [SerializeField] public float maximumX = 85f;

    [SerializeField] public float interactionDistance = 6f;
    [SerializeField] public Texture2D crosshairImage;
    [SerializeField] public string colliderTag = "Chunk";

    [SerializeField] public GameObject world;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        UpdateCursor();
        UpdateCamera();
        UpdateInteraction();
    }

    void OnGUI()
    {
        float xMin = (Screen.width / 2) - (crosshairImage.width / 2);
        float yMin = (Screen.height / 2) - (crosshairImage.height / 2);
        GUI.DrawTexture(new Rect(xMin, yMin, crosshairImage.width, crosshairImage.height), crosshairImage);
    }

    protected void UpdateCursor()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
        {
            // Debug stuff
            if (Input.GetMouseButton(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    protected void UpdateCamera()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Transform camera = transform.Find("MainCamera");

        float rotationX = Input.GetAxis("Mouse X") * sensitivityX;
        float rotationY = Input.GetAxis("Mouse Y") * sensitivityY;

        Vector3 cameraRotation = camera.localRotation.eulerAngles;
        Vector3 playerRotation = transform.localRotation.eulerAngles;

        playerRotation.y += rotationX;

        if (cameraRotation.x > 180)
        {
            cameraRotation.x = Mathf.Max(cameraRotation.x - rotationY, 360F + minimumX);
        }
        else
        {
            cameraRotation.x = Mathf.Min(cameraRotation.x - rotationY, maximumX);
        }

        camera.localRotation = Quaternion.Euler(cameraRotation);
        transform.localRotation = Quaternion.Euler(playerRotation);
    }

    protected void UpdateInteraction()
    {
        Transform camera = transform.Find("MainCamera");

        // Don't raycast player model
        int layerMask = 1 << 9;
        layerMask = ~layerMask;

        // Left mouse button down
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.position, camera.TransformDirection(Vector3.forward), out hit, interactionDistance, layerMask))
            {
                if (hit.collider.tag.Equals(colliderTag))
                {
                    Vector3 localPosition = hit.transform.InverseTransformPoint(hit.point);

                    localPosition.x = Mathf.FloorToInt(localPosition.x - (hit.normal.x / 2));
                    localPosition.y = Mathf.FloorToInt(localPosition.y - (hit.normal.y / 2));
                    localPosition.z = Mathf.FloorToInt(localPosition.z - (hit.normal.z / 2));

                    hit.transform.GetComponent<VolumeChunk>().RemoveVoxel((int)localPosition.x, (int)localPosition.y, (int)localPosition.z);
                }
            }
        }

        // Right mouse button down
        if (Input.GetMouseButtonDown(1))
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.position, camera.TransformDirection(Vector3.forward), out hit, interactionDistance, layerMask))
            {
                if (hit.collider.tag.Equals(colliderTag))
                {
                    Vector3 localPosition = hit.transform.InverseTransformPoint(hit.point);

                    localPosition.x = Mathf.FloorToInt(localPosition.x - (hit.normal.x / 2)) + hit.normal.x;
                    localPosition.y = Mathf.FloorToInt(localPosition.y - (hit.normal.y / 2)) + hit.normal.y;
                    localPosition.z = Mathf.FloorToInt(localPosition.z - (hit.normal.z / 2)) + hit.normal.z;

                    VolumeChunk currentChunk = hit.transform.GetComponent<VolumeChunk>();
                    World currentWorld = world.GetComponent<World>();
                    if (localPosition.x == 16)
                    {
                        currentWorld.GetChunk(currentChunk.x + 1, currentChunk.y).GetComponent<VolumeChunk>().CreateVoxel(0, (int)localPosition.y, (int)localPosition.z);
                        return;
                    }
                    if (localPosition.x == -1)
                    {
                        currentWorld.GetChunk(currentChunk.x - 1, currentChunk.y).GetComponent<VolumeChunk>().CreateVoxel(15, (int)localPosition.y, (int)localPosition.z);
                        return;
                    }
                    if (localPosition.z == 16)
                    {
                        currentWorld.GetChunk(currentChunk.x, currentChunk.y + 1).GetComponent<VolumeChunk>().CreateVoxel((int)localPosition.x, (int)localPosition.y, 0);
                        return;
                    }
                    if (localPosition.z == -1)
                    {
                        currentWorld.GetChunk(currentChunk.x, currentChunk.y - 1).GetComponent<VolumeChunk>().CreateVoxel((int)localPosition.x, (int)localPosition.y, 15);
                        return;
                    }

                    hit.transform.GetComponent<VolumeChunk>().CreateVoxel((int)localPosition.x, (int)localPosition.y, (int)localPosition.z);
                }
            }
        }
    }
}