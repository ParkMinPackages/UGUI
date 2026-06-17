using com.mutant.ugui.UIAnimations;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.mutant.ugui.Editor
{
	[CustomEditor(typeof(UIActivator))]
	public class UIActivatorEditor :
#if ODIN_INSPECTOR
		Sirenix.OdinInspector.Editor.OdinEditor
#else
		UnityEditor.Editor
#endif
	{
		const string UxmlPath = "Packages/com.mutant.ugui/Editor/UIActivatorEditor.uxml";

		public override VisualElement CreateInspectorGUI() {
			var root = new VisualElement();

			// 기존 인스펙터 그대로 출력
#if ODIN_INSPECTOR
			var odinInspector = new IMGUIContainer(() =>
			{
				serializedObject.Update();

				base.OnInspectorGUI();

				serializedObject.ApplyModifiedProperties();
			});

			root.Add(odinInspector);
#else
			InspectorElement.FillDefaultInspector(root, serializedObject, this);
#endif
			
			// 추가 UI를 UXML로 로드
			var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);

			if (visualTree == null) {
				root.Add(new HelpBox(
					$"UXML file not found: {UxmlPath}",
					HelpBoxMessageType.Warning));

				return root;
			}

			var extraUi = visualTree.CloneTree();
			root.Add(extraUi);

			foreach (Foldout foldout in root.Query<Foldout>().ToList()) {
				foldout.value = false;
			}

			RegisterButtons(extraUi);

			return root;
		}

		void RegisterButtons(VisualElement root) {
			root.Q<Button>("ActiveButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				if (target is not UIActivator uiActivator)
					return;

				if (Application.isPlaying) {
					uiActivator.ActiveAsync(
						cancellationToken: Application.exitCancellationToken).Forget();
				}
				else {
					uiActivator.ActiveInEditor();
				}
			});

			root.Q<Button>("DeActiveButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				if (target is not UIActivator uiActivator)
					return;

				if (Application.isPlaying) {
					uiActivator.DeActiveAsync(
						cancellationToken: Application.exitCancellationToken).Forget();
				}
				else {
					uiActivator.DeActiveInEditor();
				}
			});

			root.Q<Button>("DotweenAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
				if (target is not UIActivator uiActivator)
					return;

				Undo.AddComponent<UIDTFadeShowAnimation>(uiActivator.gameObject);
				Undo.AddComponent<UIDTFadeHideAnimation>(uiActivator.gameObject);
#endif
			});

			root.Q<Button>("LitMotionAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
#if LITMOTION_SUPPORT
				if (target is not UIActivator uiActivator)
					return;

				Undo.AddComponent<UILMFadeShowAnimation>(uiActivator.gameObject);
				Undo.AddComponent<UILMFadeHideAnimation>(uiActivator.gameObject);
#endif
			});
		}
	}
}