using System;
using Cysharp.Threading.Tasks;
using ParkMinPackages.Foundation.Extensions;
using ParkMinPackages.UGUI.Components;
using ParkMinPackages.UGUI.Components.UIActivatorAnimations;
using R3;
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
			root.Add(visualTree.CloneTree());

			serializedObject.Update();

			//초기화면 구현
			CollapseFoldout(root);
			ExpandAdditionalReactivePropertyFoldout(root);

			//필드 구현
			ImplementStartActiveStateToggle(root);
			ImplementVisbleToggle(root);
			ImplementRaycastableToggle(root);
			ImplementInteractableToggle(root);
			ImplementFadeSlider(root);
			ImplementDisableRaycastWhileAnimationToggle(root);

			//버튼 구현
			ImplementActiveDeactiveButton(root);

			//ReactiveProperty 구현
			ImplementActiveStateToggle(root);
			ImplementAnimationStateEnumField(root);
			ImplementActiveCompleteToggle(root);
			ImplementDeActiveCompleteToggle(root);
			ImplementDisplayStateToggle(root);

			//Additaionl ReactiveProperty Enable 구현
			ImplementEnableActiveStateInHierarchyReactivePropertyToggle(root);
			ImplementEnableActiveCompleteInHierarchyReactivePropertyToggle(root);
			ImplementEnableDeActiveCompleteInHierarchyReactivePropertyToggle(root);
			ImplementEnableDisplayStateInHierarchyReactivePropertyToggle(root);

			//Additaionl ReactiveProperty Value 구현
			ImplementActiveStateInHierarchyValueToggle(root);
			ImplementActiveCompleteInHierarchyValueToggle(root);
			ImplementDeActiveCompleteInHierarchyValueToggle(root);
			ImplementDisplayStateInHierarchyValueToggle(root);

			serializedObject.ApplyModifiedProperties();

			return root;
		}

		//초기화면 구현
		void CollapseFoldout(VisualElement root) {
			foreach (Foldout foldout in root.Query<Foldout>().ToList()) {
				foldout.value = false;
			}
		}
		void ExpandAdditionalReactivePropertyFoldout(VisualElement root) {
			Foldout additionalReactivePropertyFoldout = root.Q<Foldout>("AdditionalReactivePropertyFoldout");
			foreach (Foldout foldout in additionalReactivePropertyFoldout.Query<Foldout>().ToList()) {
				foldout.value = true;
			}
			additionalReactivePropertyFoldout.value = false;
		}

		//필드 구현
		void ImplementStartActiveStateToggle(VisualElement root) {
			root.Q<Toggle>("StartActiveStateToggle").BindProperty(serializedObject.FindProperty("_startActiveState"));
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
		void ImplementActiveDeactiveButton(VisualElement root) {
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

			root.Q<Button>("DeActiveButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				if (target is not UIActivator uiActivator)
					return;

				if (Application.isPlaying)
					uiActivator.DeActiveAsync(cancellationToken: Application.exitCancellationToken).Forget();
				else
					uiActivator.DeActiveImmediate();

				EditorUtility.SetDirty(uiActivator.gameObject);
			});

#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
			root.Q<Button>("DotweenAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UIDTFadeShowAnimation>();
				uiActivator.GetOrAddComponent<UIDTFadeHideAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("DotweenAddScaleButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UIDTScaleShowAnimation>();
				uiActivator.GetOrAddComponent<UIDTScaleHideAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("DotweenAddSlideButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UIDTSlideShowAnimation>();
				uiActivator.GetOrAddComponent<UIDTSlideHideAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
#endif

#if LITMOTION_SUPPORT
			root.Q<Button>("LitMotionAddFadeButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UILMFadeShowAnimation>();
				uiActivator.GetOrAddComponent<UILMFadeHideAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("LitMotionAddScaleButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UILMScaleShowAnimation>();
				uiActivator.GetOrAddComponent<UILMScaleHideAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
			root.Q<Button>("LitMotionAddSlideButton")?.RegisterCallback<ClickEvent>(_ =>
			{
				UIActivator uiActivator = target as UIActivator;
				uiActivator.GetOrAddComponent<UILMSlideShowAnimation>();
				uiActivator.GetOrAddComponent<UILMSlideHideAnimation>();
				EditorUtility.SetDirty(uiActivator.gameObject);
			});
#endif
		}

		//ReactiveProperty 구현
		void ImplementActiveStateToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("ActiveStateToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.ActiveState.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
		void ImplementAnimationStateEnumField(VisualElement root) {
			EnumField enumField = root.Q<EnumField>("AnimationStateEnumField");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.AnimationState_.Subscribe(value =>
			{
				enumField.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
		void ImplementActiveCompleteToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("ActiveCompleteToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.ActiveComplete.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
		void ImplementDeActiveCompleteToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("DeActiveCompleteToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.DeActiveComplete.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
		void ImplementDisplayStateToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("DisplayStateToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.DisplayState.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}

		//Additaionl ReactiveProperty Enable 구현
		void ImplementEnableActiveStateInHierarchyReactivePropertyToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("EnableActiveStateInHierarchyReactivePropertyToggle");
			toggle.BindProperty(serializedObject.FindProperty("_enableActiveStateInHierarchyReactiveProperty"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.EnableActiveStateInHierarchyReactiveProperty = evt.newValue;
			});
		}
		void ImplementEnableActiveCompleteInHierarchyReactivePropertyToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("EnableActiveCompleteInHierarchyReactivePropertyToggle");
			toggle.BindProperty(serializedObject.FindProperty("_enableActiveCompleteInHierarchyReactiveProperty"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.EnableActiveCompleteInHierarchyReactiveProperty = evt.newValue;
			});
		}
		void ImplementEnableDeActiveCompleteInHierarchyReactivePropertyToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("EnableDeActiveCompleteInHierarchyReactivePropertyToggle");
			toggle.BindProperty(serializedObject.FindProperty("_enableDeActiveCompleteInHierarchyReactiveProperty"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.EnableDeActiveCompleteInHierarchyReactiveProperty = evt.newValue;
			});
		}
		void ImplementEnableDisplayStateInHierarchyReactivePropertyToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("EnableDisplayStateInHierarchyReactivePropertyToggle");
			toggle.BindProperty(serializedObject.FindProperty("_enableDisplayStateInHierarchyReactiveProperty"));
			toggle.RegisterValueChangedCallback(evt =>
			{
				if (target is not UIActivator uiActivator)
					return;

				uiActivator.EnableDisplayStateInHierarchyReactiveProperty = evt.newValue;
				//EditorUtility.SetDirty(uiActivator.gameObject);
			});
		}

		//Additaionl ReactiveProperty Value 구현
		void ImplementActiveStateInHierarchyValueToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("ActiveStateInHierarchyValueToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.ActiveStateInHierarchy.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
		void ImplementActiveCompleteInHierarchyValueToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("ActiveCompleteInHierarchyValueToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.ActiveCompleteInHierarchy.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
		void ImplementDeActiveCompleteInHierarchyValueToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("DeActiveCompleteInHierarchyValueToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.DeActiveCompleteInHierarchy.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
		void ImplementDisplayStateInHierarchyValueToggle(VisualElement root) {
			Toggle toggle = root.Q<Toggle>("DisplayStateInHierarchyValueToggle");

			if (target is not UIActivator uiActivator)
				return;

			IDisposable disposable = uiActivator.DisplayStateInHierarchy.Subscribe(value =>
			{
				toggle.SetValueWithoutNotify(value);
			});

			root.RegisterCallback<DetachFromPanelEvent>(_ =>
			{
				disposable.Dispose();
			});
		}
	}
}