using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rot : MonoBehaviour
{
    //https://forum.unity.com/threads/circular-movement-using-forces-script.746453/
    public bool moving = false;
    public float initialDistance;
    public Rigidbody rigidbodyOfSection;
    private Vector3 distance;
    public Transform center;
    private Vector3 normalizedDistance;
    private Vector3 perpendicularDistance;
    public float magnitudeOfForce;
    // Start is called before the first frame update
    void Start()
    {
        rigidbodyOfSection = GetComponent<Rigidbody>();
        distance = center.position - transform.position;

        moving = !moving;
    }

    void Update()
    {
       
            initialDistance = distance.magnitude;
        
        if (moving)
        {
            //We desactivate the gravity so it doesn't affect the movement
            //rigidbodyOfSection.useGravity = false;

            //Refresh the distance vector every second to get the proper normalized vector(normalizedDistance) below
            distance = center.position - transform.position;
            normalizedDistance = distance.normalized;

            //We get the perpendicular vector to normalizedDistance, this vector will be in every frame tangent to the circumference that the object
            //attached to this script will travel
            perpendicularDistance = new Vector3(normalizedDistance.y, -normalizedDistance.x, 0);

            //We modify the velocity component of our object matching it with the tangent vector, and multiplied by the distance to make the
            //circumference radio equal to the initial distance they were before starting the movement
            //rigidbodyOfSection.velocity = perpendicularDistance * initialDistance * magnitudeOfForce;
            rigidbodyOfSection.AddForce(perpendicularDistance * magnitudeOfForce);
        }
    }
}
 

