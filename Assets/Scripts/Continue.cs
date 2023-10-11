using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//Script localizado no GameObject "OnScreenPopups" somente na cena "MapGeneration"
//Controla comportamento da UI quando o jogador é derrotado
public class Continue : MonoBehaviour
{
    DataGenerator dataGen;
    UIManager uiManager;//requer referência a UI associada ao prefab do jogador para realizar a mudança de cenas
    public GameObject gameOverScreen;
    bool gameHasEnded = false;
    bool loadedUI = false;
    private void Start()
    {
        dataGen = FindObjectOfType<DataGenerator>();
        gameOverScreen.SetActive(false);
        gameHasEnded = false;
    }
    public void GameOverScreen()
    {
        Time.timeScale = 0;
        if (!loadedUI && uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
            loadedUI = true;
        }//garante presença da UI
        if (!gameHasEnded)
        {
            dataGen.deathCounter++;
            gameHasEnded = true;
            gameOverScreen.SetActive(true);
        }//incrementa contador de mortes e mostra tela de game over
    }
    public void EndGame()
    {
        dataGen.SaveAsCSV();
        StartCoroutine(WaitToQuit());
    }
    public void Restart()//caso o jogador queira jogar novamente, recarrega a cidade inicial
    {
        gameOverScreen.SetActive(false);
        gameHasEnded = false;
        Time.timeScale = 1;
        dataGen.SaveAsCSV();
        StartCoroutine(uiManager.FadeOut("Game", true, 1));
    }

    IEnumerator WaitToQuit()//simplesmente executar Application.Quit() ocasionalmente resultava nos dados não serem enviados, uma curta espera resolveu o problema
    {
        yield return new WaitForSeconds(2);
        Application.Quit();
    }

}
