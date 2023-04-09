using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Citizen : MonoBehaviour
{

    Rigidbody rig;
    Vector3 vec3;
    

    void Start()
    {
        rig = GetComponent<Rigidbody>();
        
        
    }

    
    void Update()
    {
        rig.angularVelocity = Vector3.zero;
    }
}
