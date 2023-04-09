using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public Text playerLevel;

    private PlayerController player;



    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();





    }


    void Update()
    {
        ExpUpdate();

    }


    private void ExpUpdate()
    {

        playerLevel.text = player.UserLevel.ToString();
        


    }



}
