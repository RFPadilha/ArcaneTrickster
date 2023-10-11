using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    private Queue<string> sentences;
    public GameObject dialogueBox;
    public Text nameText;
    public Text dialogueText;
    //public Animator animator;
    public GameObject interactionText;
    public GameObject questButtonMage;
    public GameObject questButtonWarrior;
    DataGenerator dataGen;

    public QuestGiver questGiver;
    public bool activeDialogue = false;
    void Start()
    {
        dataGen = FindObjectOfType<DataGenerator>();
        sentences = new Queue<string>();
        //animator = GetComponent<Animator>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        /* deve haver uma maneira melhor de controlar os diálogos
         * no momento o script habilita e desabilita os botões de "Aceitar missão" dos NPCs de acordo com qual está interagindo
         * uma função que controle esse comportamento com poucos parâmetros seria ideal
         */
        dialogueBox.SetActive(true);
        if (!questGiver)
        {
            questButtonMage.SetActive(false);
            questButtonWarrior.SetActive(false);
        }
        else
        {
            if (questGiver.name == "MageNialNPC")
            {
                questButtonMage.SetActive(true);
                questButtonWarrior.SetActive(false);
            }
            if (questGiver.name == "WarriorDaveNPC")
            {
                questButtonWarrior.SetActive(true);
                questButtonMage.SetActive(false);
            }

        }

        interactionText.SetActive(false);//texto de "Aperte E para interagir" é desativado ao iniciar diálogo
        activeDialogue = true;
        //animator.SetBool("IsOpen", activeDialogue);

        nameText.text = dialogue.name;
        sentences.Clear();

        foreach(string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if(sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = sentences.Dequeue();
        dataGen.interactions++;
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence (string sentence)//usado para o efeito de "digitação progressiva" durante as falas
    {
        dialogueText.text = "";
        foreach(char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }

    public void EndDialogue()
    {
        activeDialogue = false;
        dialogueBox.SetActive(false);
        //animator.SetBool("IsOpen", activeDialogue);
    }
}
