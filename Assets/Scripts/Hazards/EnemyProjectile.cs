using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;//usado para exibir efeito de texto

/*
 * Controla comportamentos de projéteis inimigos após serem instanciados pelo script do inimigo em questão:
 *  - Dano causado
 *  - Efeito visual de texto
 *  - Instancia animação de explosão (talvez seja mais computacionalmente eficiente associar a animação ao "prefab" do projétil ao invés de instanciar um novo objeto)
 *  - (incluir som ao colidir)
 */
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] GameObject damageTextPrefab;
    [SerializeField] GameObject explosionVFX;

    public int damage;//público para ser determinado pelo script que instancia projéteis
    
    public void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            other.gameObject.GetComponent<PlayerScript>().HurtPlayer(damage);//causa dano ao acertar jogador
            GameObject damageTextInstance = Instantiate(damageTextPrefab, other.transform);
            damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().color = new Color32(200, 100, 100, 255);
            damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
            
        }
        //colisão com terrenos e objetos
        if (!other.gameObject.CompareTag("Player") && !other.gameObject.CompareTag("Enemy"))
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Weapon"))
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
