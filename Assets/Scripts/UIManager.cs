using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public PlayerScript playerScript;//references to use healthbar
    public Slider healthBar;
    public Text hpText;
    public Slider ammoCounter;
    public Slider expBar;

    public Slider healthBarFade;//usado para a animação da healthbar

    public Image fade;
    /* Resta implementar:
     * -tela de carregamento
     * -Sinal visual de vida baixa
     */
    private void Start()
    {
        StartCoroutine(FadeOut("", false, 1));//fade in inicial ao iniciar o jogo
    }
    private void Update()
    {
        if(healthBar.value <= 0)
        {
            StartCoroutine(FadeOut(""));
        }
    }

    /* Funções de update que são chamadas quando relevante
     * P: Por que não estão dentro da função "Update"?
     * R: Porque não necessitam ser atualizadas a cada frame, somente quando são utilizadas
     */

    public void UpdateAmmo()//ao disparar ou recuperar munição
    {
        ammoCounter.maxValue = playerScript.maxAmmo;
        ammoCounter.value = playerScript.currentAmmo;
    }
    public void UpdateHealth()//ao receber dano, subir de nível ou se curar
    {
        float startValue = healthBar.value;

        healthBar.maxValue = playerScript.maxHealth;
        healthBarFade.maxValue = healthBar.maxValue;
        healthBar.value = playerScript.currentHealth;//controls updating of healthbar
        hpText.text = "HP: " + playerScript.currentHealth + "/" + playerScript.maxHealth;

        StartCoroutine(AnimateSliderOverTime(1f, startValue, healthBar.value));
    }
    public void UpdateExp()//ao cumprir missão ou derrotar inimigo
    {
        expBar.maxValue = playerScript.expToLevelUp[playerScript.playerLevel - 1];
        expBar.value = playerScript.currentExp;
    }

    IEnumerator AnimateSliderOverTime(float seconds, float startValue, float endValue)//anima barra de vida ao receber dano
    {
        float animationTime = 0f;
        while (animationTime < seconds)
        {
            animationTime += Time.deltaTime;
            float lerpValue = animationTime / seconds;
            healthBarFade.value = Mathf.Lerp(startValue, endValue, lerpValue);
            yield return null;
        }
    }

    public IEnumerator FadeOut(string sceneToLoad, bool fadeToBlack = true, int speed = 5)//controla fade-in e fade-out
    {
        Color imgColor = fade.GetComponent<Image>().color;
        float fadeAmount;

        if (fadeToBlack)
        {
            while (fade.GetComponent<Image>().color.a <= 1)//fade-out
            {
                fadeAmount = imgColor.a + (speed * Time.deltaTime);
                imgColor = new Color(imgColor.r, imgColor.g, imgColor.b, fadeAmount);
                fade.GetComponent<Image>().color = imgColor;
                yield return null;
            }
            if(fade.GetComponent<Image>().color.a >= 1 && sceneToLoad != "")//quando fade-out é concluído, passa para a cena passada como parâmetro
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
        else//fade to transparency (fade-in)
        {
            while (fade.GetComponent<Image>().color.a > 0)
            {
                fadeAmount = imgColor.a - (speed * Time.deltaTime);
                imgColor = new Color(imgColor.r, imgColor.g, imgColor.b, fadeAmount);
                fade.GetComponent<Image>().color = imgColor;
                yield return null;
            }
        }
    }
}

