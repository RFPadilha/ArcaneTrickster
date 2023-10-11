using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

/*
 * Controla a geração do mapa em sua abstração na forma de matriz:
 * - Recebe dados persistentes relevantes para a geração do script "PersistentStats"
 * - Posiciona Jogador, inimigos, itens e "portal" do chefe de acordo com dados recebidos
 * - Calcula métricas de geração
 * - Mantém contador de quantos mapas foram gerados na sessão de jogo para o script "DataGen"
 */
public class MapGenAutomata : MonoBehaviour
{
    DataGenerator dataGen;
    PersistentStats stats;

    //map config, save on persistent data
    public int width, height;//largura e altura do mapa
    public int smooth;//grau de suavização dos quadrados gerados
    public int minRegionSize;//tamanho mínimo das regiões geradas (exclui o que estiver abaixo)

    public bool useRandomSeed = false;
    public string seed;

    [Range(0, 100)]
    public int randomFillPercent;//porcentagem de terreno/parede

    [Range(0, 100)]
    public int enemyDensity;
    [Range(0, 100)]
    public int itemDensity;

    public int minEnemyDistance;
    public int minItemDistance;

    public int maxEnemies;
    public int maxItems;



    [HideInInspector] public int[,] map;//matriz do mapa
    //geração de inimigos
    [HideInInspector] public int currentItems = 0;
    [HideInInspector] public int currentEnemies = 0;
    public GameObject enemyPrefab;
    public GameObject enemyPrefab2;
    [HideInInspector] public GameObject[] enemies;
    [HideInInspector] public Vector2[] enemyPositions;
    [HideInInspector] public float averageEnemyDistance;
    [HideInInspector] public float averageItemDistance;
    [HideInInspector] bool guaranteeSpawn;

    //geração de itens
    
    public GameObject coinPrefab;
    [HideInInspector] public GameObject[] items;
    [HideInInspector] public Vector2[] itemPositions;
    public GameObject playerPrefab;
    public GameObject bossPortalPrefab;
    public GameObject arenaSpawnPos;
    public GameObject endStage;
    public GameObject player;
    bool canSpawnEnd = false;
    bool canSpawnPlayer = false;
    public Vector2 playerStartPos;
    System.Random pseudoRandom;

    public bool reverseMap = true;

    private void Start()//Recebe dados e gera mapa assim que a cena inicia
    {
        stats = FindObjectOfType<PersistentStats>();
        dataGen = FindObjectOfType<DataGenerator>();
        bossPortalPrefab.GetComponent<MoveToArena>().arenaSpawnPos = arenaSpawnPos;

        //receive persistent stats relevant to generation
        
        width = stats.width;
        height = stats.height;
        minRegionSize = stats.minRegionSize;
        minEnemyDistance = stats.minEnemyDistance;
        minItemDistance = stats.minItemDistance;
        smooth = stats.smooth;
        useRandomSeed = stats.useRandomSeed;
        seed = stats.seed;
        randomFillPercent = stats.randomFillPercent;
        enemyDensity = stats.enemyDensity;
        itemDensity = stats.itemDensity;
        maxEnemies = stats.maxEnemies;
        maxItems = stats.maxItems;
        
        guaranteeSpawn = false;

        if (useRandomSeed)//usado para aleatorizar seed
        {
            seed = Time.time.ToString();
        }
        pseudoRandom = new System.Random(seed.GetHashCode());//pseudo random number generator

        enemies = new GameObject[maxEnemies];
        enemyPositions = new Vector2[maxEnemies];

        items = new GameObject[maxItems];
        itemPositions = new Vector2[maxItems];

        //Raramente a inicialização do mapa falhava, "try-catch" foi um "band-aid" inesperadamente eficiente
        try
        {
            GenerateMap();
        }
        catch (Exception)
        {

            GenerateMap();
            throw;
        }
        dataGen.playthroughs++;
    }

    
    void GenerateMap()
    {
        dataGen.doneGen = false;
        map = new int[width, height];

        RandomFillMap();
        ProcessMap();

        for (int x = 0; x < smooth; x++)
        {
            SmoothMap();
        }

        ProcessMap();//processar o mapa duas vezes dessa forma resultou em mapas de aparência mais "natural"

        //repassa dados relevantes para o script que gera os polígonos que compôem o mapa e inicializa a composição
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(map, 1, reverseMap);

        #region PlayerPlacement

        if (player != null)//garante que somente um player será criado
        {
            Destroy(player);
        }
        else
        {
            while (!canSpawnPlayer)//encontra posição válida para o jogador
            {

                int x = Mathf.FloorToInt(pseudoRandom.Next(2, width - 2));
                int y = Mathf.FloorToInt(pseudoRandom.Next(2, height - 2));

                Vector2 spawnPos = new Vector2(x - (width / 2), y - (height / 2));//conversão de parâmetros de geração para posicionamento real no unity
                if (map[x, y] == 0)
                {
                    canSpawnPlayer = PreventSpawnOverlap(spawnPos, 5f);//garante que jogador não será posicionado dentro de paredes
                }
                if (canSpawnPlayer)
                {
                    player = Instantiate(playerPrefab, spawnPos, Quaternion.identity) as GameObject;
                    playerStartPos = player.transform.position;
                }
            }
        }
        #endregion

        #region ExitPlacement
        if (endStage != null)//garante que somente uma saída será gerada
        {
            Destroy(endStage);
        }
        while (!canSpawnEnd)//encontra posição válida para saída
        {

            int x = Mathf.FloorToInt(pseudoRandom.Next(0, width - 1));
            int y = Mathf.FloorToInt(pseudoRandom.Next(0, height - 1));
            if (map[x, y] == 0 && Vector2.Distance(player.transform.position, new Vector2(x,y)) > 5)//garante distância mínima do jogador
            {
                Vector2 spawnPos = new Vector2(x, y);
                canSpawnEnd = PreventSpawnOverlap(spawnPos, 5f);
            }
            if (canSpawnEnd)
            {
                endStage = Instantiate(bossPortalPrefab, new Vector2(x - (width / 2), y - (height / 2)), Quaternion.identity) as GameObject;
            }
        }
        #endregion

        #region EnemyPlacement 
        //generating enemies
        foreach (List<Coord> region in GetRegions(0))//recebe os tiles vazios
        {
            foreach (Coord tile in region)//para cada tile vazio
            {
                if ((pseudoRandom.Next(0, 100) < enemyDensity && currentEnemies < maxEnemies)//confere se inimigo deve ser inserido
                    || guaranteeSpawn)//é necessário garantir o posicionamento do primeiro
                {
                    Vector2 spawnPos = new Vector2(tile.tileX - (width / 2), tile.tileY - (height / 2));//converte posição inicial para coordenadas reais do unity
                    bool canSpawn  = PreventSpawnOverlap(spawnPos, 5f);//garante posição fora das paredes
                    if(!canSpawn){
                        guaranteeSpawn = true;
                        continue;
                    }

                    
                    if (!enemies[0])//insere primeiro inimigo
                    {
                        if(canSpawn){
                            GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity) as GameObject;//instancia inimigo
                            enemies[0] = go;//coloca no array de referencia
                            enemyPositions[0] = new Vector2(Mathf.FloorToInt(go.transform.position.x + width / 2), Mathf.FloorToInt(go.transform.position.y + height / 2));
                            currentEnemies = 1;//inicia contador
                            guaranteeSpawn = false;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < currentEnemies; i++)//compara distancia a todos os inimigos instanciados
                        {
                            if (Vector2.Distance(enemies[i].transform.position, spawnPos) < minEnemyDistance)
                            {
                                canSpawn = false;
                                break;
                            }
                        }
                        //se está em uma distância razoavel do inimigo anterior
                        if (canSpawn)
                        {
                            if (currentEnemies % 2 == 1)
                            {
                                GameObject go = Instantiate(enemyPrefab2, spawnPos, Quaternion.identity) as GameObject;//instancia inimigo
                                enemies[currentEnemies] = go;//coloca no array de referencia
                                enemyPositions[currentEnemies] = new Vector2(Mathf.FloorToInt(go.transform.position.x + width / 2), Mathf.FloorToInt(go.transform.position.y + height / 2));
                                currentEnemies++;//incrementa contador
                                guaranteeSpawn = false;
                            }
                            else
                            {
                                GameObject go = Instantiate(enemyPrefab, spawnPos, Quaternion.identity) as GameObject;//instancia inimigo
                                enemies[currentEnemies] = go;//coloca no array de referencia
                                enemyPositions[currentEnemies] = new Vector2(Mathf.FloorToInt(go.transform.position.x + width / 2), Mathf.FloorToInt(go.transform.position.y + height / 2));
                                currentEnemies++;//incrementa contador
                                guaranteeSpawn = false;
                            }
                            
                        }
                    }
                }
            }
        }
        #endregion

        #region ItemPlacement
        //generating Items
        guaranteeSpawn = false;
        foreach (List<Coord> region in GetRegions(0))//recebe os tiles vazios
        {
            foreach (Coord tile in region)//para cada tile vazio
            {
                if ((pseudoRandom.Next(0, 100) < itemDensity && currentItems < maxItems) || guaranteeSpawn)//confere se item deve ser inserido
                {
                    Vector2 spawnPos = new Vector2(tile.tileX - (width / 2), tile.tileY - (height / 2));
                    bool canSpawn  = PreventSpawnOverlap(spawnPos, 5f);
                    if(!canSpawn){
                        guaranteeSpawn = true;
                        continue;
                    }

                    if (items[0] == null)
                    {
                        if(canSpawn)
                        {
                            GameObject go = Instantiate(coinPrefab, spawnPos, Quaternion.identity) as GameObject;//instancia item
                            items[0] = go;//coloca no array de referencia
                            itemPositions[0] = new Vector2(Mathf.FloorToInt(go.transform.position.x + width / 2), Mathf.FloorToInt(go.transform.position.y + height / 2));
                            currentItems = 1;//inicia contador
                            guaranteeSpawn = false;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < currentItems; i++)//compara distancia a todos os itens instanciados
                        {
                            if (Vector2.Distance(items[i].transform.position, spawnPos) < minItemDistance)
                            {
                                canSpawn = false;
                                break;
                            }
                        }
                        //se está em uma distância razoavel do inimigo anterior
                        if (canSpawn)
                        {
                            GameObject go = Instantiate(coinPrefab, spawnPos, Quaternion.identity) as GameObject;//instancia item
                            items[currentItems] = go;//coloca no array de referencia
                            itemPositions[currentItems] = new Vector2(Mathf.FloorToInt(go.transform.position.x + width / 2), Mathf.FloorToInt(go.transform.position.y + height / 2));
                            currentItems++;//incrementa contador
                            guaranteeSpawn = false;
                        }
                    }
                }
            }
        }
        #endregion

        #region calculatingAverageDistances
        float distSum = 0;
        for (int i = 0; i < currentEnemies; i++)
        {
            for (int j = 0; j < currentEnemies; j++)
            {
                distSum += Vector2.Distance(enemies[i].transform.position, enemies[j].transform.position);
            }
        }
        averageEnemyDistance = distSum / (enemies.Length * enemies.Length);

        float distSumItem = 0;
        for (int i = 0; i < currentItems; i++)
        {
            for (int j = 0; j < currentItems; j++)
            {
                distSumItem += Vector2.Distance(items[i].transform.position, items[j].transform.position);
            }
        }
        averageItemDistance = distSumItem / (items.Length * items.Length);
        #endregion

    }

    public bool PreventSpawnOverlap(Vector2 spawnPos, float radius)//função que previne instanciação de inimigos / itens / jogador dentro das paredes
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(spawnPos, radius);//projeta circunferencia 2d em torno da posição sendo testada

        for (int i = 0; i < colliders.Length; i++)//se algum colisor atravessa a circunferencia:
        {
            Vector2 centerPoint = colliders[i].bounds.center;
            float wid = colliders[i].bounds.extents.x;
            float hei = colliders[i].bounds.extents.y;

            float leftExtent = centerPoint.x - wid;
            float rightExtent = centerPoint.x + wid;
            float topExtent = centerPoint.y + hei;
            float botExtent = centerPoint.y - hei;

            if (spawnPos.x >= leftExtent && spawnPos.x <= rightExtent)
            {
                if (spawnPos.y >= botExtent && spawnPos.y <= topExtent)//se a posição testada está dentro da circunferencia que teve colisões
                {
                    return false;
                }
            }
        }
        return true;//somente quando circunferencia não possui intersecções com outros colisores
    }

    /* usado somente no botão "Recarregar" no menu de pausa, um band-aid enquanto a instanciação do jogador ainda tinha chances de ocorrer dentro das paredes
     * Agora não possui utilidade, está presente somente para que seja adequadamente removida, junto do botão do menu
     */
    public void RespawnPlayer()
    {
        
        bool canRespawn = false;
        while (!canRespawn)
        {
            int x = Mathf.FloorToInt(pseudoRandom.Next(1, width - 2));
            int y = Mathf.FloorToInt(pseudoRandom.Next(1, height - 2));
            Vector2 spawnPos = new Vector2(x - (width / 2), y - (height / 2));
            if (map[x, y] == 0)
            {
                canRespawn = PreventSpawnOverlap(spawnPos, 3f);
            }
            if (canRespawn)
            {
                player.transform.position = spawnPos;
                playerStartPos = player.transform.position;
            }
        }
        
            
    }

    #region MapGenFunctions
    void RandomFillMap()//preenche mapa aleatoriamente
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)//percorrendo o mapa todo:
            {
                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)//se estamos nas bordas externas do mapa
                {
                    map[i, j] = 1;//garante paredes nas bordas do mapa
                }
                else
                {
                    map[i, j] = (pseudoRandom.Next(0, 100) < randomFillPercent) ? 1 : 0;//aleatoriza que áreas serão terreno ou parede
                }
            }
        }
    }

    void SmoothMap()//transforma tiles dependendo da quantidade de paredes ao redor dele
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)//percorrendo todos os tiles
            {
                int neighbourWallTiles = GetSurroundingWallCount(i, j);//determina quantos tiles de parede o cercam
                if (neighbourWallTiles > 4)//mais que 4, transforma tile em parede
                {
                    map[i, j] = 1;
                }
                else if (neighbourWallTiles < 4)//menos que 4, transforma em aberto
                {
                    map[i, j] = 0;
                }

            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)//"pega" paredes em torno do tile selecionado
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++) //percorre tiles na esquerda e na direita
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)//percorre tiles acima e abaixo
            {
                if (IsInMapRange(neighbourX, neighbourY))//garante que não conta tiles fora do mapa
                {
                    if (neighbourX != gridX || neighbourY != gridY)//garante que não conta tile selecionado
                    {
                        wallCount += map[neighbourX, neighbourY];//adiciona paredes em torno do tile selecionado, não conta espaços abertos
                    }
                }
                else
                {
                    wallCount++;//adiciona parede da borda do mapa
                }
            }
        }
        return wallCount;
    }

    void ProcessMap()//usado para detectar regiões no mapa e eliminá-las se forem muito pequenas, posteriormente será usado para conectá-las
    {
        List<List<Coord>> wallRegions = GetRegions(1);//"pega" regiões de parede, tiles marcados com 1

        foreach (List<Coord> wallRegion in wallRegions)//para cada uma dessas regiões
        {
            if (wallRegion.Count < minRegionSize)//deleta regiões muito pequenas
            {
                foreach (Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }

        }



        List<List<Coord>> roomRegions = GetRegions(0);//"pega" regiões de sala, tiles abertos
        List<Room> remainingRooms = new List<Room>();//cria lista de salas abertas


        foreach (List<Coord> roomRegion in roomRegions)//Para cada uma
        {
            if (roomRegion.Count < minRegionSize)//deleta regiões muito pequenas
            {
                foreach (Coord tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                remainingRooms.Add(new Room(roomRegion, map));//se forem de tamanho adequado, são adicionadas na lista de salas

            }
        }
        remainingRooms.Sort();//ordena salas da maior para a menor
        remainingRooms[0].isMainRoom = true;//maior sala se torna a principal
        remainingRooms[0].isAccessibleFromMain = true;


        ConnectClosestRoom(remainingRooms);
    }

    List<List<Coord>> GetRegions(int tileType)//forma uma lista de regiões (salas). "Coord" são tiles, "List<Coord>" são salas, "List<List<Coord>>" é uma lista com todas as salas
    {
        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height];//marca tiles verificados

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)//para cada tile do mapa
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)//se tile não foi verificado e é do tipo correto
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);//marca tiles da nova região
                    regions.Add(newRegion);//adiciona nova região a lista

                    foreach (Coord tile in newRegion)//marca todos os tiles da região
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)//forma uma lista dos tiles de cada sala
    {
        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];//marca tiles verificados
        int tileType = map[startX, startY];//determina tipo de tile: 1=parede, 0=aberto

        Queue<Coord> queue = new Queue<Coord>();//fila de tiles a serem percorridos
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)//enquanto houverem tiles a percorrer
        {
            Coord tile = queue.Dequeue();//"pega" primeiro da fila
            tiles.Add(tile);//adiciona a lista

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)//para cada tile ao redor do tile selecionado
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))//se está dentro do mapa e não é uma diagonal (pensa que verificamos os tiles em forma de +)
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType) //se tile não foi verificado e é do tipo correto
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));//verifica e adiciona a fila
                        }
                    }
                }
            }
        }
        return tiles;
    }

    struct Coord//definição de coordenadas de um tile
    {
        public int tileX;
        public int tileY;
        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }

    bool IsInMapRange(int x, int y)//função auxiliar para determinar se coordenadas estão dentro do mapa
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    void ConnectClosestRoom(List<Room> allRooms, bool forceAcessToMain = false)//com lista de todas as salas, garante que todas as salas estarão conectadas
    {
        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();//cria 2 sub-listas

        if (forceAcessToMain)//se devemos conectar as salas a main
        {
            //lista A irá conter salas que devem ser conectadas
            //lista B irá conter salas que já estão conectadas a main
            foreach (Room room in allRooms)//para cada sala na lista
            {
                if (room.isAccessibleFromMain)//se já consegue acessar a main sala
                {
                    roomListB.Add(room);//adiciona na lista B
                }
                else
                {
                    roomListA.Add(room);//senão adiciona na lista A
                }
            }
        }
        else
        {
            roomListA = allRooms;//senão sub-listas serão iguais
            roomListB = allRooms;
        }


        int bestDistance = 0;//menor distância entre salas
        Coord bestTileA = new Coord();
        Coord bestTileB = new Coord();//melhores tiles de partida
        Room bestRoomA = new Room();
        Room bestRoomB = new Room();//melhores salas de partida
        bool possibleConnectionFound = false;



        foreach (Room roomA in roomListA)//em cada sala na lista A
        {
            if (!forceAcessToMain)//se não precisa acessar a main
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)//se já possui conexão
                {
                    continue;//parte para a próxima sala A
                }
            }

            foreach (Room roomB in roomListB)//para cada sala na lista B
            {
                if (roomA == roomB || roomA.IsConnected(roomB))//se  estã conectada a sala da lista A ou é igual
                {
                    continue;//parte para proxima sala B
                }
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)//para cada tile nas arestas de ambas as salas
                    {
                        Coord tileA = roomA.edgeTiles[tileIndexA];
                        Coord tileB = roomB.edgeTiles[tileIndexB];//referencia tiles

                        int distance = (int)(Mathf.Pow((tileA.tileX - tileB.tileX), 2) + Mathf.Pow((tileA.tileY - tileB.tileY), 2));//calcula distância entre eles com pitágoras

                        if (distance < bestDistance || !possibleConnectionFound)//se é melhor que a melhor distancia ou ainda não foi encontrada uma conexão
                        {
                            bestDistance = distance;//determina nova melhor distancia
                            possibleConnectionFound = true;//marca conexão encontrada
                            bestTileA = tileA;
                            bestTileB = tileB;//marca locais da conexão
                            bestRoomA = roomA;
                            bestRoomB = roomB;//marca salas de conexão
                        }
                    }
                }
            }
            if (possibleConnectionFound && !forceAcessToMain)//se encontrou conexão após percorrer lista B e não necessita de acesso a sala main
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);//cria passagem com tiles e salas selecionados
            }
        }

        if (possibleConnectionFound && forceAcessToMain)//se encontrou conexão após percorrer lista A e necessita de acesso a main
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);//cria passagem
            ConnectClosestRoom(allRooms, true);//chamada recursiva com necessidade de conectar a main
        }

        if (!forceAcessToMain)//após percorrer ambas as listas, ainda existe chance de conglomerados de salas sem conexão (possibleConnectionFound == false)
        {
            ConnectClosestRoom(allRooms, true);//conecta salas remanescentes
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)//usa DrawCircle para criar passagens entre as salas
    {
        Room.ConnectRooms(roomA, roomB);//marca que salas devem estar conectadas
        List<Coord> line = GetLine(tileA, tileB);//traça linha entre 2 tiles
        foreach (Coord c in line)//para cada tile por onde a linha passa
        {
            DrawCircle(c, 1);//abre corredor
            //receber tiles do corredor em uma lista de corredores potencialmente leva a melhor posicionamento de portas, armadilhas, inimigos, etc...
        }
    }

    void DrawCircle(Coord c, int r)//abre corredores entre salas com um determinado raio r
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)//partindo do ponto central do tile, projetando um raio r em torno dele:
            {
                if (x * x + y * y <= r * r)//se está dentro do circulo
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;//coordenadas do tile a ser aberto
                    if (IsInMapRange(drawX, drawY))//se está dentro do mapa
                    {
                        map[drawX, drawY] = 0;//abre tile
                        //retornar tiles abertos
                    }
                }
            }
        }
    }

    List<Coord> GetLine(Coord from, Coord to)//descobre distancia em tiles entre salas que serao conectadas
    {
        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;//marca coordenadas de partida

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;//marca direção da linha nos eixos

        bool inverted = false;//as contas foram feitas assumindo que a linha fosse sempre ser maior no eixo x, essa variável garante o funcionamento caso a linha seja maior em y

        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);//com a direção marcada, sabemos o sinal de incremento

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)//inverte caso linha seja maior em y
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);

            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;//usado para saber quando incrementar na direção y (ou x caso "inverted")

        for (int i = 0; i < longest; i++)//até percorrer distancia maior
        {
            line.Add(new Coord(x, y));//adiciona coordenadas novas
            if (inverted)//incrementa variável que cresce mais rápido
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;//incrementa contador


            //incrementa variável que cresce mais devagar(pensa em uma linha no plano cartesiano a um angulo de 30 graus: ela cresce mais rapido em x do que em y
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;//reseta contador da variável mais devagar
            }
        }

        return line;//retorna tiles da menor distancia calculada
    }

    class Room : IComparable<Room>//definição do que é uma sala e métodos de set e compare com seus atributos
    {
        public List<Coord> tiles;//tiles pertencentes
        public List<Coord> edgeTiles;//tiles nas arestas
        public List<Room> connectedRooms;//salas conectadas
        public int roomSize;//numero de tiles na sala
        public bool isAccessibleFromMain;//acessa a sala principal?
        public bool isMainRoom;//é sala principal?

        public Room()
        {

        }
        public Room(List<Coord> roomTiles, int[,] map)//construtor
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            edgeTiles = new List<Coord>();

            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if ((x == tile.tileX || y == tile.tileY) && x < map.GetLength(0) && y < map.GetLength(1))
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMain()//determina que sala é acessível a partir da main
        {
            if (!isAccessibleFromMain)
            {
                isAccessibleFromMain = true;
                foreach (Room connectedRoom in connectedRooms)//para cada sala conectada, marca que também são acessíveis pela main
                {
                    connectedRoom.SetAccessibleFromMain();
                }
            }
        }
        public static void ConnectRooms(Room roomA, Room roomB)//conecta duas salas
        {
            if (roomA.isAccessibleFromMain)
            {
                roomB.SetAccessibleFromMain();
            }
            else if (roomB.isAccessibleFromMain)
            {
                roomA.SetAccessibleFromMain();
            }
            roomA.connectedRooms.Add(roomB);//marca salas conectadas
            roomB.connectedRooms.Add(roomA);
        }
        public bool IsConnected(Room otherRoom)
        {
            return connectedRooms.Contains(otherRoom);
        }
        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
    #endregion
}

   
