using UnityEngine;

public class TriggerHealth : MonoBehaviour
{
    private ScreenDamage script;

    void Start()
    {   //get the main screen damage script
        script = GetComponent<ScreenDamage>();
    }

    //decrease health method
    public void DecreaseHealth(){
        script.CurrentHealth -= 10f;
    }

    //increase health
    public void IncreaseHealth(){
        script.CurrentHealth += 10f;
    }
}
