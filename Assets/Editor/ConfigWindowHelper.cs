// using System.Collections.Generic;
// using System.Linq;
// using Sirenix.OdinInspector.Editor;
// using Sirenix.Utilities.Editor;
// using UnityEditor;
// using UnityEngine;
//
//
//     public abstract class ConfigWindowHelper : OdinMenuEditorWindow
//     {
//         protected OdinMenuTree tree;
//
//         protected abstract void BuildTree();
//
//         protected override OdinMenuTree BuildMenuTree()
//         {
//             var customMenuStyle = new OdinMenuStyle();
//
//             tree = new OdinMenuTree(true);
//             tree.DefaultMenuStyle = customMenuStyle;
//             tree.Config.DrawScrollView = true;
//             tree.Config.DrawSearchToolbar = true;
//
//             BuildTree();
//
//             Preview(tree.MenuItems);
//
//             return tree;
//         }
//
//         protected override void OnBeginDrawEditors()
//         {
//             var selected = MenuTree.Selection.FirstOrDefault();
//             var toolbarHeight = MenuTree.Config.SearchToolbarHeight;
//
//             // Draws a toolbar with the name of the currently selected menu item.
//             SirenixEditorGUI.BeginHorizontalToolbar(toolbarHeight);
//             {
//                 if (selected != null)
//                 {
//                     GUILayout.Label(selected.Name);
//                 }
//
//                 if (SirenixEditorGUI.ToolbarButton(new GUIContent("全部保存【别乱点】")))
//                 {
//                     Save(MenuTree.MenuItems);
//                 }
//
//                 if (SirenixEditorGUI.ToolbarButton(new GUIContent("刷新")))
//                 {
//                     ForceMenuTreeRebuild();
//                 }
//             }
//             SirenixEditorGUI.EndHorizontalToolbar();
//         }
//
//         /// <summary>
//         /// 添加配置
//         /// </summary>
//         /// <param name="configOverview"></param>
//         /// <param name="menuFolder">编辑器目录项名称</param>
//         /// <param name="assetFolder">config下的文件夹名</param>
//         /// <param name="configHead">文件名</param>
//         /// <param name="hasCreator"></param>
//         protected virtual void AddAssets<T1, T2>(IConfigOverview<T1, T2> configOverview, string menuFolder, string assetFolder, string configHead,
//             bool hasCreator = true) where T2 : ConfigBase
//         {
//             if (hasCreator)
//             {
//                 //配置项前缀名为配置文件夹相对路径名最后一级名称
//                 var itemConfig = new ConfigCreator<T2>(assetFolder, configHead, configOverview.NextBaseConfigId);
//                 tree.Add(string.Format("{0}/{1}", menuFolder, $"#创建新{menuFolder}"), itemConfig);
//             }
//
//             foreach (var baseConfig in configOverview.AllValues)
//             {
//                 if (baseConfig == null) continue;
//
//                 var showConfigItemName = string.IsNullOrEmpty(baseConfig.mCommon)
//                     ? baseConfig.mID + "_" + configHead
//                     : baseConfig.mID + "_" + baseConfig.mCommon;
//
//                 var menu = string.Format("{0}/{1}", menuFolder, showConfigItemName);
//                 tree.Add(menu, baseConfig);
//             }
//         }
//
//         /// <summary>
//         /// 不需要多次创建，单独显示的配置表（任务奖励、新手教程）
//         /// </summary>
//         /// <param name="menuFolder">编辑器目录项名称</param>
//         /// <param name="assetPath"></param>
//         protected void AddAssets<T>(string menuFolder, string assetPath) where T : ScriptableObject
//         {
//             var config = AssetDatabase.LoadAssetAtPath<T>(assetPath);
//             if (config != null)
//             {
//                 tree.Add(menuFolder, config);
//             }
//             else
//             {
//                 Debug.LogError($"尝试加载{assetPath}时失败");
//             }
//         }
//
//         protected void AddAsset<T2>(string menuFolder, string assetFolder, string configHead, int id)
//             where T2 : ConfigBase
//         {
//             //配置项前缀名为配置文件夹相对路径名最后一级名称
//             var itemConfig = new ConfigCreator<T2>(assetFolder, configHead, id);
//             tree.Add(string.Format("{0}/{1}", menuFolder, $"#创建新{menuFolder}"), itemConfig);
//         }
//
//         private void Preview(List<OdinMenuItem> items)
//         {
//             foreach (var item in items)
//             {
//                 if (item.Value is ConfigBase config)
//                 {
//                     config.Preview();
//                 }
//
//                 if (item.ChildMenuItems != null)
//                 {
//                     Preview(item.ChildMenuItems);
//                 }
//             }
//         }
//
//         private void Save(List<OdinMenuItem> items)
//         {
//             foreach (var item in items)
//             {
//                 if (item.Value is ConfigBase config)
//                 {
//                     config.Save();
//                 }
//
//                 if (item.ChildMenuItems != null)
//                 {
//                     Save(item.ChildMenuItems);
//                 }
//             }
//         }
//     }