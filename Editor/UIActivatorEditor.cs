using Cysharp.Threading.Tasks;
using ParkMinPackages.Foundation.Extensions;
using ParkMinPackages.UGUI.Components;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ParkMinPackages.UGUI.Editor
{
	[CustomEditor(typeof(UIActivator))]
	public class UIActivatorEditor : UnityEditor.Editor
	{
		const string UxmlPath = "Packages/com.parkminpackages.ugui/Editor/UIActivatorEditor.uxml";

		public override VisualElement CreateInspectorGUI() {
			VisualElement root = new VisualElement();

			VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UxmlPath);
			if (visualTree == null) {
                root.Add(new HelpBox($"Could not load the UIActivator inspector layout at '{UxmlPath}'.", HelpBoxMessageType.Error));
                return root;
            }
            root.Add(visualTree.CloneTree());

			serializedObject.Update();

			//초기화면 구현
			CollapseFoldout(root);

			//필드 구현
			ImplementStartActiveStateToggle(root);
			ImplementStartLocalPosition(root);
			ImplementVisbleToggle(root);
			ImplementRaycastableToggle(root);
			ImplementInteractableToggle(root);
			ImplementFadeSlider(root);
			ImplementDisableRaycastWhileAnimationToggle(root);

			//버튼 구현
			ImplementActiveDeactivateButton(root);

			serializedObject.ApplyModifiedProperties();

			return root;
		}

		//초기화면 구현
		void CollapseFoldout(VisualElement root) {
			foreach (Foldout foldout in root.Query<Foldout>().ToList()) {
				foldout.value = false;
			}
		}
		//필드 구현
		void ImplementStartActiveStateToggle(VisualElement root) {
			root.Q<Toggle>("StartActiveStateToggle").BindProperty(serializedObject.FindProperty("_startActiveState"));
		}
		void ImplementStartLocalPosition(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("OverrideStartLocalPositionToggle");
			Vector3Field vector3Field = root.Q<Vector3Field>("StartLocalPositionVector3Field");
			SerializedProperty overrideStartLocalPositionProperty = serializedObject.FindProperty("_overrideStartLocalPosition");
			toggle.BindProperty(overrideStartLocalPositionProperty);
			vector3Field.BindProperty(serializedObject.FindProperty("_startLocalPosition"));
			vector3Field.style.display = overrideStartLocalPositionProperty.boolValue ? DisplayStyle.Flex : DisplayStyle.None;
			toggle.RegisterValueChangedCallback(evt =>
			{
				vector3Field.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None;
			});
		}
		void ImplementVisbleToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("VisibleToggle");
			toggle.BindProperty(serializedObject.FindProperty("_visible"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.Visible = evt.newValue;
			});
		}
		void ImplementRaycastableToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("RaycastableToggle");
			toggle.BindProperty(serializedObject.FindProperty("_raycastable"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.Raycastable = evt.newValue;
			});
		}
		void ImplementInteractableToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("InteractableToggle");
			toggle.BindProperty(serializedObject.FindProperty("_interactable"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.Interactable = evt.newValue;
			});
		}
		void ImplementFadeSlider(VisualElement root) {
			SerializedProperty fadeProp = serializedObject.FindProperty("_fade");

			Slider slider = root.Q<Slider>("FadeSlider");
			slider.BindProperty(fadeProp);
			slider.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.Fade = evt.newValue;
			});

			FloatField floatField = root.Q<FloatField>("FadeFloatField");
			floatField.BindProperty(fadeProp);
			floatField.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.Fade = evt.newValue;
			});
		}
		void ImplementDisableRaycastWhileAnimationToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("DisableRaycastWhileAnimationToggle");
			toggle.BindProperty(serializedObject.FindProperty("_disableRaycastWhileAnimation"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.DisableRaycastWhileAnimation = evt.newValue;
			});
		}

		//버튼 구현
		void ImplementActiveDeactivateButton(VisualElement root) {
			root.Q<Button>("ActiveButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				if (target is not UIActivator uiActivator)
					return;

				if (Application.isPlaying)
					uiActivator.ActiveAsync(cancellationToken: Application.exitCancellationToken).Forget();
				else
					uiActivator.ActiveImmediate();

				EditorUtility.SetDirty(uiActivator.gameObject);
			});

			root.Q<Button>("DeactivateButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				if (target is not UIActivator uiActivator)
					return;

				if (Application.isPlaying)
					uiActivator.DeactivateAsync(cancellationToken: Application.exitCancellationToken).Forget();
				else
					uiActivator.DeactivateImmediate();

				EditorUtility.SetDirty(uiActivator.gameObject);
			});

#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
			root.Q<Button>("DotweenAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.DOTweens.UIFadeActiveAnimation>();
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.DOTweens.UIFadeDeactivateAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("DotweenAddScaleButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.DOTweens.UIScaleActiveAnimation>();
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.DOTweens.UIScaleDeactivateAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("DotweenAddSlideButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.DOTweens.UISlideActiveAnimation>();
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.DOTweens.UISlideDeactivateAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
#endif

#if LITMOTION_SUPPORT
			root.Q<Button>("LitMotionAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.LitMotions.UIFadeActiveAnimation>();
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.LitMotions.UIFadeDeactivateAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("LitMotionAddScaleButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.LitMotions.UIScaleActiveAnimation>();
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.LitMotions.UIScaleDeactivateAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("LitMotionAddSlideButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.LitMotions.UISlideActiveAnimation>();
				uiActivator.GetOrAddComponent<Components.UIActivatorAnimations.LitMotions.UISlideDeactivateAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
#endif
		}
	}
}
