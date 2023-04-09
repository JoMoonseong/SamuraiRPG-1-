using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public GameObject healthbar;


    private RectTransform rect;

    private EnemyController enemy;

    private float maxWidth = 0.5f;
    void Start()
    {
        enemy = GetComponent<EnemyController>();
        rect = healthbar.GetComponent<RectTransform>();
    }


    void Update()
    {
        EnemyHP();
    }

    void EnemyHP()
    {
        if (enemy.Maxhealth != enemy.GetHealth())
        {
            float nowWidth = maxWidth * (enemy.GetHealth() / enemy.Maxhealth);

            Vector2 size = rect.sizeDelta;
            rect.sizeDelta = new Vector2(nowWidth, size.y);


        }
    }
}
