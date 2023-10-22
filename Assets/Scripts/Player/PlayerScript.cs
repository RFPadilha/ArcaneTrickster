using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Analytics;

public class PlayerScript : MonoBehaviour
{
    #region ControllerVariables
    private Rigidbody2D rb;
    private Animator animator;
    public Camera cam;
    private float moveX, moveY;
    private Vector2 mousePosition;
    private Vector3 mouseDirection;
    public float speed = 7f;
    public ParticleSystem dust;
    bool waiting = false;
    CameraController camControl;
    public UIManager playerUI;
    #endregion

    #region PlayerStats
    [Header("Player Stats")]
    PersistentStats stats;
    public int playerLevel = 1;
    public int maxLevel = 99;
    public int currentExp;
    public int baseExp = 100;
    public Text levelText;
    public int[] expToLevelUp;
    [SerializeField] ParticleSystem levelUpVFX;
    #endregion

    #region HealthVariables
    [Header("Health Variables")]
    public int maxHealth;
    public int currentHealth;
    private bool flashActive;//variável que controla o "piscar" do jogador ao ser atingido
    [SerializeField] private float flashLength = 0f;
    private float flashCounter = 0f;
    private SpriteRenderer playerSprite;
    [HideInInspector] public float waitToDamage = 2f;//tempo que o sistema espera quando o jogador permanece em contato com os inimigos antes de causar dano novamente
    [HideInInspector] public bool isTouching;//detecta se player entrou em contato com inimigo
    public float knockbackStrength = 5f; 
    Color red = new Color(1f, 0f, 0f, 1f);
    Color normal = new Color(1f, 1f, 1f, 1f);
    [SerializeField] ParticleSystem bloodVFX;
    #endregion

    #region DashVariables
    [Header("Dash Variables")]
    private float rollSpeed;
    [SerializeField] private float defaultRollSpeed = 1000f;
    [HideInInspector] public bool canRoll = true;
    [SerializeField] private float dashTime = .25f;//tempo de espera entre dashes
    private float rollCounter;
    private bool isRolling;
    #endregion

    #region AttackVariables
    [Header("Attack Variables")]
    public GameObject slashHitbox;
    [SerializeField] private float attackTime = .25f;//tempo de espera entre ataques
    public float angleOffset;
    private float attackCounter;
    private bool isAttacking;
    public int damage = 2;
    public GameObject damageTextPrefab;
    float swordAngleOffset = 10f;

    [SerializeField] GameObject swordRotation;
    Quaternion targetRotation;
    public float rotationSpeed;
    [SerializeField] GameObject sword;
    #endregion

    #region ShootingVariables
    [Header("TopDown Shooting")]
    public GameObject projectilePrefab;
    public float projectileForce = 20f;
    [HideInInspector] public Vector2 shootDirection;
    [HideInInspector] public float angle;
    [HideInInspector] public bool isShooting;
    [SerializeField] private float shootingTime = .25f;//tempo de espera entre disparos
    private float shootingCounter;//conta tempo até poder atirar novamente
    public int maxAmmo;
    public int currentAmmo;
    DataGenerator dataGen;
    #endregion

    #region DataVariables
    [Header ("Data Variables")]
    MapGenAutomata mapReference;//referencia ao gerador do mapa
    [HideInInspector] public int[,] pathing;
    int currPlayerX, currPlayerY;
    int playerX, playerY;

    public float percentItemsCollected;
    [HideInInspector] public float itemsCollected;
    [HideInInspector] public float itemsGenerated;

    float ammoPickup=0;
    public float ammoPickupRate;
    float shotsFired = 0;
    public float attacksAttempted = 0;
    [HideInInspector] public float successfulAttacks = 0;
    public float precision;
    public int totalLifeLost = 0;

    public float percentKills;
    [HideInInspector] public float enemiesDefeated;
    public int steps = 0;
    public int coins = 0;
    Scene activeScene;

    public QuestTracker tracker;
    #endregion

    #region Sounds
    [Header ("Sounds")]
    AudioManager audioManager;
    #endregion

    #region Inventory
    [Header("Inventory")]
    public GameObject inventoryUI;




    #endregion

    #region ExternalSetups
    [HideInInspector] public NecroBoss boss;
    #endregion
    void Start()
    {
        dataGen = FindObjectOfType<DataGenerator>();
        stats = FindObjectOfType<PersistentStats>();
        tracker = FindObjectOfType<QuestTracker>();
        audioManager = FindObjectOfType<AudioManager>();
        inventoryUI.SetActive(false);

        cam = Camera.main;
        camControl = cam.GetComponent<CameraController>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        activeScene = SceneManager.GetActiveScene();
        playerSprite = GetComponent<SpriteRenderer>();

        if (activeScene.name == "MapGeneration")
        {
            boss = FindObjectOfType<NecroBoss>();
            mapReference = FindObjectOfType<MapGenAutomata>();

            itemsGenerated = mapReference.currentItems;
            playerX = Mathf.FloorToInt(transform.position.x + mapReference.width / 2);//conversão da posição global para a posição no mapa do jogador
            playerY = Mathf.FloorToInt(transform.position.y + mapReference.height / 2);
            boss.target = transform;
        }

        steps = 0;
        attacksAttempted = 0;

        attackCounter = attackTime;
        maxHealth = stats.maxHealth;
        currentExp = stats.currentExp;
        playerLevel = stats.playerLevel;

        currentHealth = maxHealth;//makes sure current health can't be greater than max health

        levelText.text = playerLevel.ToString();//sets screen text for player level
        expToLevelUp = new int[maxLevel];
        expToLevelUp[0] = baseExp;
        for (int i = 1; i < expToLevelUp.Length; i++)
        {
            expToLevelUp[i] = Mathf.FloorToInt(expToLevelUp[i - 1] * 1.1f);//sets experience thresholds for leveling up
        }

        shootingCounter = shootingTime;
        currentAmmo = maxAmmo;
        sword.SetActive(false);
        slashHitbox.SetActive(false);
        playerUI.UpdateHealth();
    }


    void Update()//atualiza comandos de movimento o mais rápido possível
    {
        //toggle inventory, made for a future programmer that would use this code
        //if (Input.GetKeyDown(KeyCode.I)) inventoryUI.SetActive(!inventoryUI.activeSelf);


        moveX = Input.GetAxisRaw("Horizontal");//receives directional inputs
        moveY = Input.GetAxisRaw("Vertical");
        mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);//recebe posição do mouse na tela
        mouseDirection = -((Vector2)transform.position - mousePosition).normalized;//calcula vetor direcional do ataque, com valor absoluto = 1

        #region Movement and rolling
        if (!isRolling)
        {
            rb.velocity = new Vector2(moveX, moveY).normalized * speed;//actually does the moving part
            if(rb.velocity.magnitude > 0 && !audioManager.isPlaying("PlayerStep"))
            {
                audioManager.Play("PlayerStep");
            }
        }//controla movimento
        else
        {
            canRoll = false;
            rb.velocity = new Vector2(moveX, moveY).normalized * rollSpeed;
        }//gerencia troca de movimentos de rolamento/caminhada  
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (canRoll)
            {
                canRoll = false;
                isRolling = true;
                animator.SetBool("IsRolling", true);
                audioManager.Play("PlayerDash");
                rollSpeed = defaultRollSpeed;
            }
        }//starts rolling
        if (!canRoll)
        {
            rollCounter -= Time.deltaTime;
            if (rollCounter <= 0)
            {
                canRoll = true;
            }
        }//resets rolling
        else
        {
            rollCounter = dashTime;
        }//regulates rolling timers
        if (isRolling)
        {
            float dashSlowForce = 3f;
            float minDashSpeed = speed;

            GetComponent<TrailRenderer>().enabled = true;
            rollSpeed -= rollSpeed * dashSlowForce * Time.deltaTime;

            if ((rollSpeed < minDashSpeed))
            {
                isRolling = false;
                GetComponent<TrailRenderer>().enabled = false;
                animator.SetBool("IsRolling", false);
            }//stops rolling animation
        }//actually does the rolling part
        if (moveX == 1 || moveX == -1 || moveY == 1 || moveY == -1)
        {
            animator.SetFloat("lastMoveX", moveX);//required for correcting idle direction
            animator.SetFloat("lastMoveY", moveY);
        }//idle look direction
        #endregion


        if (isAttacking)
        {
            canRoll = false;
            rb.velocity = Vector2.zero;//blocks movement when attacking
            attackCounter -= Time.deltaTime;//counts down attack timer
            if (attackCounter <= 0)
            {
                //animator.SetBool("IsAttacking", false);
                animator.SetBool("IsShooting", false);
                isAttacking = false;//resets attack
                slashHitbox.SetActive(false);
                sword.SetActive(false);
                canRoll = true;
            }
        }//regulates attack timer and resets
        if (Input.GetKeyDown(KeyCode.Mouse0))//attack button is set to left mouse button
        {
            if (!isAttacking)//checks if player is not attacking already
            {
                attackCounter = attackTime;//resets attack timer
                isAttacking = true;//sets attack variables
                slashHitbox.SetActive(true);
                sword.SetActive(true);
                if (Time.timeScale != 0)
                {
                    attacksAttempted++;
                }
                //animator.SetBool("IsAttacking", true);
                animator.SetBool("IsShooting", true);
                audioManager.PlayUnrestricted("PlayerSlash");
            }//actually does the attacking
        }//regulates attacking


        if (flashActive)
        {
            if (flashCounter > flashLength * .99f)//flashes player in and out
            {
                playerSprite.color = red;
            }
            else if (flashCounter > flashLength * .82f)
            {
                playerSprite.color = normal;
            }
            else if (flashCounter > flashLength * .66f)
            {
                playerSprite.color = red;
            }
            else if (flashCounter > flashLength * .49f)
            {
                playerSprite.color = normal;
            }
            else if (flashCounter > flashLength * .33f)
            {
                playerSprite.color = red;
            }
            else if (flashCounter > flashLength * .16f)
            {
                playerSprite.color = normal;
            }
            else if (flashCounter > 0)
            {
                playerSprite.color = red;
            }
            else
            {
                playerSprite.color = normal;
                flashActive = false;//resets flashing
            }
            flashCounter -= Time.deltaTime;//counts down on flash times
        }//controls flashing when taking damage
        if (isTouching)
        {
            waitToDamage -= Time.deltaTime;//counts down damage instance timer
            if (waitToDamage <= 0)
            {
                HurtPlayer(damage);//causa dano ao permanecer encostado na hitbox
                GameObject damageTextInstance = Instantiate(damageTextPrefab, transform);
                damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().color = new Color32(200, 100, 100, 255);
                damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());

                waitToDamage = 2f;//resets counter
            }
        }//controla recepção de dano ao permanecer tocando um inimigo
        
        animator.SetFloat("moveX", rb.velocity.x);//sets movement animation variables to animate correctly
        animator.SetFloat("moveY", rb.velocity.y);
        animator.SetFloat("mousePosX", mouseDirection.x);//variáveis direcionais de ataque
        animator.SetFloat("mousePosY", mouseDirection.y);//ataca na direção do mouse, não do movimento

        if (activeScene.name == "MapGeneration")
        {
            precision = successfulAttacks / attacksAttempted;
            ammoPickupRate = ammoPickup / shotsFired;
            percentItemsCollected = itemsCollected / itemsGenerated;
            percentKills = enemiesDefeated / mapReference.currentEnemies;
            playerX = Mathf.FloorToInt(transform.position.x + mapReference.width / 2);//conversão da posição global para a posição no mapa do jogador
            playerY = Mathf.FloorToInt(transform.position.y + mapReference.height / 2);
            if (playerX != currPlayerX || playerY != currPlayerY)//se mudar de posição
            {
                steps++;
            }
        }//controla dados do jogador no mapa gerado
        
    }

    void FixedUpdate()//60 vezes por segundo, atualiza os comandos de mirar e atirar
    {
        mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        shootDirection = (mousePosition - rb.position).normalized;
        angle = Mathf.Atan2(shootDirection.y, shootDirection.x) * Mathf.Rad2Deg + 235f;
        
        //controla rotação da espada ao atacar
        if (attacksAttempted % 2 == 0)
        {
            targetRotation = swordRotation.transform.rotation;
            targetRotation = Quaternion.AngleAxis(angle - swordAngleOffset-15f, transform.forward);
        }
        else
        {
            targetRotation = swordRotation.transform.rotation;
            targetRotation = Quaternion.AngleAxis(angle - swordAngleOffset + 105f, transform.forward);
        }
        swordRotation.transform.rotation = Quaternion.Lerp(swordRotation.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        //-----------------------------------

        if (isShooting)
        {
            canRoll = false;
            rb.velocity = Vector2.zero;//blocks movement when shooting
            shootingCounter -= Time.fixedDeltaTime;//counts down attack timer
            if (shootingCounter <= 0)
            {
                animator.SetBool("IsShooting", false);
                isShooting = false;//resets shooting
                canRoll = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            if (!isShooting)
            {
                shootingCounter = shootingTime;//resets attack timer
                isShooting = true;//sets attack variables
                animator.SetBool("IsShooting", true);
                Shoot();
            }

        }
    }

    void LevelUp()
    {
        //efeitos
        audioManager.Play("LevelUp");
        levelUpVFX.Play();
        //recompensas
        damage += 2;
        UpgradeHealth(10);
        //atualiza valores
        stats.currentExp = stats.currentExp-expToLevelUp[playerLevel];
        currentExp = stats.currentExp;
        stats.playerLevel++;
        playerLevel = stats.playerLevel;
        levelText.text = playerLevel.ToString();
        playerUI.UpdateExp();

    }
    public void Shoot()
    {
        if (currentAmmo > 0)
        {
            audioManager.PlayUnrestricted("DaggerToss");
            currentAmmo--;

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
            projectile.GetComponent<DaggerToss>().damage = damage;

            Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
            projectileRb.AddForce(new Vector2(shootDirection.x, shootDirection.y) * projectileForce, ForceMode2D.Impulse);

            if (Time.timeScale != 0)
            {
                attacksAttempted++;
            }
            shotsFired++;
            playerUI.UpdateAmmo();
        }

    }//dispara projétil

    public void PickUpAmmo()
    {
        audioManager.PlayUnrestricted("DaggerPickup");
        if (currentAmmo < maxAmmo)
        {
            currentAmmo++;
        }
        ammoPickup++;
        playerUI.UpdateAmmo();
    }//incrementa munição
    void GetExp(int amount)
    {
        stats.currentExp += amount;
        currentExp = stats.currentExp;
        if (currentExp >= expToLevelUp[playerLevel]){
            LevelUp();
        }else playerUI.UpdateExp();
    }
    public void UpgradeHealth(int amount)
    {
        stats.maxHealth += amount;
        maxHealth = stats.maxHealth;
        currentHealth = maxHealth;
        playerUI.UpdateHealth();
    }//incrementa HP

    public void GetCoin()
    {
        for(int i = 0; i < tracker.quest.Length; i++)
        {
            if (tracker.quest[i].goal.goalType == QuestGoal.GoalType.Gather && tracker.quest[i].isActive)
            {
                tracker.quest[i].goal.Gathered();
                if (tracker.quest[i].goal.IsReached())
                {
                    audioManager.Play("FinishQuest");
                    GetExp(tracker.quest[i].expReward);
                    UpgradeHealth(tracker.quest[i].healthImprovement);
                    tracker.quest[i].Complete();
                    dataGen.completedQuests++;
                    dataGen.activeQuests -= 1;
                }
            }
        }
        coins++;
        itemsCollected++;
    }

    public void DefeatEnemy(int exp)
    {
        audioManager.PlayUnrestricted("EnemyDeath");
        isTouching = false;
        GetExp(exp);
        enemiesDefeated++;
        for (int i = 0; i < tracker.quest.Length; i++)
        {
            if (tracker.quest[i].goal.goalType == QuestGoal.GoalType.Kill && tracker.quest[i].isActive)
            {
                tracker.quest[i].goal.EnemyKilled();
                if (tracker.quest[i].goal.IsReached())
                {
                    GetExp(tracker.quest[i].expReward);
                    UpgradeHealth(tracker.quest[i].healthImprovement);
                    tracker.quest[i].Complete();
                    dataGen.completedQuests++;
                    dataGen.activeQuests -= 1;
                }
            }
        }

    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            EnemyHealthManager enemyHealthManager = other.GetComponent<EnemyHealthManager>();
            GameObject damageTextInstance = Instantiate(damageTextPrefab, other.transform);
            damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(damage.ToString());
            enemyHealthManager.HurtEnemy(damage);
        }
    }//trigger em questão está associado ao objeto da arma do personagem, que, ao entrar em contato com inimigo, causa dano

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            int enemyDamage = other.gameObject.GetComponent<EnemyStats>().damage;
            HurtPlayer(enemyDamage);//causa dano na colisão
            GameObject damageTextInstance = Instantiate(damageTextPrefab, other.transform);
            damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(enemyDamage.ToString());
            damageTextInstance.transform.GetChild(0).GetComponent<TextMeshPro>().color = new Color32(200, 100, 100, 255);//red 
        }        
    }//caso jogador encoste em algum inimigo, recebe dano

    public void OnCollisionExit2D(Collision2D other)//resets "touching" status
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            isTouching = false;
        }
    }

    public void HurtPlayer(int damageTaken)
    {
        HitStop(.1f);
        bloodVFX.Play();
        audioManager.PlayUnrestricted("EnemyHit");
        flashActive = true;//begins flashing player
        flashCounter = flashLength;//resets health timer
        currentHealth -= damageTaken;//decreases health
        totalLifeLost += damageTaken;
        playerUI.UpdateHealth();
        if (currentHealth <= 0)
        {
            audioManager.Play("PlayerDeath");
            for (int i = 0; i < tracker.quest.Length; i++)
            {
                tracker.quest[i].goal.currentAmount = 0;
            }
            FindObjectOfType<Continue>().GameOverScreen();
        }
    }

    public void HitStop(float duration)
    {
        if (waiting) return;
        Time.timeScale = 0f;
        StartCoroutine(WaitForFloat(duration));
    }//breve pausa ao ser atingido, melhoria de feedback para o jogador
    IEnumerator WaitForFloat(float duration)
    {
        waiting = true;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        waiting = false;
    }
}
