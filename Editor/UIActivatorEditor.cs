using com.mutant.expansion.Extensions;
using com.mutant.ugui.UIAnimations;
using Cysharp.Threading.Tasks;
using UnityEditor;
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
			VisualElement root = new VisualElement();

			// 기존 인스펙터 그대로 출력
#if ODIN_INSPECTOR
			IMGUIContainer odinInspector = new IMGUIContainer(() =>
			{
				serializedObject.Update();

				base.OnInspectorGUI();

				serializedObject.ApplyModifiedProperties();
			});

			root.Add(odinInspector);
#else
			UnityEditor.UIElements.InspectorElement.FillDefaultInspector(root, serializedObject, this);
#endif

			// 추가 UI를 UXML로 로드
			VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);

			if (visualTree == null) {
				root.Add(new HelpBox(
					$"UXML file not found: {UxmlPath}",
					HelpBoxMessageType.Warning));

				return root;
			}

			TemplateContainer extraUi = visualTree.CloneTree();
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

				if (Application.isPlaying)
					uiActivator.ActiveAsync(cancellationToken: Application.exitCancellationToken).Forget();
				else
					uiActivator.ActiveImmediate();
			});

			root.Q<Button>("DeActiveButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				if (target is not UIActivator uiActivator)
					return;

				if (Application.isPlaying)
					uiActivator.DeActiveAsync(cancellationToken: Application.exitCancellationToken).Forget();
				else
					uiActivator.DeActiveImmediate();
			});

#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
			root.Q<Button>("DotweenAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UIDTFadeShowAnimation>();
				uiActivator.GetOrAddComponent<UIDTFadeHideAnimation>();
			});
			root.Q<Button>("DotweenAddScaleButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UIDTScaleShowAnimation>();
				uiActivator.GetOrAddComponent<UIDTScaleHideAnimation>();
			});
			root.Q<Button>("DotweenAddSlideButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UIDTSlideShowAnimation>();
				uiActivator.GetOrAddComponent<UIDTSlideHideAnimation>();
			});
#endif

#if LITMOTION_SUPPORT
			root.Q<Button>("LitMotionAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UILMFadeShowAnimation>();
				uiActivator.GetOrAddComponent<UILMFadeHideAnimation>();
			});
			root.Q<Button>("LitMotionAddScaleButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UILMScaleShowAnimation>();
				uiActivator.GetOrAddComponent<UILMScaleHideAnimation>();
			});
			root.Q<Button>("LitMotionAddSlideButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UILMSlideShowAnimation>();
				uiActivator.GetOrAddComponent<UILMSlideHideAnimation>();
			});
#endif
		}
	}
}