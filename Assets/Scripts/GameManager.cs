using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [Header("몬스터 스폰 관리")]
    [SerializeField]
    private GameObject[] spawnPoints = new GameObject[3];

    [SerializeField]
    private GameObject EnemyPrefab;

    [SerializeField]
    GameObject Gate;



    [SerializeField]
    private GameObject EnemyParent;

    [SerializeField]
    private float maxspwanCoolDown = 2f;

    [SerializeField]
    private int maxSpawnCount = 10;

    private float nowSpawnCoolDown = 0f;

    [SerializeField]
    private int nowSpawnCount = 0;

    void Start()
    {
        
    }


    void Update()
    {
        EnemySpawn();
    }


    private void EnemySpawn()
    {
        if (nowSpawnCount < maxSpawnCount)
        {
            if (nowSpawnCoolDown >= maxspwanCoolDown)
            {
                nowSpawnCoolDown = 0f;


                int random = Random.Range(0, spawnPoints.Length);

                GameObject Enemy1 = Instantiate(EnemyPrefab, spawnPoints[random].transform.position, Quaternion.identity, EnemyParent.transform);
                nowSpawnCount++;
            }
            else
            {
                nowSpawnCoolDown += Time.deltaTime;
            }
            
            if (nowSpawnCount == maxSpawnCount)
            {
                
                Destroy(Gate);
            }
        }
    }


    public void GameOver()
    {
        SceneManager.LoadScene(2);
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }
}
