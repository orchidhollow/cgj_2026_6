using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerCg : MonoBehaviour
{
    [SerializeField] private CgFade cgFade;
    [SerializeField] private float loadDelay = 0.2f;
    [SerializeField] private int nextSceneIndex;
    [SerializeField] private AudioSource finishSource;
    public void Start()
    {
        nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (!c.CompareTag("Player")) return;

        // 禁用重复触发
        GetComponent<Collider2D>().enabled = false;
        finishSource.Play();

        // 播放 CG 过场，播完再切关
        cgFade.Play(() =>
        {
            SceneManager.LoadScene(nextSceneIndex);
            //Invoke("LoadNextScene", loadDelay);
        });
    }

    private void LoadNextScene()
{
    SceneManager.LoadScene(nextSceneIndex);
}
}
