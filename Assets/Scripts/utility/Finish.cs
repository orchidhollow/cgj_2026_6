using UnityEngine;
using UnityEngine.SceneManagement;

public class Finish : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float loadDelay = 0.2f; // 0.2-second delay before loading
    [SerializeField] private int nextSceneIndex;
    [Tooltip("拖所有需要暂停的 Animator")]
    [SerializeField] private Animator[] targets;
    [SerializeField] private CgFade cgFade;

    public void Start()
    {
        nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        foreach (var anim in targets)          // 一键全停
            anim.speed = 0f;
        if (!other.CompareTag("Player")) return;
        else
        {
            GetComponent<Collider2D>().enabled = false;

            // 播放 CG 过场，播完再切关
            cgFade.Play(() =>
            {
                SceneManager.LoadScene("NextLevel");
                // [Key edit 1] Hide the finish object immediately (re-usable).
                //Invoke("LoadNextScene", loadDelay);

                //gameObject.SetActive(false);
                // Optional: replace with Destroy(gameObject) if you never want to reuse it.

                //Debug.Log("Player reached finish. Finish object hidden; loading next level in " + loadDelay + " seconds.");
                // Delay-load the next scene
                //Invoke("LoadNextScene", loadDelay);
            });
        }
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneIndex);
    }
}