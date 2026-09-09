using System.Collections;
using System.Collections.Generic;
using TUFHelper;
using UnityEngine;

public class SFXControllerScript : MonoBehaviour
{
    public static SFXControllerScript Instance { get; private set; }

    public AudioSource buttonClick1, buttonClick2;

    public void Awake()
    {
        Instance = this;
    }

    public enum SoundType
    {
        ButtonClick1,
        ButtonClick2,
    }

    public void PlaySound(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.ButtonClick1:
                buttonClick1.Play();
                break;
            case SoundType.ButtonClick2:
                buttonClick2.Play();
                break;
        }
    }
}