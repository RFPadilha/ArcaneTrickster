using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script associado ao item da moeda

public class CoinPickup : MonoBehaviour
{
    PlayerScript player;
    AudioManager audioManager;

    private void Start()
    {
        player = FindObjectOfType<PlayerScript>();
        audioManager = FindObjectOfType<AudioManager>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            audioManager.PlayUnrestricted("CoinPickup");
            player.GetCoin();
            Destroy(gameObject);
        }
    }
}
