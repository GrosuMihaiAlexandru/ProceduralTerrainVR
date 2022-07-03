using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour
{
    public Camera camera;
    public WorldGenerator world;
    // Update is called once per frame

    [SerializeField]
    private float moveSpeed = 5f;

    [SerializeField]
    private float sprintMoveSpeed = 12f;

    [SerializeField]
    private float lookSpeedH = 2f;

    [SerializeField]
    private float lookSpeedV = 2f;

    private float yaw = 0f;
    private float pitch = 0f;

    private void Start()
    {
        // Initialize the correct initial rotation
        yaw = this.transform.eulerAngles.y;
        pitch = this.transform.eulerAngles.x;
    }

    private void Update()
    {
        // Move Up Down Left Right
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float forward = Input.GetAxis("Forward");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.Translate(horizontal * sprintMoveSpeed * Time.deltaTime, vertical * sprintMoveSpeed * Time.deltaTime, forward * sprintMoveSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            transform.Translate(horizontal * moveSpeed * Time.deltaTime, vertical * moveSpeed * Time.deltaTime, forward * moveSpeed * Time.deltaTime, Space.Self);
        }
        //Look around with Left Mouse
        if (Input.GetMouseButton(1))
        {
            yaw += lookSpeedH * Input.GetAxis("Mouse X");
            pitch -= lookSpeedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0f);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Terrain")
                {
                    world.GetChunkFromVector3(hit.transform.position).PlaceTerrain(hit.point);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Terrain")
                {
                    world.GetChunkFromVector3(hit.transform.position).RemoveTerrain(hit.point);
                }
            }
        }
    }
}
