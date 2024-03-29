﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Stabilizes the rope joint based on if it is on the ground or in the air.
public class RopeJointStabilizer : MonoBehaviour
{
    LayerMask groundLayer = 1 << 8;
    ConfigurableJoint joint;
    void Start() {
        joint = GetComponent<ConfigurableJoint>();
    }


    // check if the joint is near the ground or not
    // to ensure the joint is stable
    void FixedUpdate()
    {
        if (Physics.OverlapSphere(this.transform.position, 0.4f, groundLayer).Length != 0){
            joint.projectionDistance = 0.2f; //0.2
        }
        else {
            joint.projectionDistance = 0.001f; //0.001
        }
    }
}
