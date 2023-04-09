using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Skills : MonoBehaviour
{
    public Image skillFilter;
    PlayerController pcl;

    public float coolTime;


     void Update()
    {
        if (Input.GetButtonDown("Buff"))
        {
            StartCoroutine(CoolTime(10f));
        }
    }

    IEnumerator CoolTime(float cool)
    {
        while (cool > 1.0f)
        {
            cool -= Time.deltaTime;
            skillFilter.fillAmount = (1.0f / cool);
            yield return new WaitForFixedUpdate();
        }
    }
 
}
