using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TalkManager : MonoBehaviour
{
    public GameObject Talkpanel;
    public Text TalkText;
    public GameObject PlayerController;

    public bool isTalk;
    public void Scan(GameObject player)
    {
        if (isTalk)
        {
            isTalk = false;
        }
        else
        {
            isTalk = true;
            PlayerController = player;
            TalkText.text = player.name;


            Talkpanel.SetActive(isTalk);
        }
    }
}
