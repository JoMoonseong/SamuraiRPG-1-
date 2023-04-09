using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class NPCTalk : MonoBehaviour
{
    public GameObject DialoguePanel;
    public Text DialoguePText;
    public string[] Dialogue;
    private int index;

    public GameObject contBtn;
    public float wordSpeed;
    public bool PlayerClose;





    void Update()
    {
        if (Input.GetButtonDown("Talkto"))
        {
            if (DialoguePanel.activeInHierarchy)
            {
                
                
                
            }
            else
            {
                DialoguePanel.SetActive(true);
                StartCoroutine(Typing());
            }
        }

        if (DialoguePText.text == Dialogue[index])
        {
            contBtn.SetActive(true);
        }


        KeyNextLine();  
    }


    public void ZeroText()
    {
        DialoguePText.text = "";
        index = 0;
        DialoguePanel.SetActive(false);
    }
    
    public void NextLine()
    {
        contBtn.SetActive(false);

        if (index < Dialogue.Length - 1)
        {
                index++;
                DialoguePText.text = "";
                StartCoroutine(Typing());
        }
            else
            {
                ZeroText();
            }
        
    }

    public void KeyNextLine()
    {
        contBtn.SetActive(false);  

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (index < Dialogue.Length - 1)
            {
                index++;
                DialoguePText.text = "";
                StartCoroutine(Typing());
            }
            else
            {
                ZeroText();
            }
        }
    }

    IEnumerator Typing()
    {

        foreach (char letter in Dialogue[index].ToCharArray())
            {
                DialoguePText.text += letter;
                yield return new WaitForSeconds(wordSpeed);
            }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) {
            PlayerClose = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerClose = false;
        }
    }


    
}
