using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPS_Display : MonoBehaviour
{
    private TMP_Text text;
    void Start(){
        text = GetComponent<TMP_Text>();
    }
    // Update is called once per frame
    void Update()
    {
        float current = 0;
        current = (int)(1f / Time.unscaledDeltaTime);
        text.text = "FPS: " + current;
    }
}
