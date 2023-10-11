using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
//Questionário foi removido do ciclo de gameplay do jogo
public class Questionario : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern int AddNumbers(int x, int y);
    DataGenerator dataGen;
    public GameObject[] questions;
    int activeQuestion;
    public GameObject replayButton;
    private CallAI callAI;

    string url = "https://docs.google.com/forms/u/0/d/e/1FAIpQLScRToMptMqLz3J0mSTqUqnZpBWOIxLEwiGF6B5qOY_s96DsJQ/formResponse";
    //string url = "https://docs.google.com/forms/u/0/d/e/1FAIpQLSeTWlIZjBk9SPzWS3e6JbhDtIf1UdTbiTv2EUxs2FHQ2DP3Qg/formResponse"; //original


    public string[] answers;
    private void Start()
    {
        replayButton.SetActive(false);
        dataGen = FindObjectOfType<DataGenerator>();
        callAI = FindObjectOfType<CallAI>();
        dataGen.UpdateCounters();
        activeQuestion = 0;
        answers = new string[questions.Length];
    }

    private void BuildRegressionSample()
    {
       callAI.data[0]   = (float)dataGen.playerData.totalLifeLost;
       callAI.data[1]   = (float)dataGen.playerData.timeSpent; 
       callAI.data[2]   = (float)dataGen.playerData.steps; 
       callAI.data[3]   = (float)dataGen.playerData.deaths; 
       callAI.data[4]   = (float)dataGen.playerData.percentKills; 
       callAI.data[5]   = (float)dataGen.playerData.percentItemsCollected; 
       callAI.data[6]   = float.Parse(answers[1]); // complexety
       callAI.data[7]   = float.Parse(answers[6]); // difficulty
       callAI.data[8]   = (float)dataGen.genData.averageEnemyDistance; 
       callAI.data[9]   = (float)dataGen.genData.averageItemDistance;
       callAI.data[10]  = (float)dataGen.playerData.interactions; 
       callAI.data[11]  = (float)dataGen.playthroughs;
       //callAI.data[12]  = float.Parse(answers[3]); // enemydensety
       //callAI.data[13]  = (float)dataGen.genData.itemDensity;
       //callAI.data[9]  = float.Parse(answers[0]); // mapsize
    }

    public void Send()
    {
        StartCoroutine(Post(dataGen));
        //Analytics.CustomEvent("LifeLost", new Dictionary<string, object> { { "TotalLifeLost", dataGen.playerData.totalLifeLost } });
    }
    public void Replay()
    {
        Send();
        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        StartCoroutine(Quit());
    }
    public IEnumerator Quit()
    {
        Send();
        yield return new WaitForSeconds(2);
        Application.Quit();
    }

    IEnumerator Post(DataGenerator data)
    {
        
        WWWForm form = new WWWForm();
        //player data to forms
        form.AddField("entry.963488702", data.playerData.interactions);//interactions
        form.AddField("entry.596939258", data.playerData.percentQuests.ToString().Replace(",", "."));//percentQuests

        //one playthrough info-------------------------------------------------------------------------------------
        form.AddField("entry.2005760843", data.totalLifeLost);//lifelost
        form.AddField("entry.1108869829", data.runLevel);//nível nas runs
        form.AddField("entry.1204426239", data.time);//time
        form.AddField("entry.175035077", data.steps);//steps
        form.AddField("entry.1023732896", data.playerData.deaths.ToString());//deaths
        form.AddField("entry.793916201", data.precision);//precision
        form.AddField("entry.788247621", data.percentKills);//percentKills
        form.AddField("entry.1765112349", data.percentItemPickup);//percentItems
        form.AddField("entry.1340334397", data.percentAmmoPickup);//percentAmmo


        form.AddField("entry.139430481", data.playthroughs);//playthroughs

        form.AddField("entry.53939988", data.runVictory);//victorious
        form.AddField("entry.646014955", data.playerData.foundSecret.ToString());//foundSecret
        form.AddField("entry.105534763", data.playerData.finalPosition.ToString().Replace(",", "."));//finalPosition



        //genData
        form.AddField("entry.2048159362", data.width);//width
        form.AddField("entry.1371681809", data.height);//height

        form.AddField("entry.1451868055", data.playerStartPos);//playerStart
        form.AddField("entry.1889395333", data.exitDoorPos);//exitDoor

        form.AddField("entry.1933815164", data.itemPositions);//itemPos
        form.AddField("entry.592241768", data.enemyPositions);//EnemyPos
        form.AddField("entry.364457296", data.seed);//seed

        form.AddField("entry.351761793", data.smooth);//smooth
        form.AddField("entry.371079419", data.minRegionSize);//minRegionSize
        form.AddField("entry.1717891015", data.randomFillPercent);//randomFillPercent
        
        form.AddField("entry.177368219", data.minEnemyDistance);//minEnemyDistance
        form.AddField("entry.200603954", data.minItemDistance);//minItemDistance
        form.AddField("entry.1101105295", data.averageEnemyDistance);//averageEnemyDistance
        form.AddField("entry.590433800", data.averageItemDistance);//averageItemDistance
                        
        form.AddField("entry.1803966669", data.enemyDensity);//EnemyDensity
        form.AddField("entry.647642773", data.itemDensity);//ItemDensity
        form.AddField("entry.1313680592", data.maxEnemies);//MaxEnemies
        form.AddField("entry.2094617186", data.maxItems);//MaxItems
        form.AddField("entry.1626933655", data.currentEnemies);//GeneratedEnemies
        form.AddField("entry.1461146218", data.currentItems);//GeneratedItems

        //playerInput
        form.AddField("entry.777965021", answers[0]);//mapSize
        form.AddField("entry.1009410047", answers[1]);//complexity
        form.AddField("entry.299056882", answers[2]);//enemyAmount
        form.AddField("entry.203252442", answers[3]);//enemyDensity

        form.AddField("entry.258848594", answers[4]);//interactionAmount
        form.AddField("entry.675072857", answers[5]);//conversationMaterial

        form.AddField("entry.1282593994", answers[6]);//difficulty
        form.AddField("entry.1313662470", answers[7]);//fun

        //end one playthrough info--------------------------------------------------------------------------------------------------


        
        


        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();
    }

    public void Answer()
    {
        answers[activeQuestion] = questions[activeQuestion].GetComponentInChildren<Slider>().value.ToString();

        questions[activeQuestion].SetActive(false);
        activeQuestion += 1;
        if (activeQuestion<8)
        {
            questions[activeQuestion].SetActive(true);
        }
        else
        {
            replayButton.SetActive(true);
            BuildRegressionSample();
            callAI.CallAPI();
        }
    }

}
