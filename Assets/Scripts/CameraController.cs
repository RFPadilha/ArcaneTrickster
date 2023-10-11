using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;//precisa de acesso as cenas

public class CameraController : MonoBehaviour
{
    public GameObject target;//Alvo até o momento é sempre jogador
    public GameObject camHolder;//Objeto que move e orienta a câmera, necessário para "screen-shake"

    public Vector2 minPosition, maxPosition;//variáveis para limitar posicionamento da câmera
    public Vector2 minPosArena, maxPosArena;
    public float smoothing;

    PersistentStats stats;
    NecroBoss bossFight;

    bool bound = false;//variáveis de controle de execução
    bool done = false;

    private void Start()
    {
        stats = FindObjectOfType<PersistentStats>();
    }
    /* Execução constante desse trecho no update garante que a câmera receberá os acessos e referências que necessita
     * usando as variáveis de controle para que não executem sem necessidade, dado que os acessos precisam ser feitos uma vez só
     */
    
    void LateUpdate()
    {
        if(SceneManager.GetActiveScene().name == "MapGeneration" && !bound)
        {
            bossFight = FindObjectOfType<NecroBoss>();
            minPosition = new Vector2(-stats.width/2, -stats.height/2);
            maxPosition = new Vector2(stats.width/2, stats.height/2);
            bound = true;
        }
        
        if (!done && FindObjectOfType<PlayerScript>() != null)
        {
            target = FindObjectOfType<PlayerScript>().gameObject;
            camHolder.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, camHolder.transform.position.z);
            done = true;
        }

        
        if (done)//somente após receber referências do jogador
        {
            if (transform.position != target.transform.position)
            {
                Vector3 targetPosition = new Vector3(target.transform.position.x, target.transform.position.y, transform.position.z);
                if (bossFight == null)
                {
                    targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);
                }
                else if (bossFight.activeFight)
                {
                    targetPosition.x = Mathf.Clamp(targetPosition.x, minPosArena.x, maxPosArena.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, minPosArena.y, maxPosArena.y);
                }
                //Lerp é usado para movimentações graduais
                camHolder.transform.position = Vector3.Lerp(transform.position, targetPosition, smoothing);
            }
        }
        
    }
    //função de controle do efeito "screen-shake"
    public IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }
}
