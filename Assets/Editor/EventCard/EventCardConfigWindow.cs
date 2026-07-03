// using Sirenix.Utilities;
// using Sirenix.Utilities.Editor;
// using UnityEditor;
// using UnityEngine;
//
// public class EventCardConfigWindow : ConfigWindowHelper
// {
//     [MenuItem("Tools/编辑器/事件卡配置库")]
//     private static void Open()
//     {
//         var window = GetWindow<EventCardConfigWindow>();
//         window.position = GUIHelper.GetEditorWindowRect().AlignCenter(1200, 600);
//
//         window.titleContent = new GUIContent("事件卡配置");
//         window.DrawUnityEditorPreview = true;
//     }
//
//     protected override void BuildTree()
//     {
//         EventCardOverview.Instance.UpdateConfigOverview();
//         AddAssets(EventCardOverview.Instance, "事件卡", "EventCard", "EventCardConfig");
//     }
// }