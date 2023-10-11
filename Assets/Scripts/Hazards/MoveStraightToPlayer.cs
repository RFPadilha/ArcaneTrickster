using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Script utilizado para mover o inimigo "Skeleton" na direção do jogador.
//Skeleton possui um ponto de partida, onde é instanciado, e também é para onde retorna caso o jogador saia de alcance
//Move em linha reta até o jogador, o que quer dizer que qualquer obstáculo entre o inimigo e o jogador irá impedir a movimentação
//Talvez a implementação ideal desse script envolva um A*, usando os tiles do mapa como células
public class MoveStraightToPlayer : MonoBehaviour
{
    Transform player;
    public Transform rootPosition;//posição do objeto raiz, onde o inimigo é instanciado e para onde ele retorna ao parar de perseguir jogador
    public float speed;
    public float range;

    Rigidbody2D rb;
    Vector2 movement;
    EnemyHealthManager enemyHealth;

    //variáveis "hunt" usadas juntamente do bool "huntPlayer" do script "EnemyHealthManager" para configurar perseguição ao jogador
    float huntDuration = 5f;
    float huntCounter;

    Animator anim;
    PlayerScript playerRef;
    private void Start()
    {       
        rb = GetComponent<Rigidbody2D>();
        enemyHealth = GetComponent<EnemyHealthManager>();
        playerRef = FindObjectOfType<PlayerScript>();
        if (GetComponent<Animator>())//alguns inimigos como o canhão não usam o componente "Animator", utilizando somente rotação para simular comportamento
        {
            anim = GetComponent<Animator>();
        }
        player = playerRef.transform;
    }
    private void FixedUpdate()
    {
        if (Vector2.Distance(transform.position, player.position) < range || 
            (enemyHealth.huntPlayer && (huntCounter > 0)))
        {
            Vector2 moveDirection = (player.position - transform.position).normalized;
            MoveTowards(moveDirection);
            if (enemyHealth.huntPlayer)
            {
                huntCounter -= Time.deltaTime;
            }
        }
        else
        {
            ReturnToSpawnPoint();
            huntCounter = huntDuration;
            enemyHealth.huntPlayer = false;
        }

        if (anim)
        {
            if (rb.velocity.x != 0 || rb.velocity.y != 0)
            {
                anim.SetBool("isMoving", true);//activates moving animations
                anim.SetFloat("moveX", (player.position.x - transform.position.x));//controls movement animation variables
                anim.SetFloat("moveY", (player.position.y - transform.position.y));
                //these subtractions of player position minus this objects' position results in a 2D vector pointing towards the player
            }
            else
            {
                anim.SetBool("isMoving", false);
            }
        }//controla animação de caminhar
    }
    public void MoveTowards(Vector2 target)
    {
        rb.velocity = target * speed;
    }

    void ReturnToSpawnPoint()
    {
        Vector2 moveDirection = (rootPosition.position - transform.position).normalized;
        MoveTowards(moveDirection);
    }

    private void OnTriggerEnter2D(Collider2D other)//usado pada controlar posição ao ser atingido pelo ataque do jogador
    {
        if (other.gameObject.CompareTag("Weapon"))//knockback
        {
            //atualmente pode fazer inimigo atravessar paredes
            Vector2 difference = transform.position - other.transform.position;
            transform.position = new Vector2(transform.position.x + difference.x, transform.position.y + difference.y);
        }
    }
}
