using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
//classe para a definição e configuração de sons
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f,1f)]
    public float volume;
    [Range(.1f,3f)]
    public float pitch;
    [HideInInspector]
    public AudioSource source;
    public bool loop;
}

