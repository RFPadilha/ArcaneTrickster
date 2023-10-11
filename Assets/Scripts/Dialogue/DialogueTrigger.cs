using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
//Script associado em cada GameObject que possua caixa de diálogo na interação, tal como NPCs
//Usado para determinar quando iniciar diálogo e quais frases serão ditas
public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public GameObject interactionText;
    DialogueManager dialogueManager;
    void Start()
    {
        dialogueManager = FindObjectOfType<DialogueManager>();//referência ao controlador
    }

    public void TriggerDialogue()
    {
        if (gameObject.GetComponentInParent<QuestGiver>())
        {
            dialogueManager.questGiver = gameObject.GetComponentInParent<QuestGiver>();
        }
        else dialogueManager.questGiver = null;


        dialogueManager.StartDialogue(dialogue);
    }

    //Controla comportamento em relação ao alcance de interação
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                TriggerDialogue();
            }
            interactionText.SetActive(true);
        }
    }
    void OnTriggerStay2D(Collider2D other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                TriggerDialogue();
            }
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            interactionText.SetActive(false);
            dialogueManager.EndDialogue();
        }
    }
}
