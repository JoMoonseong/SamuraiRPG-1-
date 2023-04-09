using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{


    public static SoundManager instance;



    public AudioSource[] audlist;

    public AudioSource[] BossaudList;

    private void Start()
    {


        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);

        }
        else
        {
            Destroy(gameObject);
        }

    }



    public void atkSnd(string sndName, AudioClip clip)
    {
        GameObject go = new GameObject(sndName + "Sound");
        AudioSource audioSource = go.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.Play();

        Destroy(go, clip.length);
    }





}
