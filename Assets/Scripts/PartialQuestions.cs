using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
//Dado que o questionário foi removido do ciclo de gameplay, esse script não possui aplicação atualmente
public class PartialQuestions : MonoBehaviour
{
    DataGenerator dataGen;
    public GameObject[] questions = new GameObject[6];
    public string[] answers;
    int activeQuestion;
    int maxQuestions;
    // Start is called before the first frame update
    void Start()
    {
        maxQuestions = questions.Length;
        dataGen = FindObjectOfType<DataGenerator>();
        activeQuestion = 0;
        answers = new string[maxQuestions];
    }

    public void Answer()
    {
        answers[activeQuestion] = questions[activeQuestion].GetComponentInChildren<Slider>().value.ToString();

        questions[activeQuestion].SetActive(false);

        dataGen.answers[activeQuestion] += answers[activeQuestion] + " / ";

        activeQuestion += 1;
        if (activeQuestion < maxQuestions)
        {
            questions[activeQuestion].SetActive(true);
        }
        else
        {
            //dataGen.UpdateCounters();
            dataGen.SaveAsCSV();
            Time.timeScale = 1;
            gameObject.SetActive(false);
            SceneManager.LoadScene("Game");//reloads town scene

        }
    }
}
