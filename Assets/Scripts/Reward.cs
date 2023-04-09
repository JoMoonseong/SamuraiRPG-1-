using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reward : MonoBehaviour
{
    EnemyController enemy;

    public Transform Treaser;

    public GameObject Coin;

    private int count = 0;

    public Transform RewardPos;

    public Transform RewardPre;

     void Awake()
    {
        //Treaser = GameObject.Find("RewardBox").GetComponent<Transform>();
        RewardPos = GameObject.Find("RewardPosi").GetComponent<Transform>();
        RewardPre = GameObject.Find("RewardPres").GetComponent<Transform>();
    }

    void Start()
    {
        enemy = GetComponent<EnemyController>();

    }

    void Update()
    {

        if (enemy.curHealth < 0)
        {
            count ++;  
            if (count == 1)
            {
              Instantiate(Coin, RewardPos.transform.position, Quaternion.identity, RewardPre.transform);
                Debug.Log("보상이다!");  
            }
            

            

        }


    }
}
    
