using UnityEngine;
using UnityEngine.SceneManagement;

public class HeartHUD : MonoBehaviour
{
    [Header("三对容器（仅拖父节点）")]
    [SerializeField] private GameObject[] heartPairs; // 长度=3，拖 Pair0/1/2

    private int maxHP = 3;
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
        UpdateHearts();
    }

    public void ChangeHP(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        UpdateHearts();
        if (currentHP <= 0) Invoke(nameof(Restart), 1f);
    }

    void UpdateHearts()
    {
        for (int i = 0; i < heartPairs.Length; i++)
        {
            bool alive = i < currentHP;
            heartPairs[i].transform.GetChild(0).gameObject.SetActive(alive);   // Full
            heartPairs[i].transform.GetChild(1).gameObject.SetActive(!alive);  // Empty
        }
    }

    void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
}
