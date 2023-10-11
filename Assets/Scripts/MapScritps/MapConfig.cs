using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Script que define os parâmetros de geração do mapa


public class MapConfig : MonoBehaviour
{
    PersistentStats stats;
    DataGenerator dataGen;
    public GameObject missionCheck;//aviso que é mostrado caso o jogador tenha zero missões ativas
    public GameObject startingMenu;//tela inicial do menu
    public GameObject configMenu;//menu de configuração de parâmetros
    public GameObject[] mapGenData;//parâmetros em si
    public bool accept = false;


    void Start()
    {
        dataGen = FindObjectOfType<DataGenerator>();
        accept = false;
        stats = FindObjectOfType<PersistentStats>();
        Load();//ao abrir menu, carrega últimas configurações
    }
    private void Update()
    {
        if (mapGenData[6].GetComponent<Toggle>().isOn)//componente para usar seed aleatória
        {
            mapGenData[7].SetActive(false);
        }else
        {
            mapGenData[7].SetActive(true);
        }
        if (dataGen.activeQuests == 0)
        {
            missionCheck.SetActive(true);
        }else missionCheck.SetActive(false);
    }
    public void Load()//loads previously used parameters
    {
        //carrega valores usados anteriormente
        mapGenData[0].GetComponent<InputField>().text = stats.width.ToString();
        mapGenData[1].GetComponent<InputField>().text = stats.height.ToString();
        mapGenData[2].GetComponent<InputField>().text = stats.minRegionSize.ToString();
        mapGenData[3].GetComponent<InputField>().text = stats.minEnemyDistance.ToString();
        mapGenData[4].GetComponent<InputField>().text = stats.minItemDistance.ToString();
        mapGenData[5].GetComponent<InputField>().text = stats.smooth.ToString();

        mapGenData[6].GetComponent<Toggle>().isOn = stats.useRandomSeed; //randomSeed bool, toggle
        mapGenData[7].GetComponent<InputField>().text = stats.seed.ToString();
        mapGenData[8].GetComponent<Slider>().value = stats.randomFillPercent; //map fill percent, slider
        mapGenData[9].GetComponent<Slider>().value = stats.enemyDensity; //enemy density, slider
        mapGenData[10].GetComponent<Slider>().value = stats.itemDensity; //item density, slider
        mapGenData[11].GetComponent<InputField>().text = stats.maxEnemies.ToString();
        mapGenData[12].GetComponent<InputField>().text = stats.maxItems.ToString();
    }
    public void Accept()
    {
        //converte parâmetros de texto para int
        int.TryParse(mapGenData[0].GetComponent<InputField>().text, out stats.width);
        int.TryParse(mapGenData[1].GetComponent<InputField>().text, out stats.height);
        int.TryParse(mapGenData[2].GetComponent<InputField>().text, out stats.minRegionSize);
        int.TryParse(mapGenData[3].GetComponent<InputField>().text, out stats.minEnemyDistance);
        int.TryParse(mapGenData[4].GetComponent<InputField>().text, out stats.minItemDistance);
        int.TryParse(mapGenData[5].GetComponent<InputField>().text, out stats.smooth);

        stats.useRandomSeed = mapGenData[6].GetComponent<Toggle>().isOn;
        stats.seed = mapGenData[7].GetComponent<InputField>().text;
        stats.randomFillPercent =  Mathf.FloorToInt(mapGenData[8].GetComponent<Slider>().value);
        stats.enemyDensity = Mathf.FloorToInt(mapGenData[9].GetComponent<Slider>().value);
        stats.itemDensity = Mathf.FloorToInt(mapGenData[10].GetComponent<Slider>().value);

        int.TryParse(mapGenData[11].GetComponent<InputField>().text, out stats.maxEnemies);
        int.TryParse(mapGenData[12].GetComponent<InputField>().text, out stats.maxItems);

        accept = true;
        //ao aceitar, jogador é enviado ao mapa configurado
        
    }
    public void DefaultGen()
    {
        accept = true;
    }
    public void ConfigureGen()
    {
        startingMenu.SetActive(false);
        configMenu.SetActive(true);
    }
    public void Cancel()
    {
        accept = false; 
        startingMenu.SetActive(true);
        configMenu.SetActive(false);
        gameObject.SetActive(false);
        Time.timeScale = 1;
        //close popup
    }

}
