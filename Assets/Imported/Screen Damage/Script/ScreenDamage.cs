using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenDamage : MonoBehaviour
{
    public float maxHealth = 100f;                                      //maximum health - used for calculations (THIS IS NOT YOUR CURRENT HEALTH)
    public Image bloodyFrame;                                           //get the bloody frame image gameobject
    public float criticalHealth = 10f;                                  //the amount of health in which pulsating effect starts
    
    public bool useBlurEffect = true;                                   //if enabled will use blur effect on damage
    public Image blurImage;                                             //the blur image
    [Range(0f, 5f)] public float blurDuration = 0.1f;                   //the duration of the hit blur effect
    public float blurFadeSpeed = 5f;                                    //the speed of the fading

    public AudioSource pulseSound;                                      //pulse sound audio source
    public bool fadeAudios = true;                                      //fade all the world audios on near death
    public List<AudioSource> audiosToFade = new List<AudioSource>();    //the audio sources to fade
    public float audiosFadeVolume = 0.3f;                               //set the volume of audio fade
    public float healingSpeed = 20f;                                    //the time it takes to heal back to maximum health
    public bool autoHeal = true;                                        //turn on/off auto healing
    public float autoHealTime = 2f;                                     //time for the auto-heal to kick in

    float _health;                                                      //property modifier
    float opacity = 0f;                                                 //opacity value
    Animator animator;                                                  //get animator
    bool shouldHeal = false;                                            //should start healing flag
    float lastHealth = 100f;                                            //save the last health amount
    bool pulseSoundOn = false;                                          //flag to fade the sound in
    bool pulseSoundOff = false;                                         //flag to fade the sound out
    Dictionary<AudioSource, float> fadedAudiosCopy;                     //save references and volumes of audio sources
    int currentVolumeIndex = 0;                                         //save the current loop index
    bool showingBlur = false;                                           //flag that the show hit blur coroutine is enabled
    bool triggerBlur = false;
    bool hideBlur = false;

    
    public float CurrentHealth {                                        //property for setting/getting health
        get { return _health; }

        set {
            //save the last health
            lastHealth = _health;

            //set the new health
            _health = value;
            
            //show radial blur effect
            if (_health < lastHealth && useBlurEffect && blurImage != null && _health > 0f) {
                blurImage.enabled = true;
                hideBlur = false;
                triggerBlur = true;
            }

            bloodyFrame.enabled = true;

            //if health set to something less than 0 - DEAD
            if (_health <= 0f) {
                if (flagHealingCoroutine != null) {
                    StopCoroutine(flagHealingCoroutine);
                }

                shouldHeal = false;
                _health = 0f;
                animator.enabled = false;
                if (pulseSound != null) pulseSoundOff = true;

                //change blood frame color to black
                var temp = bloodyFrame.color;
                temp = new Color(0, 0, 0, 255);
                bloodyFrame.color = temp;

                return;
            
            }

            //turn on/off pulsating effect
            if(_health <= criticalHealth && (_health > 0f)){
                animator.enabled = true;
                //play pulse sound if not playing
                if (pulseSound != null) {
                    if(!pulseSound.isPlaying){
                        pulseSoundOff = false;
                        pulseSound.Play();
                        pulseSoundOn = true;
                    }
                }
            }else{
                if (pulseSound != null) {
                    pulseSoundOn = false;
                    pulseSoundOff = true;
                }
                animator.enabled = false;
            }

            //if setting current health to something more than the max health
            //reset health to max health
            if (_health >= maxHealth) {
                _health = maxHealth;
                shouldHeal = false;
                bloodyFrame.enabled = false;
            }

            //calculate the opacity
            opacity = 1f - (_health / maxHealth);

            //change image opacity value
            var tempColor = bloodyFrame.color;
            tempColor = new Color(255, 255, 255, opacity);
            bloodyFrame.color = tempColor;

            //trigger auto-healing
            if (autoHeal && (_health < maxHealth) && !shouldHeal) {
                if (flagHealingCoroutine != null) StopCoroutine(flagHealingCoroutine);
                flagHealingCoroutine = StartCoroutine(FlagHealing());
                return;
            }

            //if hit during auto-heal
            //restart auto-heal timer
            if (shouldHeal && (lastHealth > _health)) {
                if (flagHealingCoroutine != null) StopCoroutine(flagHealingCoroutine);
                shouldHeal = false;
                flagHealingCoroutine = StartCoroutine(FlagHealing());
            }
        }
    }

    void Start() 
    {
        animator = bloodyFrame.transform.GetComponent<Animator>();

        //set current health to maximum health on start
        CurrentHealth = maxHealth;

        //set the volume to 0, for better fading experience
        if (pulseSound != null) pulseSound.volume = 0f;
        if (fadeAudios && audiosToFade.Count > 0) fadedAudiosCopy = new Dictionary<AudioSource, float>();
    }

    //flag that healing should occur
    Coroutine flagHealingCoroutine;
    IEnumerator FlagHealing()
    {
        yield return new WaitForSeconds(autoHealTime);
        shouldHeal = true;
    }

    //method responsible for healing overtime
    void Heal()
    {
        if (shouldHeal) {
            CurrentHealth += Time.deltaTime * healingSpeed;
            if (CurrentHealth >= maxHealth) {
                shouldHeal = false;
            }
        }
    }

    //decrease volume of world audio sources
    void DecreaseVolumes()
    { 
        foreach (AudioSource audio in audiosToFade)
        {
            //only decrease volume of sounds that aren't the main pulse sound
            //and audios that are actually playing
            if ((audio != pulseSound) && audio != null) {
                if (audio.isPlaying) {

                    //save the original audio reference
                    if (!fadedAudiosCopy.ContainsKey(audio)) fadedAudiosCopy.Add(audio, audio.volume);
                    
                    if (audio.volume > audiosFadeVolume) {
                        audio.volume -= Time.deltaTime * 1f;
                        if (audio.volume <= audiosFadeVolume) {
                            audio.volume = audiosFadeVolume;
                        }
                    }else{
                        audio.volume += Time.deltaTime * 1f;
                        if (audio.volume >= audiosFadeVolume) {
                            audio.volume = audiosFadeVolume;
                        }
                    }
                }
            } 
        }
    }

    //increase volume of world audio sources
    void IncreaseVolumes()
    {
        foreach (AudioSource audio in audiosToFade)
        {
            currentVolumeIndex++;
            //only decrease volume of sounds that aren't the main pulse sound
            if (audio != pulseSound && audio != null) {
                if (fadedAudiosCopy.ContainsKey(audio)) {
                    if (audio.volume < fadedAudiosCopy[audio]) {
                        audio.volume += Time.deltaTime * 1f;
                        if (audio.volume >= fadedAudiosCopy[audio]) {
                            audio.volume = fadedAudiosCopy[audio];
                        }
                    }else{
                        audio.volume -= Time.deltaTime * 1f;
                        if (audio.volume <= fadedAudiosCopy[audio]) {
                            audio.volume = fadedAudiosCopy[audio];
                        }
                    }
                }
            }

            if (currentVolumeIndex == fadedAudiosCopy.Count) {
                fadedAudiosCopy.Clear();
                currentVolumeIndex = 0;
            }
        }
    }

    void Update()
    {
        Heal();
        
        //fade the pulse sound in
        if (pulseSoundOn) {
            
            pulseSound.volume += Time.deltaTime * 1f;
            if (pulseSound.volume >= 1f) {
                pulseSoundOn = false;
            }

            //if fading is checked then fade out audios
            if (fadeAudios) DecreaseVolumes();
        }

        //fade the pulse sound out
        if (pulseSoundOff) {
            
            pulseSound.volume -= Time.deltaTime * 1f;
            if (pulseSound.volume <= 0f) {
                pulseSoundOff = false;
                pulseSound.Stop();
            }

            //if fading is checked then fade in audios
            if (fadeAudios && _health > 0f) IncreaseVolumes();
        }

        //fade in blur
        if (triggerBlur) {
            
            var tempColor = blurImage.color;
            tempColor.a += Time.deltaTime * blurFadeSpeed;
            blurImage.color = tempColor;

            if (tempColor.a >= 1.9f) {
                triggerBlur = false;
                showRadialBlurCoroutine = StartCoroutine(ShowRadialBlur());
            }
        }
        
        //fade out blur
        if (hideBlur) {
            var tempColor = blurImage.color;
            tempColor.a -= Time.deltaTime * blurFadeSpeed;
            blurImage.color = tempColor;

            if (tempColor.a <= 0f) {
                hideBlur = false;
                blurImage.enabled = false;
            }
        }
    }

    //show the radial blur coroutine
    Coroutine showRadialBlurCoroutine;
    IEnumerator ShowRadialBlur()
    {
        if (showingBlur) yield break;

        showingBlur = true;
        blurImage.enabled = true;
        
        yield return new WaitForSeconds(blurDuration);
        
        showingBlur = false;
        hideBlur = true;
    }
}
