using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//script usado para transportar jogador e mudar posição da câmera sem mudar de cena, exemplo de transição vertical a norte (cima, botão W, eixo Y) da cidade na cena "Game"

public class AreaTransitions : MonoBehaviour
{
    private CameraController cam;
    public Vector2 newCamMinPos, newCamMaxPos;//âncoras da câmera
    public Vector3 movePlayer;//curta movimentação do jogador, definido no editor visual do unity (Inspector)
    void Start()
    {
        cam = Camera.main.GetComponent<CameraController>();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cam.minPosition = newCamMinPos;
            cam.maxPosition = newCamMaxPos;
            other.transform.position += movePlayer;
        }
    }
}
