using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NewEnemyContorller : MonoBehaviour
{
    private float health = 100f;

    [SerializeField]
    private float AttackRange = 1f;

    [SerializeField]
    private float AttackDamage = 15f;


    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float attackspeed = 1.5f;

    private float attackCooldown = 0f;

    private Animator ani;
    private NavMeshAgent nav;

    private GameObject player;
    private Rigidbody rig;

    private SkinnedMeshRenderer smr;

    void Start()
    {
        ani = GetComponent<Animator>();
        nav = GetComponent<NavMeshAgent>();
        rig = GetComponent<Rigidbody>();
        smr = GetComponentInChildren<SkinnedMeshRenderer>();

        player = GameObject.FindGameObjectWithTag("Player");
    }


    void Update()
    {
        if (health > 0)
        {
            nav.enabled = true;
            nav.isStopped = false;
            nav.speed = speed;


            if (player != null)
            {
                PlayerController playerctrl = player.GetComponent<PlayerController>();
                nav.SetDestination(player.transform.position);

                ani.SetBool("Run", true);

                if (playerctrl.GetHealth() > 0)
                {
                    float dist = Vector3.Distance(player.transform.position, transform.position);

                    if (dist <= AttackRange)
                    {
                        if (attackCooldown >= attackspeed)
                        {
                            attackCooldown = 0;

                            ani.SetTrigger("Attack");
                            playerctrl.Ondamaged(AttackDamage);
                            //플레이어가 공격을 받았을때 처리할 내용 작성

                        }

                        else
                        {
                            attackCooldown += Time.deltaTime;
                        }

                    }
                    else
                    {
                        //몬스터가 더 이상 플레이어를 공격하지 않는 로직
                    }
                }
            }
            else
            {
                nav.SetDestination(Vector3.zero);

                nav.isStopped = true;
                nav.enabled = false;
            }
        }
        else
        {
            nav.SetDestination(Vector3.zero);

            nav.isStopped = true;
            nav.enabled = false;

            ani.SetTrigger("Die");
        }

        rig.velocity = Vector3.zero;
        rig.angularVelocity = Vector3.zero;
    }
}
