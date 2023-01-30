using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Observe : MonoBehaviour
{
    public float MoveSpeed;
    public float RotateSpeed;
    // Start is called before the first frame update
    private void Start()
    {
        Cursor.visible = false; // Make the mouse ptr disappear
        MoveSpeed = 20f;
        RotateSpeed = 20f;
    }
    private void Move()
    {
        float Horizontal = Input.GetAxis("Horizontal");
        float Vertical = Input.GetAxis("Vertical");

        // Move when "w a s d" is pressed
        if (Mathf.Abs(Vertical) > 0.01)
        {
            // move forward
            transform.Translate(MoveSpeed * Time.deltaTime * Vertical * transform.forward, Space.World);
        }
        if (Mathf.Abs(Horizontal) > 0.01)
        {
            // move aside 
            transform.Translate(MoveSpeed * Time.deltaTime * Horizontal * transform.right, Space.World);
        }
    }
    private void Rotate()
    {
        float MouseX = Input.GetAxis("Mouse X");
        float MouseY = Input.GetAxis("Mouse Y");

        if ((Mathf.Abs(MouseX) > 0.01 || Mathf.Abs(MouseY) > 0.01))
        {
            transform.Rotate(new Vector3(0, MouseX * RotateSpeed * Time.deltaTime, 0), Space.World);
            transform.Rotate(new Vector3(-MouseY * RotateSpeed * Time.deltaTime * 1.5f, 0, 0));
        }
    }
    // Update is called once per frame
    private void FixedUpdate()
    {
        Move();
        Rotate();
    }
}