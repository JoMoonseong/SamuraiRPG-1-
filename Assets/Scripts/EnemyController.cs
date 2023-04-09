using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Net.NetworkInformation;
using System;

public class EnemyController : MonoBehaviour
{
    public static EnemyController instance;

    public float Maxhealth;
    public float curHealth;



    public Transform target;
    public enum Type {Melee,Boss};
    public Type EnemyType;

    public float RewardExp = 25;

    public bool isTracking;

    public int DeadCount = 0;

    public bool isSTattack;

    public BoxCollider atArea;

    public ParticleSystem hitffect;



    Rigidbody rig;

    Material mat;

    NavMeshAgent nav;

    Animator ani;

    PlayerController PlayerCt;

    BoxCollider box;

    

     void Awake()
    {
        target = GameObject.Find("Player").GetComponent<Transform>();

    }


    void Start()
    {
        rig = GetComponent<Rigidbody>();
        box = GetComponent<BoxCollider>();  
        mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();



          Invoke("TrackingStart", 2);

        curHealth = Maxhealth;

    }


    void TrackingStart()
    {  
        isTracking = true;
        ani.SetBool("isMove", true);
    }

     void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isTracking;
        }

    }


    void VelocityRotation()
    {
        if (isTracking)
        {
            rig.velocity = Vector3.zero;
            rig.angularVelocity = Vector3.zero;
        }

    }

    void Targeting()
    {
            float targetRadius = 0f;
            float targetRange = 0f;

            switch (EnemyType)
            {
                case Type.Melee:
                    targetRadius = 0.5f;
                    targetRange = 1.5f;
                    break;
            }
            RaycastHit[] rayhit =
                Physics.SphereCastAll(transform.position, 
                                        targetRadius,
                                        transform.forward,
                                        targetRange, 
                                        LayerMask.GetMask("Player"));

            if (rayhit.Length > 0 && !isSTattack)
            {
                StartCoroutine("Attack");
            }

    }

    IEnumerator Attack()
    {
        isTracking = false;
        isSTattack = true;
        ani.SetBool("isAttack", true);
        SoundManager.instance.BossaudList[0].Play();


        yield return new WaitForSeconds(1f);
        atArea.enabled = true;

        yield return new WaitForSeconds(0.3f);
        atArea.enabled = false;

          
        yield return new WaitForSeconds(0.5f);
        isTracking = true;
        isSTattack = false;
        ani.SetBool("isAttack", false);
    }



     void FixedUpdate()
    {
        Targeting();
        VelocityRotation();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Weapon")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            SoundManager.instance.audlist[2].Play();
            Vector3 KnockBack = transform.position - other.transform.position;

            StartCoroutine(OnDamage(KnockBack));
            SoundManager.instance.audlist[4].Play();
        }


    }

    IEnumerator OnDamage(Vector3 KnockBack)
    {
        mat.color = Color.red;

        ani.SetTrigger("doHit");
        hitffect.Play();
        yield return new WaitForSeconds(0.1f);
        if (curHealth > 0)
        {
                mat.color = Color.white;

            
        }

        else 
        {
               mat.color = Color.gray;
            gameObject.layer = 7;
            isTracking = false;
            nav.enabled = false;
            ani.SetTrigger("doDie");
            
                KnockBack = KnockBack.normalized;
                KnockBack += Vector3.up;
                rig.AddForce(KnockBack * 3, ForceMode.Impulse);
            PlayerController player = target.GetComponent<PlayerController>();
            player.curExp += RewardExp;

            DeadCount++;

            Destroy(gameObject, 2);

        }

    }  

    public float GetHealth()
    {
        return curHealth;
    }


}
