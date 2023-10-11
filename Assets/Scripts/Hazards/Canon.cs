using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Canon : MonoBehaviour
{
    public float rotationSpeed;
    public float range;

    public float rateOfFire;
    float rofCounter;

    public float projectileSpeed;
    public Transform firePoint;
    public GameObject projectile;
    bool isFiring;

    Vector2 playerPos;
    Transform player;
    private void Start()
    {
        player = FindObjectOfType<PlayerScript>().transform;//adquire alvo
    }
    private void FixedUpdate()
    {
        playerPos = player.position;

        //cálculo para direção que canhão mira
        Vector2 aimDir = playerPos - new Vector2(transform.position.x,transform.position.y);//subtração retorna direção partindo do canhão em direção ao jogador
        float angle = Mathf.Atan2(aimDir.y,aimDir.x)*Mathf.Rad2Deg - 90f;//calcula ângulo determinado pela direção calculada na linha acima
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);//calcula qual a rotação final que o objeto deve ter para mirar corretamente
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.fixedDeltaTime);//aplica rotação gradativamente

        if (isFiring)//controla tempo entre disparos
        {
            rofCounter -= Time.fixedDeltaTime;//counts down attack timer
            if (rofCounter <= 0)
            {
                isFiring = false;//resets shooting
            }
        }

        if (Vector2.Distance(playerPos, transform.position) < range && !isFiring)//dispara quando possível se jogador está dentro do alcance
        {

            rofCounter = rateOfFire;//resets attack timer
            isFiring = true;//sets attack variables
            ShootPlayer();            
        }

    }
    void ShootPlayer()
    {
        GameObject shot = Instantiate(projectile, firePoint.position, transform.rotation);//instancia objeto "projétil"
        Rigidbody2D rb = shot.GetComponent<Rigidbody2D>();//acessa componente de "rigidbody" do projétil
        shot.GetComponent<EnemyProjectile>().damage = GetComponent<EnemyStats>().damage;//repassa dano do canhão para o projétil
        rb.AddForce(firePoint.up * projectileSpeed, ForceMode2D.Impulse);//aplica força no rigidbody para movimentar o objeto como um disparo (um impulso inicial, sem força constante)
    }
}
