using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward_etc : MonoBehaviour
{
    public Light light;
    public ParticleSystem particle;
    CapsuleCollider cap;
    Rigidbody rig;




    void Start()
    {
        rig = GetComponent<Rigidbody>();
        cap = GetComponent<CapsuleCollider>();
        light = GetComponent<Light>();
        particle = GetComponent<ParticleSystem>();
    }


    void Update()
    {
        transform.Rotate(Vector3.forward * 20 * Time.deltaTime);
    }
}
