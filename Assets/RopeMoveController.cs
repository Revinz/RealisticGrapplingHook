using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeMoveController : MonoBehaviour
{
    public GameObject[] ropeJoints;
    public GameObject selectedJoint;
    public float speed = 1f;
    void FixedUpdate()
    {
        if (!selectedJoint)
            return;
            
        Vector3 moveDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0) * Time.deltaTime * speed; 
        selectedJoint.GetComponent<Rigidbody>().AddForce(moveDir * speed, ForceMode.Force);
    }
}
