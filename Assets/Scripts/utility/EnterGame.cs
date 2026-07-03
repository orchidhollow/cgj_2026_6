using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterGame : MonoBehaviour
{
    // Start is called before the first frame update

    // 在Inspector面板中指定要加载的场景索引（比如第一关索引为1）

    // 目标场景索引，默认设为0
    [SerializeField] private int i = 0;

    // 按钮点击时触发的方法（需绑定到Button的OnClick事件）
    public void OnStartButtonClick()
        
    {
        int nextSceneIndex = (SceneManager.GetActiveScene().buildIndex + i)%6;
        // 加载目标场景（确保场景0已添加到Build Settings）
        SceneManager.LoadScene(nextSceneIndex);
    }
}