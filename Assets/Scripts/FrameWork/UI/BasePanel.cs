using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI面板基类
/// 自动查找子控件并绑定事件
/// </summary>
public abstract class BasePanel : MonoBehaviour
{
    protected Dictionary<string, UIBehaviour> controlDic = new Dictionary<string, UIBehaviour>();

    /// <summary>
    /// 默认控件名列表，这些名称的控件不会被自动注册
    /// </summary>
    private static List<string> defaultNameList = new List<string>()
    {
        "Image",
        "Text (TMP)",
        "RawImage",
        "Background",
        "Checkmark",
        "Label",
        "Text (Legacy)",
        "Arrow",
        "Placeholder",
        "Fill",
        "Handle",
        "Viewport",
        "Scrollbar Horizontal",
        "Scrollbar Vertical"
    };

    protected virtual void Awake()
    {
        FindChildrenControl<Button>();
        FindChildrenControl<Toggle>();
        FindChildrenControl<Slider>();
        FindChildrenControl<InputField>();
        FindChildrenControl<ScrollRect>();
        FindChildrenControl<Dropdown>();
        FindChildrenControl<Text>();
        FindChildrenControl<TextMeshPro>();
        FindChildrenControl<Image>();
    }

    /// <summary>
    /// 面板显示时调用
    /// </summary>
    public abstract void ShowMe();

    /// <summary>
    /// 面板隐藏时调用
    /// </summary>
    public abstract void HideMe();

    /// <summary>
    /// 获取指定名称和类型的控件
    /// </summary>
    public T GetControl<T>(string name) where T : UIBehaviour
    {
        if (controlDic.ContainsKey(name))
        {
            T control = controlDic[name] as T;
            if (control == null)
            {
                Debug.LogError($"存在对应名称{name}，但不是{typeof(T)}类型的控件");
            }
            return controlDic[name] as T;
        }
        else
        {
            Debug.LogError($"不存在对应名称{name}的控件");
            return null;
        }
    }

    protected virtual void ClickBtn(string btnName) { }

    protected virtual void SliderValueChange(string SliderName, float value) { }

    protected virtual void ToggleValueChange(string SliderName, bool value) { }

    private void FindChildrenControl<T>() where T : UIBehaviour
    {
        T[] controls = GetComponentsInChildren<T>();
        for (int i = 0; i < controls.Length; i++)
        {
            string controlName = controls[i].gameObject.name;

            if (!controlDic.ContainsKey(controlName))
            {
                if (!defaultNameList.Contains(controlName))
                {
                    controlDic.Add(controlName, controls[i]);

                    if (controls[i] is Button)
                    {
                        (controls[i] as Button).onClick.AddListener(() =>
                        {
                            ClickBtn(controlName);
                        });
                    }
                    else if (controls[i] is Slider)
                    {
                        (controls[i] as Slider).onValueChanged.AddListener((value) =>
                        {
                            SliderValueChange(controlName, value);
                        });
                    }
                    else if (controls[i] is Toggle)
                    {
                        (controls[i] as Toggle).onValueChanged.AddListener((value) =>
                        {
                            ToggleValueChange(controlName, value);
                        });
                    }
                }
            }
        }
    }
}
