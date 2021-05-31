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

    [Header("Swining Properties")]
    public GameObject rotCenter;
    public int swingingPoint = 6;
    public int swingingPower = 50;
    public float swingingDecreaseMultiplierPerJoint = 0.8f;
    private bool finishedFirstTimeSetup = false;
    private Vector3? previousLoc = null; //vector3? = nullable vector3
    private bool holdingRope = true;
    private bool swingRope = false;
    public float speed = 0.0001f;

    [Header("Other - Don't Change")]
    public List<GameObject> joints;
    public GameObject jointPrefab;
    public GameObject hookHeadPrefab;
    private GameObject hookHead;

    void Awake() {
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

        this.gameObject.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));

        SetupSwinging();
        finishedFirstTimeSetup = true;
    }

    private void OnValidate()
    {

        if (Application.isPlaying && finishedFirstTimeSetup)
            SetupSwinging();
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

        // Add and configure Rotation script
        rot rotScript = joint.AddComponent<rot>();
        rotScript.center = rotCenter.transform;

        return joint;
    }

    void SetupSwinging()
    {
        //First reset everything
        for (int i = 0; i < joints.Count; i++)
        {

            setJointNonKinematic(joints[i]);

        }

        setJointKinematic(joints[swingingPoint]);  
    }

    void setJointKinematic(GameObject joint)
    {
        var rb = joint.GetComponent<Rigidbody>();
        var rotScript = joint.GetComponent<rot>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = true;

        for (int i = swingingPoint - 1; i >= 0; i--)
        {
            joints[i].GetComponent<rot>().magnitudeOfForce = - swingingPower * Mathf.Pow(swingingDecreaseMultiplierPerJoint, i);
            joints[i].GetComponent<rot>().center = rotScript.center;
        }

        if (previousLoc != null)
            joint.transform.position = (Vector3)previousLoc;

        joint.transform.rotation = Quaternion.Euler(new Vector3(-90, 0, 0));
        previousLoc = rb.transform.position;
        rotScript.center.position = rotScript.gameObject.transform.position + rb.gameObject.transform.up * jointLengthCM / 100;
    }

    void setJointNonKinematic(GameObject joint)
    {
        var rb = joint.GetComponent<Rigidbody>();
        var rotScript = joint.GetComponent<rot>();
        rotScript.magnitudeOfForce = 0;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.isKinematic = false;
    }


    float CalcWeight() {
        // weight = mat_weight * Volume = mat_weight * (length * pi * r^2)
        return (int)mat * (jointLengthCM/100 * Mathf.PI * Mathf.Pow(jointDiameterCM / 2 / 100, 2));
    }




    void Update()
    {
        if (holdingRope)
        {
            if (Input.GetKeyDown("space"))
            {
                swingRope = true;
            }
            if (Input.GetKeyUp("space"))
            {   
                swingRope = false;
                swingingPower = 0;
                // Debug.Log("release rope");
                setJointNonKinematic(joints[swingingPoint]);
                holdingRope = false;
            }
            if (swingingPower < 300 && swingRope)
            {

                if (swingingPower + (int)(200 * Time.deltaTime) > 300)
                {
                    swingingPower = 300;
                }
                else
                {
                    swingingPower += (int)(200 * Time.deltaTime);
                }
            }
            
            if (Input.GetKeyDown("left")&&(swingingPoint >= 2))
            {
                swingingPoint -= 1;
                SetupSwinging();
            }
            if (Input.GetKeyDown("right") && (swingingPoint < amountOfJoints-1))
            {
                swingingPoint += 1;
                SetupSwinging();
            }
            if (Input.GetKey("up"))
            {
                joints[swingingPoint].transform.position += Vector3.up * speed * Time.deltaTime;
                previousLoc = joints[swingingPoint].transform.position;
            }
            if (Input.GetKey("down"))
            {
                joints[swingingPoint].transform.position += Vector3.up * (-speed) * Time.deltaTime;
                previousLoc = joints[swingingPoint].transform.position;
            }

            UpdatePower();
        }

    }

    void UpdatePower()
    {
        for (int i = swingingPoint - 1; i >= 0; i--)
        {
            joints[i].GetComponent<rot>().magnitudeOfForce = - swingingPower * Mathf.Pow(swingingDecreaseMultiplierPerJoint, i);
        }
    }

   
}
