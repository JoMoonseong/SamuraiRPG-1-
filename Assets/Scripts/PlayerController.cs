using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public TalkManager talk;

    public float maxhealth = 100;
    public float curhealth = 100;
    public float curExp = 0;
    public float maxExp = 100;



    public int UserLevel = 1;

    public float Speed;

    float h;
    float v;

    bool wDown;
    bool GDown;

    bool isDodge;
    bool jDown;
    bool spDown;
    bool fDown;
    bool isJump;
    bool isBuff;
    bool bDown;
    bool tDown;
    bool isfireReady;

    bool isDead;
    bool isDamage;

    bool isAttack = false;

    float buffCooldown;


    Rigidbody rig;
    Animator ani;


    public GameManager gm;

    Vector3 doDodgeVec;
    Vector3 moveVec;

    float fireCooltime;

    public ParticleSystem LevelEffect;
    public Light LevelupLight;

    

    public ParticleSystem buffect;

    public Light bufflight;

    public TrailRenderer dodgeEffect;

    SoundManager smg;

    public AudioClip clip;

    [SerializeField]
    private Slider hpbar;
    [SerializeField]
    private Slider Expbar;

    private EnemyWeapon ew;

    private EnemyController en;

    [SerializeField]
    Weapon weaponEtc;


    public Image BloodScreen;

    public Camera FollowCamera;



    void Start()
    {
        rig = GetComponent<Rigidbody>();
        ani = GetComponent<Animator>();

        hpbar.value = (float)curhealth / (float)maxhealth;
        Expbar.value = (float)curExp / (float)maxExp;

        

    }

    void Update()
    {
        Action();
        Move();
        Dodge();
        Attack();
        Guard();
        Buff();
        HandleHp();
        PlayerHit();
        Jump();
        Look();
        FootStepSnd();
        LevelUp();


    }


    void Action()
    {
        h = Input.GetAxisRaw("Horizontal");
        v = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetMouseButtonDown(0);
        GDown = Input.GetMouseButtonDown(1);
        spDown = Input.GetMouseButtonDown(2);
        bDown = Input.GetButtonDown("Buff");
        tDown = Input.GetButtonDown("Talkto");
    }

    void Jump()
    {
        if (jDown && moveVec == Vector3.zero && !isJump && !isDodge && !isDead)
        {
            rig.AddForce(Vector3.up * 10, ForceMode.Impulse);
            ani.SetBool("isJump", true);
            ani.SetTrigger("doJump");
            isJump = true;
        }
             
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge && !isDead)
        {
            Speed *= 2;
            ani.SetTrigger("doDodge");
            isDodge = true;


            Invoke("DodgeOut", 0.5f);
        }
    }
    void DodgeOut()
    {
        isDodge = false;
        Speed *= 0.5f;
        dodgeEffect.enabled = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            ani.SetBool("isJump", false);
            isJump = false;
        }
    }



    void Move()
    {

            moveVec = Vector3.zero;


        moveVec = new Vector3(h, 0, v).normalized;

        transform.position += moveVec * Speed * (wDown ? 0.3f : 1f) * Time.deltaTime;

         ani.SetBool("isRun", moveVec != Vector3.zero);
        ani.SetBool("isWalk", wDown);

        transform.LookAt(transform.position + moveVec);

        rig.angularVelocity = Vector3.zero;

  
   
    }

    void Look()
    {
        if (fDown && !isDead)
        {
            Ray ray = FollowCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayhit;
            if (Physics.Raycast(ray, out rayhit, 100))
            {
                Vector3 nextVector = rayhit.point - transform.position;
                nextVector.y = 0;
                transform.LookAt(transform.position + nextVector);
            }
        }
    }


    private void HandleHp()
    {
        hpbar.value = Mathf.Lerp(hpbar.value, (float)curhealth / (float)maxhealth, Time.deltaTime * 10);
        Expbar.value = Mathf.Lerp(Expbar.value, (float)curExp / (float)maxExp, Time.deltaTime * 10);
    }


    void Attack()
    {
        fireCooltime += Time.deltaTime;
        isfireReady = weaponEtc.rate < fireCooltime;

        if (fDown && isfireReady && !isDodge && !isJump && moveVec == Vector3.zero && !isDead && !tDown)
        {
            isAttack = true;
            weaponEtc.Use();
            ani.SetTrigger("isAttack");
            fireCooltime = 0;
            SoundManager.instance.audlist[6].Play();
        }
    }

     void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ShakeCamera.Instance.OnShakeCamera(0.1f, 0.5f);

            other.GetComponent<EnemyController>().curHealth -= weaponEtc.damage;
        }
        else if (other.CompareTag("EnemyWeapon"))
        {
            if (!isDamage)
            {
                int rand = Random.RandomRange(0, 30);
                curhealth -= rand;
                ani.SetTrigger("doHit");
                SoundManager.instance.audlist[3].Play();
                StartCoroutine(ShowBloodScreen());
                StartCoroutine(OnDamage());
            }
        }
        else if (other.CompareTag("Gold"))
        {
            SoundManager.instance.audlist[5].Stop();
                Debug.Log("씬무브!"); 
                SceneManager.LoadScene(3);
        }
    }





    void FootStepSnd()
    {
        if (moveVec == Vector3.zero)
        {
            SoundManager.instance.audlist[5].Play();
        }
    }

    void Guard()
    {
        if (GDown && moveVec == Vector3.zero && !isJump && !isDodge && !isDead)
        {
            isAttack = true;
            weaponEtc.DefenseMode();
            ani.SetTrigger("OnCombo");
            fireCooltime = 0;
            SoundManager.instance.audlist[6].Play();  
        }
    }

    public void Buff()
    {
        if (bDown && !isDead)
        {
            ani.SetTrigger("doBuff");
            StopCoroutine("buff");
            StartCoroutine("buff");
            isBuff = true;
            ShakeCamera.Instance.OnShakeCamera(0.5f, 0.1f);

            SoundManager.instance.audlist[0].Play();
            buffCooldown += Time.deltaTime;
        }
        else if(buffCooldown < 10)
        {
            isBuff = false;

            if (buffCooldown <= 10)
            {
                isBuff = true;
            }
        }
    }



    void LevelUp()
    {
        if (curExp >= maxExp)
        {
            curExp = 0;
            UserLevel++;
            curhealth = 100;
            weaponEtc.damage++;
            //leve_txt.text = " "+ level;
            StartCoroutine(LevelUpInfo());
        }
    }




    void PlayerHit()
    {
        if(Input.GetKeyDown(KeyCode.K))
            if (curhealth > 0)
            {
                curhealth -= ew.EnemyDamage;
            }
            else
            {
                curhealth = 0;
            }
    }

    IEnumerator buff()
    {
        yield return new WaitForSeconds(0.1f);
        buffect.Play();
        bufflight.enabled = true;
        weaponEtc.damage += 20;
        

        yield return new WaitForSeconds(10f);
        buffect.Stop();
        bufflight.enabled = false;
        weaponEtc.damage -= 20;

       
    }

    IEnumerator ShowBloodScreen()
    {
        BloodScreen.color = new Color(1, 0, 0, UnityEngine.Random.RandomRange(0.2f, 0.3f));
        yield return new WaitForSeconds(0.1f);
        BloodScreen.color = Color.clear;
    }



    public float GetHealth()
    {
        return curhealth;
    }


    public void Ondamaged(float damage)
    {
        curhealth -= damage;

    }


    IEnumerator OnDamage()
    {
        if (curhealth <= 0 && !isDead)
        {
            OnDeath();
        }

        isDamage = true;

        yield return new WaitForSeconds(1f);

        isDamage = false;


    }


    void OnDeath()
    {
        ani.SetTrigger("doDeath");
        isDead = true;
        gm.GameOver();

    }

    IEnumerator LevelUpInfo()
    {
        yield return new WaitForSeconds(0.2f);
        LevelEffect.Play();
        LevelupLight.enabled = true;
        SoundManager.instance.audlist[7].Play();
        yield return new WaitForSeconds(0.1f);
        LevelEffect.Stop();
        LevelupLight.enabled = false;

    }



}



