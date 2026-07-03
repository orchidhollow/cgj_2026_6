using UnityEngine;

public class HealthPickups : MonoBehaviour
{
    [SerializeField] private int spikeValue = 1;   // 1 补血，-1 扣血
    [SerializeField] private HeartHUD heartHUD;   // 拖血量管理器

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        heartHUD.ChangeHP(spikeValue);
        Destroy(gameObject);          // 吃掉就消失
    }
}
