using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type {Melee};

    public SphereCollider Area;

    public int damage;

    public TrailRenderer effect;

    public new Light light;

    public Type type;

    public float rate;

    public BoxCollider DpArea;


    public BoxCollider SpacialArea;

    public void Use()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("AttackImfor");
            StartCoroutine("AttackImfor");
        }
    }


    

    IEnumerator AttackImfor()
    {
        yield return new WaitForSeconds(0.1f);
        Area.enabled = true;
        effect.enabled = true;
        light.enabled = true;


        yield return new WaitForSeconds(0.4f);  
        Area.enabled = false;


        yield return new WaitForSeconds(0.3f);
        effect.enabled = false;
        light.enabled = false;
    }


    public void DefenseMode()
    {
        if (type == Type.Melee)
        {
            StopCoroutine("Defense");
            StartCoroutine("Defense");
        }
    }

    IEnumerator Defense()
    {
        yield return new WaitForSeconds(0.1f);
        Area.enabled = true;
        effect.enabled = true;
        light.enabled = true;


        yield return new WaitForSeconds(0.7f);
        Area.enabled = false;


        yield return new WaitForSeconds(0.3f);
        effect.enabled = false;
        light.enabled = false;


    }
}
