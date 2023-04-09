using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Potal : MonoBehaviour
{
    public GameObject Potal1;
    public GameObject Potal2;

    public GameObject BossParent;
    public GameObject Bosshm;
    public GameObject bossSpwPos;





    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Potal1 = other.gameObject;
            
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
     
        {
            StartCoroutine(Teleport());
        }
    }

    IEnumerator Teleport()
    {
        yield return null;
        GameObject Boss = Instantiate(Bosshm, bossSpwPos.transform.position, Quaternion.identity, BossParent.transform);
        Potal1.transform.position = Potal2.transform.position;


    }



}


