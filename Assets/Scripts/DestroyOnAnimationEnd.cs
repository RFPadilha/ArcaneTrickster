using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//usado para alguns efeitos visuais, para que sejam removidos após a conclusão de sua animação
public class DestroyOnAnimationEnd : MonoBehaviour
{
    public void DestroyParent()
    {
        GameObject parent = gameObject.transform.parent.gameObject;
        Destroy(parent);
    }
}
