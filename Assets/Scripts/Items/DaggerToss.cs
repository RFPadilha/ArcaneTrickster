using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//script associado ao projétil disparado pelo jogador

public class DaggerToss : MonoBehaviour
{
    public GameObject daggerDrop;
    public GameObject damageTextPrefab;
    public int damage;
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyHealthManager enemy = other.gameObject.GetComponent<EnemyHealthManager>();
            Transform pos = other.transform;
            enemy.heldAmmo++;
            enemy.HurtEnemy(damage);

            GameObject damageTextInstance = Instantiate(damageTextPrefab, pos);
            damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().color = new Color32(255, 255, 255, 255);
            damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
            Destroy(gameObject);
        }//se acerta inimigo

        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy"))
        {
            Instantiate(daggerDrop, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }//se acerta cenário

    }
}
