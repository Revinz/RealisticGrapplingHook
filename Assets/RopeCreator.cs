using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeCreator : MonoBehaviour
{
    //Possible materials to be used for the rope simulation
    //Weight is in kg/m3
    public enum Materials {
        cotton = 750, //Fake weight - couldn't find a proper weight
        hemp = 1500,
        aluminum = 2600,
        steel = 7850
    }
    [Header("Rope Properties")]
    public int amountOfJoints = 10;
    public float jointLengthCM = 10;
    public float jointDiameterCM = 4;
    public Materials mat = Materials.cotton;

    [Header("Other - Don't Change")]
    public List<GameObject> joints;
    public GameObject jointPrefab;
    public GameObject hookHeadPrefab;
    private GameObject hookHead;

    void Start() {
        if (!Application.isPlaying) {
            Debug.Log("Is EditMode");
            return;
        }
        
        foreach (Transform child in this.transform)
        {   
            Destroy(child.gameObject);
        }   

               
        hookHead = Instantiate(hookHeadPrefab, this.transform.position, Quaternion.identity);
        hookHead.transform.parent = this.transform;

        joints = new List<GameObject>(); 
        for (int i = 0; i < amountOfJoints; i++) {
            GameObject newJoint = CreateRopeSection(i);
            joints.Add(newJoint);
        }
        
    }
    
    //jointIndex = position in the rope
    GameObject CreateRopeSection(int jointIndex) {
        GameObject joint = GameObject.CreatePrimitive(PrimitiveType.Cylinder);

        joint.transform.parent = this.transform;
        joint.transform.localScale = new Vector3(jointDiameterCM/100, jointLengthCM/100, jointDiameterCM / 100);
            joint.transform.localPosition = new Vector3(0, jointIndex * -((jointLengthCM/100 + 0.01f) * 2), 0); 

        // *** Add ConfigurableJoint and configure it to the correct settings ***
        ConfigurableJoint configJoint = joint.AddComponent<ConfigurableJoint>();
        configJoint.enableCollision = true;

        //Prevent object from spasming and flying away on collisions
        configJoint.projectionMode = JointProjectionMode.PositionAndRotation;
        joint.AddComponent<RopeJointStabilizer>();



        //Connect the joint to the previous joint, if there is one
        Debug.Log(joints.Count);
        if (jointIndex == 0){
            configJoint.connectedBody = hookHead.GetComponent<Rigidbody>();
        }
        if (jointIndex != 0) {
            //Connect the previous body to the joint
            configJoint.connectedBody = joints[jointIndex - 1].GetComponent<Rigidbody>();

        }
        //Lock the motion to ensure the joints follow eachother
        configJoint.xMotion = ConfigurableJointMotion.Locked;
        configJoint.yMotion = ConfigurableJointMotion.Locked;
        configJoint.zMotion = ConfigurableJointMotion.Locked;

        // ** Configure rigidbody settings **
        Rigidbody rb = joint.GetComponent<Rigidbody>();
        rb.mass = CalcWeight();
        rb.drag = 0.05f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode  = CollisionDetectionMode.Continuous;

        return joint;
    }

    float CalcWeight() {
        // weight = mat_weight * Volume = mat_weight * (length * pi * r^2)
        return (int)mat * (jointLengthCM/100 * Mathf.PI * Mathf.Pow(jointDiameterCM / 2 / 100, 2));
    }
}
