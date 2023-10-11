using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//associado ao portal que transporta jogador da cidade para o mapa gerado

public class NextScene : MonoBehaviour
{
    [SerializeField] GameObject mapConfigMenu;
    MapConfig config;
    bool done = false;
    public UIManager uiManager;//precisa da UI para ativar menu de configuração ao entrar em contato e mudar de cena ao confirmar parâmetros
    private void Start()
    {
        done = false;
        config = mapConfigMenu.GetComponent<MapConfig>();
    }
    private void Update()
    {
        if (config.accept && !done)
        {
            mapConfigMenu.SetActive(false);
            Time.timeScale = 1f;
            StartCoroutine(uiManager.FadeOut("MapGeneration", true, 1));
            done = true;
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            mapConfigMenu.SetActive(true);
            Time.timeScale = 0;
        }
    }
}
