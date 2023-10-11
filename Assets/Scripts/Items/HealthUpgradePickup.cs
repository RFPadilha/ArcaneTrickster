using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script associado ao item que aumenta vida máxima do jogador

public class HealthUpgradePickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            FindObjectOfType<PlayerScript>().UpgradeHealth(25);
            FindObjectOfType<DataGenerator>().foundSecret = true;//no momento da implementação, esse item estava escondido no cenário
                                                                 //usado de parâmetro para identificar jogadores que gostam de explorar
            Destroy(gameObject);
        }
    }
}
