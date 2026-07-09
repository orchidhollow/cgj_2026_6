using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedup : MonoBehaviour
{
    private bool speedFlag = true;
    public void SwitchTimeScale()
    {
        speedFlag = !speedFlag;
    }

    private void Update()
    {
        // 按 E 键切换加速
        if (Input.GetKeyDown(KeyCode.E))
        {
            speedFlag = !speedFlag;
        }

        Time.timeScale = speedFlag ? 1 : 20;
    }
}
