using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//usado somente para garantir a permanência do GameObject "GameManager"
public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);//não destroi objeto na troca de cenas
    }    
}
