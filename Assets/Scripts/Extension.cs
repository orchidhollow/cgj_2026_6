using Sirenix.OdinInspector;
using System;
using System.Reflection;

/// <summary>
/// 通用扩展
/// </summary>
public static class Extension
{
    /// <summary>
    /// 获取枚举的LabelText特性值
    /// </summary>
    /// <param name="value">枚举</param>
    /// <returns>LabelText特性值</returns>
    public static string GetEnumLabel(this Enum value)
    {
        return value.GetType().GetField(value.ToString()).GetCustomAttribute<LabelTextAttribute>()?.Text;
    }
}