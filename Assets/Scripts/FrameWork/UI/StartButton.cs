using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class StartButton : MonoBehaviour
{
    // Start is called before the first frame update
    private Button button;
    void Awake()
    {
         button = GetComponent<Button>();
        if(button == null)
        {
            Debug.LogError("StartButton : Button component not found!");
            return;
        }
        button.onClick.AddListener(GameManager.Instance.OnGameStart);
    }
    void Start()
    {
       
    }
    void OnDestroy()
    {
        if(button == null)
        {
            Debug.LogError("StartButton : Button component not found!");
            return;
        }
        button.onClick.RemoveAllListeners();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
