using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMessage : MonoBehaviour
{
    public Text fildText;

    public TextAsset myText;

    public void Start()
    {
        if (myText != null)
        {
            string currentText = myText.text.Substring(0, myText.text.Length - 1);
            fildText.text = currentText;
            Debug.Log(currentText);
        }
    }




}
