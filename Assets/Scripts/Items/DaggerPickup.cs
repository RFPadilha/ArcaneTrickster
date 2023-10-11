using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script associado ao item da adaga que inimigos criam quando derrotados ou colide com outros objetos

public class DaggerPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            FindObjectOfType<PlayerScript>().PickUpAmmo();
            Destroy(gameObject);
        }
    }
}
