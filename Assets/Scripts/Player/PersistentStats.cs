using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * Script utilizado para controlar e acessar dados que precisam permanecer em mais de uma cena
 * Somente uma instância associada ao objeto "GameManager" na cena "Preload"
 * Controla o acesso aos dados do jogador e da configuração do mapa.
 * São todos públicos para poderem ser acessados por outros scripts
 */

public class PersistentStats : MonoBehaviour
{   
    // buffer de mapas com mapas bem avaliados no dataset
    public List<int[]> buffer = new List<int[]>{
        new int[] {60,40,2,15,15,15,19,12,54,36,68,3},
        new int[] {100,60,2,20,15,25,19,12,50,50,20,3},
        new int[] {100,60,2,20,15,25,19,12,50,50,20,3},
        new int[] {70,50,2,20,15,25,19,12,46,49,86,4},
        new int[] {120,100,2,20,15,25,19,12,58,15,49,4},
        new int[] {85,60,2,20,15,25,19,12,50,3,47,4},
        new int[] {80,60,2,20,15,25,19,12,50,0,3,3},
        new int[] {110,80,2,20,15,25,19,12,50,1,3,3}
    };

    [Header("Player Stats")]
    public int playerLevel = 1;
    public int currentExp = 0;

    [Header("Health Variables")]
    public int maxHealth = 50;
    public bool closedTutorial;//variável que garante que a tela de tutorial aparece somente ao entrar no jogo.

    [Header("Map configuration")]
    public int width = 100;//largura e altura do mapa
    public int height = 60;

    public int smooth = 2;//grau de suavização dos quadrados gerados, "escava" as paredes do mapa
    public int minRegionSize = 50;//tamanho mínimo das regiões geradas, exclui o que estiver abaixo

    public bool useRandomSeed = false;
    public string seed = "0";

    [Range(0, 100)]
    public int randomFillPercent = 55;//porcentagem de terreno/parede
    [Range(0, 100)]
    public int enemyDensity = 75;//medidor de frequência de posicionamento de inimigos e itens
    [Range(0, 100)]
    public int itemDensity = 35;

    public int minEnemyDistance = 5;
    public int minItemDistance = 15;

    public int maxEnemies= 99;//garante maior eficácia nos parâmetros de posicionamento
    public int maxItems=99;
    
    public void SetNewGeneratedMap(Predictions predictions){

        var num = Random.Range(0,8);

        if((int)predictions.predictions[0][11] >= 2){
            int[] map           = new int[11];
            map[0]              = (int)predictions.predictions[0][0];
            map[1]              = (int)predictions.predictions[0][1];
            map[2]              = (int)predictions.predictions[0][2];
            map[3]              = (int)predictions.predictions[0][3];
            map[4]              = (int)predictions.predictions[0][4];
            map[5]              = (int)predictions.predictions[0][5];
            map[6]              = (int)predictions.predictions[0][6];
            map[7]              = (int)predictions.predictions[0][7];
            map[8]              = (int)predictions.predictions[0][8];
            map[9]              = (int)predictions.predictions[0][9];
            map[10]             = (int)predictions.predictions[0][10];
            buffer.Add(map);
            
            width               = (int)predictions.predictions[0][0];
            height              = (int)predictions.predictions[0][1];
            smooth              = (int)predictions.predictions[0][2];
            minRegionSize       = (int)predictions.predictions[0][3];
            minEnemyDistance    = (int)predictions.predictions[0][4];
            minItemDistance     = (int)predictions.predictions[0][5];
            maxEnemies          = (int)predictions.predictions[0][6];
            maxItems            = (int)predictions.predictions[0][7];
            randomFillPercent   = (int)predictions.predictions[0][8];
            enemyDensity        = (int)predictions.predictions[0][9];
            itemDensity         = (int)predictions.predictions[0][10];
            Debug.Log("mapa regressão");
        }
        else{
            int[] map           = buffer[num];
            width               = map[0];
            height              = map[1];
            smooth              = map[2];
            minRegionSize       = map[3];
            minEnemyDistance    = map[4];
            minItemDistance     = map[5];
            maxEnemies          = map[6];
            maxItems            = map[7];
            randomFillPercent   = map[8];
            enemyDensity        = map[9];
            itemDensity         = map[10];
            Debug.Log("mapa buffer");
        }
    }

}
