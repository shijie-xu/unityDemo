using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {
    public GameObject Camera;
    public Rigidbody rd;
    public float moveSpeed = 5;
    public float rotateSpeed;

    private Camera camera;
	// Use this for initialization
	void Start () {
        camera = GetComponentInChildren<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 speed = new Vector3(h, 0, v);
        rd.velocity = speed * moveSpeed;

        //float X = Input.GetAxis("Mouse ScrollWheel") * rotateSpeed;
        //float Y = Input.GetAxis("Mouse Y") * rotateSpeed;
        //camera.transform.localRotation *= Quaternion.Euler(-Y, 0, 0);
        //transform.localRotation *= Quaternion.Euler(0, X, 0);
        //if (X > 1e-2)
        //{
        //    speed = new Vector3(0, X, 0);
        //    rd.velocity = speed;
        //}
    }
}
