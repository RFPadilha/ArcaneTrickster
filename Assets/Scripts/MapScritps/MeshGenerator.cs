using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MeshGenerator : MonoBehaviour
{

    public SquareGrid squareGrid;//define grid onde os quadrados base serão colocados
    List<Vector3> vertices;
    List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();//"lista" de todos os triangulos existentes
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();
    public MeshFilter caveMesh;//objeto onde ficarão as meshes de fato
    public Tilemap tilemap;
    public Tile[] tiles;

    public void GenerateMesh(int[,] map, float squareSize, bool reverse = false)
    {

        int wall = 1;
        tilemap.size = new Vector3Int(map.GetLength(0), map.GetLength(1), 1);
        tilemap.transform.position = new Vector3(.5f-map.GetLength(0) / 2, .5f-map.GetLength(1)/2, 0);

        if (reverse)
        {
            wall = 0;
        }
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();//remove quaisquer dados pré-existentes


        squareGrid = new SquareGrid(map, squareSize, wall);//cria nova grid

        vertices = new List<Vector3>();
        triangles = new List<int>();//novas listas de triângulos e vértices

        for (int i = 0; i < squareGrid.squares.GetLength(0); i++)
        {
            for (int j = 0; j < squareGrid.squares.GetLength(1); j++)
            {
                TriangulateSquare(squareGrid.squares[i, j], i, j);//gera as meshes relevantes para cada quadrado no mapa
            }
        }

        Mesh mesh = new Mesh();
        
        //o que realmente gera as meshes in-game
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        caveMesh.mesh = mesh;//aplica meshes no editor
        Generate2DColliders();//gera colisores nas paredes
    }

    void Generate2DColliders()
    {

        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();//cria novos colisores de aresta
        for (int i =0; i< currentColliders.Length; i++)
        {
            Destroy(currentColliders[i]);//destroi colisores pré-existentes
        }
        CalculateMeshOutlines();//calcula arestas

        foreach(List<int> outline in outlines)//para cada aresta
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();//cria colisor
            Vector2[] edgePoints = new Vector2[outline.Count];//vetor para a posição dos vértices das arestas

            for (int i=0; i<outline.Count; i++)
            {
                edgePoints[i] = new Vector2(vertices[outline[i]].x, vertices[outline[i]].z);//marca posição vértices das arestas
            }
            edgeCollider.points = edgePoints;//repassa vértices para determinar colisores de fato
        }
    }//gera colisores nas paredes complexas do mapa

    void CalculateMeshOutlines()
    {

        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)//para cada vértice
        {
            if (!checkedVertices.Contains(vertexIndex))//se vértice ainda não foi verificado
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);//recebe novo vértice conectado ao vértice em questao
                if (newOutlineVertex != -1)//se vértice pertence a aresta conhecida
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();//nova aresta
                    newOutline.Add(vertexIndex);//adiciona outline com o ponto
                    outlines.Add(newOutline);//adiciona outline aos outros outlines das arestas existentes
                    FollowOutline(newOutlineVertex, outlines.Count - 1);//com o ponto em questão, traça outline até novo vértice conectado, pertencente a aresta
                    outlines[outlines.Count - 1].Add(vertexIndex);//adiciona outline a lista de outlines
                }
            }
        }
    }//"traça" as linhas entre os pontos das arestas do mapa
    void TriangulateSquare(Square square, int i, int j)
    {
        switch (square.configuration)
        {
            case 0://"0 pontos" não necessita de mesh
                break;
            //meshes com 1 ponto
            case 1:
                MeshFromPoints(square.centerBottom, square.botLeft, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1),tiles[0]);
                break;
            case 2:
                MeshFromPoints(square.centerRight, square.botRight, square.centerBottom);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[1]);
                break;
            case 4:
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[3]);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[7]);
                break;
            //meshes com 2 pontos
            case 3:
                MeshFromPoints(square.centerRight, square.botRight, square.botLeft, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[2]);
                break;
            case 6:
                MeshFromPoints(square.centerTop, square.topRight, square.botRight, square.centerBottom);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[5]);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerBottom, square.botLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[8]);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[11]);
                break;
            case 5://2 pontos em diagonais opostas
                MeshFromPoints(square.centerTop, square.topRight, square.centerRight, square.centerBottom, square.botLeft, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[4]);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.botRight, square.centerBottom, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[9]);
                break;
            //meshes com 3 pontos
            case 7:
                MeshFromPoints(square.centerTop, square.topRight, square.botRight, square.botLeft, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[6]);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centerTop, square.centerRight, square.botRight, square.botLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[10]);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centerRight, square.centerBottom, square.botLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[12]);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.botRight, square.centerBottom, square.centerLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[13]);
                break;
            //mesh com 4 pontos
            case 15:
                MeshFromPoints(square.topLeft, square.topRight, square.botRight, square.botLeft);
                tilemap.SetTile(new Vector3Int(i, j, 1), tiles[14]);
                break;

        }
    }//individualmente busca por cada configuração possível dos triângulos das meshes (16 ao todo) e aplica sprite apropriada no tilemap

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleDictionary.ContainsKey(vertexIndexKey))//se vértice já foi encontrado
        {
            triangleDictionary[vertexIndexKey].Add(triangle);//adiciona triangulo novo a sua lista de triangulos associados
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();//senão, cria uma nova lista de triangulos pertencentes a esse vértice
            triangleList.Add(triangle);//adiciona triângulo encontrado a lista
            triangleDictionary.Add(vertexIndexKey, triangleList);//adiciona vértice e triangulo ao dicionário de triangulos
        }
    }//dicionário de todos os triângulos das meshes

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines[outlineIndex].Add(vertexIndex);//adiciona próximo vértice a aresta do mapa
        checkedVertices.Add(vertexIndex);//marca que vértice foi checado
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);//busca próximo vértice na aresta do mapa

        if (nextVertexIndex != -1)//se existir
        {
            FollowOutline(nextVertexIndex, outlineIndex);//repete para o próximo
        }
    }//define as linhas das arestas do mapa

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];//lista de triangulos que contém o vértice

        for (int i = 0; i < trianglesContainingVertex.Count; i++)//para cada triangulo que possui esse vértice
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];//vértice sendo checado
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))//para cada vértice que não é o selecionado em cada triangulo, e não foi verificado ainda
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))//se é aresta do mapa
                    {
                        return vertexB;//retorna vértice sendo checado para traçar aresta do mapa
                    }
                }
            }
        }

        return -1;//senão, vértice checado não pertence aos vértices da aresta do mapa
    }//busca próximo vértice pertencente a aresta do mapa

    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)//se 2 vértices estão entre 2 triângulos, não é aresta do mapa
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }//detecta se outline é aresta do mapa

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);
        if (points.Length >= 3)
        {
            CreateTriangle(points[0], points[1], points[2]);
        }
        if (points.Length >= 4)
        {
            CreateTriangle(points[0], points[2], points[3]);
        }
        if (points.Length >= 5)
        {
            CreateTriangle(points[0], points[3], points[4]);
        }
        if (points.Length >= 6)
        {
            CreateTriangle(points[0], points[4], points[5]);
        }
    }//marca vértices dos triângulos de cada mesh gerada
    void AssignVertices(Node[] points)
    {
        for(int i=0;i<points.Length; i++)
        {
            if(points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
                
            }
        }
    }//marca posição global dos vértices de cada mesh gerada

    void CreateTriangle(Node a, Node b, Node c)//dados 3 nós, cria a definição de um triangulo em 3 posições sequenciais na lista
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    //para cada bloco de mapa temos 4 "control nodes", um em cada canto, e 4 "nodes"
    //cada node é o ponto intermediário da aresta entre control nodes, usamos eles para
    //gerar as meshes de mapa
    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize, int wallValue)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for(int i = 0; i < nodeCountX; i++)
            {
                for(int j = 0; j < nodeCountY; j++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + i * squareSize + squareSize / 2, 0, -mapHeight / 2 + j * squareSize + squareSize / 2);
                    controlNodes[i, j] = new ControlNode(pos, map[i, j] == wallValue, squareSize);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int i = 0; i < nodeCountX-1; i++)
            {
                for (int j = 0; j < nodeCountY-1; j++)
                {
                    squares[i, j] = new Square(controlNodes[i, j + 1], controlNodes[i + 1, j + 1], controlNodes[i + 1, j], controlNodes[i, j]);
                }
            }
        }
    }//define grid de quadrados
    public class Square
    {
        public ControlNode topLeft, topRight, botRight, botLeft;
        public Node centerTop, centerRight, centerBottom, centerLeft;
        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _botRight, ControlNode _botLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            botRight = _botRight;
            botLeft = _botLeft;

            centerTop = topLeft.right;
            centerRight = botRight.above;
            centerBottom = botLeft.right;
            centerLeft = botLeft.above;

            if (topLeft.active) configuration += 8;
            if (topRight.active) configuration += 4;
            if (botRight.active) configuration += 2;
            if (botLeft.active) configuration += 1;
        }
    }//define quadrados com nós de controle nos vértices, e nós no meio das arestas
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _pos)
        {
            position = _pos;
        }
    }//pontos usado para gerar os triângulos das meshes
    public class ControlNode : Node
    {
        public bool active;
        public Node above, right;

        public ControlNode(Vector3 _pos, bool _active, float squareSize) : base(_pos){
            active = _active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
            }
    }//pontos usado para gerar os triângulos das meshes
    struct Triangle
    {
        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }


        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }//declaração auxiliar para determinação das meshes
}
