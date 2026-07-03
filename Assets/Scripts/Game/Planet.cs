using System.Collections;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public float gravityStrength = 15f;
    public float rotateSpeed = 10f;

    void Update()
    {
        transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponent<Player>();
        if (p != null)
        {
            p.targetPlanet = this;
            // 延迟一帧设置父物体，避免在激活/禁用过程中报错
            StartCoroutine(SetParentNextFrame(other.transform, transform));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponent<Player>();
        if (p != null && p.targetPlanet == this)
        {
            p.targetPlanet = null;
            // 延迟一帧清除父物体，避免在激活/禁用过程中报错
            StartCoroutine(SetParentNextFrame(other.transform, null));
        }
    }

    IEnumerator SetParentNextFrame(Transform child, Transform parent)
    {
        yield return null;
        if (child != null)
            child.SetParent(parent);
    }
}
