using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class loadlevel2 : MonoBehaviour
{ 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float loadDelay = 0.2f; // 0.2-second delay before loading
    [SerializeField] private int nextSceneIndex;
    [Tooltip("拖所有需要暂停的 Animator")]

    public void Start()
    {
        nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        SceneManager.LoadScene(nextSceneIndex);
    }
}
