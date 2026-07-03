// using Sirenix.Utilities;
// using Sirenix.Utilities.Editor;
// using UnityEditor;
// using UnityEngine;
//
// public class ActionCardModuleConfigWindow : ConfigWindowHelper
// {
//     [MenuItem("Tools/编辑器/行动卡模块配置库")]
//     private static void Open()
//     {
//         var window = GetWindow<ActionCardModuleConfigWindow>();
//         window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1200, 600);
//
//         window.titleContent = new GUIContent("行动卡模块配置");
//         window.DrawUnityEditorPreview = true;
//     }
//
//     protected override void BuildTree()
//     {
//         ActionCardModuleOverview.Instance.UpdateConfigOverview();
//         AddAssets(ActionCardModuleOverview.Instance, "行动卡模块", "ActionCardModule", "ActionCardModuleConfig");
//         AddAssets<ActionCardNormalDataConfig>("行动卡通用配置",
//             "Assets/Resource/Config/ActionCardModule/NormalData/ActionCardNormalDataConfig.asset");
//     }
//     protected override void AddAssets<T1, T2>(
//         IConfigOverview<T1, T2> configOverview, 
//         string menuFolder,
//         string assetFolder, 
//         string configHead,
//         bool hasCreator = true)
//     {
//         if (hasCreator)
//         {
//             //配置项前缀名为配置文件夹相对路径名最后一级名称
//             var itemConfig = new ConfigCreator<T2>(assetFolder, configHead, configOverview.NextBaseConfigId);
//             tree.Add(string.Format("{0}/{1}", menuFolder, $"#创建新{menuFolder}"), itemConfig);
//         }
//
//         foreach (var baseConfig in configOverview.AllValues)
//         {
//             if (baseConfig == null) continue;
//
//             var showConfigItemName = string.IsNullOrEmpty(baseConfig.mCommon)
//                 ? baseConfig.mID + "_" + configHead
//                 : baseConfig.mID + "_" + baseConfig.mCommon;
//
//             var moduleConfig = baseConfig as ActionCardModuleConfig;
//             menuFolder = moduleConfig!.type.GetEnumLabel();
//
//             var menu = string.Format("{0}/{1}", menuFolder, showConfigItemName);
//             tree.Add(menu, baseConfig);
//         }
//     }
// }
