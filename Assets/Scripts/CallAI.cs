using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CallAI : MonoBehaviour{

    private string jsonDownloaded;
    private List<object> data_values_regression = new List<object>();
    public float[] data = new float[12];
    public Predictions predicted = new Predictions();
    private PersistentStats stats;

    private void Start() {
        stats = FindObjectOfType<PersistentStats>();
    }

    public void CallAPI(){
        StartCoroutine(JsonReader());
    }

    public IEnumerator JsonReader(){

        //float[] data1 = {100f,62f,477f,0f,0.210526f,0.700000f,0f,1f,33.681550f,32.242820f,4f,4f};

        string data_api = BuildString(data);
    
        //string url = "https://python-integration.herokuapp.com/regression";
        string url = "http://127.0.0.1:5000/regression";
        WWWForm form = new WWWForm();

        form.AddField("key", data_api.ToString());
        
        using(UnityWebRequest request = UnityWebRequest.Post(url, form)){
            yield return request.SendWebRequest();
            if(request.isNetworkError) Debug.Log(request.error);
            else{
                jsonDownloaded = request.downloadHandler.text;
                Debug.Log(jsonDownloaded);
                predicted = JsonConvert.DeserializeObject<Predictions>(jsonDownloaded);
                stats.SetNewGeneratedMap(predicted);
            }
        }
    }

    private string BuildString(float[] data){
        string x = string.Join(" ", data);
        string y = x.Replace(',', '.');
        string z = y.Replace(' ', ',');
        return z;
    }

}

[System.Serializable]
public class Predictions{
    public float[][] predictions;
}
