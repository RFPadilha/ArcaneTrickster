using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Script utilizado para o ataque de tentáculo que sai do chão do único chefão implementado até o momento
//Esse mesmo script pode ser utilizado para armadilhas no futuro, qualquer coisa que "aparece - causa dano - desaparece"
public class GroundHazard : MonoBehaviour
{
    public int damage = 10;
    AudioManager audioManager;
    private void OnTriggerEnter2D(Collider2D collision)//deve ter um colisor do tipo "trigger"
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<PlayerScript>().HurtPlayer(damage);
        }
    }
    private void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.PlayUnrestricted("GroundTentacle");
        Destroy(gameObject, 2f);
    }
}
