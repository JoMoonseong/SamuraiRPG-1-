using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginSystem : MonoBehaviour
{
    public InputField email;
    public InputField password;


    public Text OutPutText;

    void Start()
    {
        FirebaseAuthManager.Instance.Loginstate += OnChangedState;
        FirebaseAuthManager.Instance.Init();


    }

    private void OnChangedState(bool sign)
    {
        OutPutText.text = sign ? "로그인 : " : "로그아웃 : ";
        OutPutText.text = FirebaseAuthManager.Instance.UserID;
    }

    public void Create()
    {
        string e = email.text;
        string p = password.text;

        FirebaseAuthManager.Instance.Create(e, p);
    }

    public void Login()
    {
        FirebaseAuthManager.Instance.Login(email.text, password.text);

    }

    public void LogOut()
    {
        FirebaseAuthManager.Instance.Logout();
    }
}
