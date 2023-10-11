using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
//Localizado no GameObject "GameManager", assim, existe somente uma instancia do AudioManager que é acessível em todas as cenas


public class AudioManager : MonoBehaviour
{
    //array de sons que podem vir a ser tocados em jogo
    public Sound[] sounds;

    public static AudioManager instance;//referência para esse script

    void Awake()
    {
        if(instance == null) 
        {
            instance = this;
        }
        else{
            Destroy(gameObject);
            return;
        }
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name)//toca som uma vez, interrompendo caso esteja tocando
    {
        
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s==null){
            return;
        }

        if (!s.source.isPlaying)
        {
            s.source.Play();
        }
    }
    public void PlayUnrestricted(string name)//toca som uma vez, permitindo que diversas instâncias do mesmo som toquem ao mesmo tempo
    {

        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null)
        {
            return;
        }
        s.source.Play();
    }
    public bool isPlaying(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s.source.isPlaying)
        {
            return true;
        }
        else return false;
    }
    
}
