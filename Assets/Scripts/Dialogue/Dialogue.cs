using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Classe para armazenar quem está falando no diálogo e quais frases serão ditas
[System.Serializable]
public class Dialogue {
    public string name;
    [TextArea(3,3)]
    public string[] sentences;
}
