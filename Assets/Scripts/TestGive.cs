using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGive : MonoBehaviour
{
    public static TestGive instance;


    public GameObject box;
    public GameObject boxpos;
    public GameObject boxpres;  



    private void Update()
    {
        giveItem();
    }



    public void giveItem()
    {
        GameObject item = Instantiate(box, boxpos.transform.position, Quaternion.identity, boxpres.transform);
    }



}
