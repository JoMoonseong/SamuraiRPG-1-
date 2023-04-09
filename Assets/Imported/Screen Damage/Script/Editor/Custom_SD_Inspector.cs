using UnityEngine;
using UnityEditor;

[CanEditMultipleObjects]
[CustomEditor(typeof(ScreenDamage))]

public class Custom_SD_Inspector : Editor
{
    SerializedProperty maxHealth,
    bloodyFrame,
    criticalHealth,
    useBlurEffect,
    blurImage,
    blurDuration,
    blurFadeSpeed,
    healingSpeed,
    autoHeal,
    autoHealTime,
    pulseSound,
    fadeAudios,
    audiosToFade,
    audiosFadeVolume;

    void OnEnable() {
        maxHealth = serializedObject.FindProperty("maxHealth");
        bloodyFrame = serializedObject.FindProperty("bloodyFrame");
        criticalHealth = serializedObject.FindProperty("criticalHealth");
        useBlurEffect = serializedObject.FindProperty("useBlurEffect");
        blurImage = serializedObject.FindProperty("blurImage");
        blurDuration = serializedObject.FindProperty("blurDuration");
        blurFadeSpeed = serializedObject.FindProperty("blurFadeSpeed");
        healingSpeed = serializedObject.FindProperty("healingSpeed");
        autoHeal = serializedObject.FindProperty("autoHeal");
        autoHealTime = serializedObject.FindProperty("autoHealTime");
        pulseSound = serializedObject.FindProperty("pulseSound");
        fadeAudios = serializedObject.FindProperty("fadeAudios");
        audiosToFade = serializedObject.FindProperty("audiosToFade");
        audiosFadeVolume = serializedObject.FindProperty("audiosFadeVolume");
    }

    public override void OnInspectorGUI() {

        ScreenDamage script = (ScreenDamage)target;

        var button = GUILayout.Button("Click for more tools");
        if (button) Application.OpenURL("https://assetstore.unity.com/publishers/39163");
        EditorGUILayout.Space(5);

        EditorGUILayout.LabelField("Health", EditorStyles.boldLabel);
        //Bloody Frame
        EditorGUILayout.PropertyField(bloodyFrame, new GUIContent("Bloody Frame", "Drag and drop the image game object which contains the bloody frame."));

        //Max Health
        EditorGUILayout.PropertyField(maxHealth, new GUIContent("Max Health", "The maximum amount of health. THIS IS NOT THE CURRENT HEALTH."));

        //Critical Health
        EditorGUILayout.PropertyField(criticalHealth, new GUIContent("Critical Health", "The amount of critical health remaining which will make a pulsating effect."));
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Blur Effect", EditorStyles.boldLabel);
        //Use Blur Rffect
        EditorGUILayout.PropertyField(useBlurEffect, new GUIContent("Use Blur Effect", "Will show a blur effect when hit."));
        
        EditorGUI.BeginDisabledGroup (script.useBlurEffect == false);
            //Blur Image
            EditorGUILayout.PropertyField(blurImage, new GUIContent("Blur Image", "The radial blur image."));
            //Blur Duration
            EditorGUILayout.PropertyField(blurDuration, new GUIContent("Blur Duration", "The duration of the blur effect."));
            //Blur Fade Speed
            EditorGUILayout.PropertyField(blurFadeSpeed, new GUIContent("Blur Fade Speed", "The speed of the blur fading in and out."));
        EditorGUI.EndDisabledGroup();

        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Audios", EditorStyles.boldLabel);
        //Pulse Sound
        EditorGUILayout.PropertyField(pulseSound, new GUIContent("Pulse Sound", "Audio source of pulse sound. Will play when health is critical, if empty will skip."));

        //Fade World Audios
        EditorGUILayout.PropertyField(fadeAudios, new GUIContent("Fade Audios", "Fade out audio sources when health is critical."));
        
        //disable property when fade world audios is disabled
        EditorGUI.BeginDisabledGroup (script.fadeAudios == false);
            EditorGUILayout.PropertyField(audiosToFade, new GUIContent("Audios To Fade", "The audio sources you want to fade."));
            EditorGUILayout.PropertyField(audiosFadeVolume, new GUIContent("Audios Fade Volume", "The volume to fade the world audios to when in critical health."));
        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Healing", EditorStyles.boldLabel);
        //Auto Heal
        EditorGUILayout.PropertyField(autoHeal, new GUIContent("Auto Heal", "Turn on/off auto healing."));

        //Auto Heal -- If auto heal is false.. disable othe fields
        EditorGUI.BeginDisabledGroup (script.autoHeal == false);
            //Healing Multiplier
            EditorGUILayout.PropertyField(healingSpeed, new GUIContent("Healing Speed", "The speed of healing in auto heal."));

            //Auto Heal Time
            EditorGUILayout.PropertyField(autoHealTime, new GUIContent("Auto Heal Time", "The amount of uninterrupted time required before auto healing kicks in."));
        EditorGUI.EndDisabledGroup ();
        
        serializedObject.ApplyModifiedProperties();
    }
}
