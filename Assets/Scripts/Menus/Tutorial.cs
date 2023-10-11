using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Script associado ao objeto "OnScreenPopUps", controla comportamento da interface do tutorial

public class Tutorial : MonoBehaviour
{
    public GameObject tutorialScreen;
    PersistentStats persistentStats;
    PlayerScript player;
    private void Start()
    {
        player = FindObjectOfType<PlayerScript>();
        persistentStats = FindObjectOfType<PersistentStats>();
        if (!persistentStats.closedTutorial)
        {
            OpenTutorial();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && tutorialScreen.activeSelf)
        {
            CloseTutorial();
        }
    }
    public void OpenTutorial()
    {
        Time.timeScale = 0;
        tutorialScreen.SetActive(true);
    }
    public void CloseTutorial()
    {
        if (player.attacksAttempted != 0) player.attacksAttempted = 0;
        tutorialScreen.SetActive(false);
        persistentStats.closedTutorial = true;
        Time.timeScale = 1;
    }
}
