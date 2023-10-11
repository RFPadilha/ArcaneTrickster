using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Script associado a todos os inimigos
//Controla comportamento dos HitPoints e agressividade

public class EnemyHealthManager : MonoBehaviour
{
    public int maxHealth, currentHealth;

    //usado para controlar comportamento de "piscar" vermelho quando atingido
    private bool flashActive;
    [SerializeField]
    private float flashLength = 0f;
    private float flashCounter = 0f;

    //usado para atribuir experiência e devolver munição
    [SerializeField]
    private int experienceGiven = 10;
    public int heldAmmo = 0;

    //se "true" está perseguindo jogador, é "true" quando jogador causa dano ou entra no alcance pré-definido
    public bool huntPlayer;

    private SpriteRenderer enemySprite;
    public GameObject daggerDrop;
    AudioManager audioManager;
    PlayerScript playerRef;
    CameraController camControl;
    bool loadedPlayer = false;

    //efeitos visuais
    public GameObject hitFX;
    public GameObject dieFX;


    void Start()
    {
        camControl = FindObjectOfType<CameraController>();
        huntPlayer = false;
        audioManager = FindObjectOfType<AudioManager>();
        currentHealth = maxHealth;
        enemySprite = GetComponent<SpriteRenderer>();
    }
    
    void Update()
    {
        if (!loadedPlayer)
        {
            playerRef = FindObjectOfType<PlayerScript>();
            loadedPlayer = true;
        }//garante referência ao jogador

        if (flashActive)
        {
            if (flashCounter > flashLength * .99f)//flashes enemy in and out
            {
                enemySprite.color = new Color(1f, 0f, 0f, 1f);
            }
            else if (flashCounter > flashLength * .82f)
            {
                enemySprite.color = new Color(1f, 1f, 1f, 1f);
            }
            else if (flashCounter > flashLength * .66f)
            {
                enemySprite.color = new Color(1f, 0f, 0f, 1f);
            }
            else if (flashCounter > flashLength * .49f)
            {
                enemySprite.color = new Color(1f, 1f, 1f, 1f);
            }
            else if (flashCounter > flashLength * .33f)
            {
                enemySprite.color = new Color(1f, 0f, 0f, 1f);
            }
            else if (flashCounter > flashLength * .16f)
            {
                enemySprite.color = new Color(1f, 1f, 1f, 1f);
            }
            else if (flashCounter > 0)
            {
                enemySprite.color = new Color(1f, 0f, 0f, 1f);
            }
            else
            {
                enemySprite.color = new Color(1f, 1f, 1f, 1f);
                flashActive = false;//resets to not flashing
            }
            flashCounter -= Time.deltaTime;//counts down flash timer
        }//controls flashing when taking damage

       
    }
    public void HurtEnemy(int damage)
    {
        //gamefeel code
        GetHitVFX();//efeito visual de atingir
        audioManager.PlayUnrestricted("PlayerHit");//efeito sonoro
        if (GetComponent<NecroBoss>())
        {
            GetComponent<NecroBoss>().UpdateHealthBar();
        }//se é o único "chefe" implementado até o momento, atualiza UI da barra de vida

        //game mechanics
        huntPlayer = true;
        currentHealth -= damage;//decrementa vida
        if (currentHealth <= 0 && !gameObject.GetComponent<NecroBoss>())//para um inimigo simples, somente experiência e devolver munição após a morte
        {
            playerRef.DefeatEnemy(experienceGiven);
            if (heldAmmo > 0)
            {
                DropAmmo();
            }
            StartCoroutine(WaitToDie());
        }

        //game data
        playerRef.successfulAttacks++;
    }
    public void DropAmmo()
    {
        for(int i = heldAmmo; i>0; i--)
        {
            float distX = Random.Range(0f, 1f);
            float distY = Random.Range(0f, 1f);
            if (i == heldAmmo)
            {
                Vector2 pos = transform.position;
                Instantiate(daggerDrop, pos, Quaternion.identity);//primeira munição no exato local que inimigo foi derrotado
            }
            else
            {
                Vector2 pos = new Vector2(transform.position.x + distX, transform.position.y + distY);
                Instantiate(daggerDrop, pos, Quaternion.identity);//munição restante aleatorizada em alcance de raio 1
            }
        }
    }
    
    void GetHitVFX()
    {
        flashActive = true;//starts flashing red when taking damage
        flashCounter = flashLength;
        StartCoroutine(camControl.Shake(.05f, .1f));
        StartCoroutine(AnimateHitVFXOverTime(.2f, .1f, 1f, hitFX));
    }
    IEnumerator AnimateHitVFXOverTime(float seconds, float startValue, float endValue, GameObject vfx)
    {
        //anima VFX em transparência, tamanho e rotação ao longo do tempo
        SpriteRenderer vfxSprite = vfx.GetComponent<SpriteRenderer>(); ;
        vfx.SetActive(true);
        float animationTime = 0f;
        while (animationTime < seconds)
        {
            animationTime += Time.deltaTime;
            float lerpValue = animationTime / seconds;
            vfxSprite.color = new Color(vfxSprite.color.r, vfxSprite.color.g, vfxSprite.color.b, Mathf.Lerp(1f, 0f, lerpValue));//opacidade de 100% a 0%
            vfx.transform.localScale = new Vector3(Mathf.Lerp(startValue, endValue, lerpValue), Mathf.Lerp(startValue, endValue, lerpValue), 1f);//aumenta tamanho do efeito
            vfx.transform.Rotate(Vector3.forward * (400 * Time.deltaTime));//gira efeito visual
            yield return null;
        }
        vfx.SetActive(false);
    }
    IEnumerator WaitToDie()//garante execução correta dos efeitos de derrota ao não executar em momentos de pausa
    {
        while(Time.timeScale != 1f)
        {
            yield return null;
        }
        Destroy(gameObject);
    }
}
