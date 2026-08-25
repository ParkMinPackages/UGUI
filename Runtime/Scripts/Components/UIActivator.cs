using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using ParkMinPackages.Foundation.Components;
using ParkMinPackages.Foundation.Constants;
using ParkMinPackages.Foundation.Extensions;
using ParkMinPackages.UGUI.Components.UIActivatorAnimations;
using ParkMinPackages.UGUI.Enums;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif
#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DOTweenAnimations = ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens;
#endif
#if LITMOTION_SUPPORT
using LitMotionAnimations = ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions;
#endif

namespace ParkMinPackages.UGUI.Components
{
	[RequireComponent(typeof(Canvas))]
	[RequireComponent(typeof(CanvasGroup))]
	[DefaultExecutionOrder(-100)]
	[DisallowMultipleComponent]
	public class UIActivator : TreeNode<UIActivator>
	{
		// ===================== Public API =====================

		public async UniTask ActiveAsync(
			CancellationToken cancellationToken = default,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete,
			bool throwIfTransitioning = true
		) {
			cancellationToken.ThrowIfCancellationRequested();
			if (_isTransitioning) {
				if (throwIfTransitioning) {
					throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
				}
				return;
			}
			if (_state == UIActivationState.Active) return;

			SetLayoutIgnored(false);
			ActiveAnimation[] activeAnimations = _activeAnimations.ToArray();
			Capture(activeAnimations);
			_isTransitioning = true;
			UpdateRaycastable();

			try {
				Canvas.enabled = false;
				UniTask[] animationTasks = ExecuteAnimations(activeAnimations, cancellationToken);
				Canvas.enabled = true;
				await UniTask.WhenAll(animationTasks);
				cancellationToken.ThrowIfCancellationRequested();
				Restore(activeAnimations);
				_state = UIActivationState.Active;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
				ApplyActiveCancellation(activeAnimations, animationCancelBehaviour);
				throw;
			}
			catch {
				Restore(activeAnimations);
				Canvas.enabled = false;
				throw;
			}
			finally {
				_isTransitioning = false;
				SetLayoutIgnored(_state == UIActivationState.Inactive);
				UpdateRaycastable();
			}
		}
		public async UniTask DeactivateAsync(
			CancellationToken cancellationToken = default,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete,
			bool throwIfTransitioning = true
		) {
			cancellationToken.ThrowIfCancellationRequested();
			if (_isTransitioning) {
				if (throwIfTransitioning) {
					throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
				}
				return;
			}
			if (_state == UIActivationState.Inactive) return;

			DeactivateAnimation[] deactivateAnimations = _deactivateAnimations.ToArray();
			Capture(deactivateAnimations);
			_isTransitioning = true;
			UpdateRaycastable();

			try {
				Canvas.enabled = true;
				UniTask[] animationTasks = ExecuteAnimations(deactivateAnimations, cancellationToken);
				await UniTask.WhenAll(animationTasks);
				cancellationToken.ThrowIfCancellationRequested();
				Canvas.enabled = false;
				Restore(deactivateAnimations);
				_state = UIActivationState.Inactive;
			}
			catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
				ApplyDeactivateCancellation(deactivateAnimations, animationCancelBehaviour);
				throw;
			}
			catch {
				Restore(deactivateAnimations);
				Canvas.enabled = true;
				throw;
			}
			finally {
				_isTransitioning = false;
				SetLayoutIgnored(_state == UIActivationState.Inactive);
				UpdateRaycastable();
			}
		}
		public void ActiveImmediate() {
			if (_isTransitioning) {
				throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
			}
			SetLayoutIgnored(false);
			Canvas.enabled = true;
			_state = UIActivationState.Active;
			UpdateRaycastable();
		}
		public void DeactivateImmediate() {
			if (_isTransitioning) {
				throw new InvalidOperationException($"{nameof(UIActivator)} '{name}' is already transitioning.");
			}
			Canvas.enabled = false;
			_state = UIActivationState.Inactive;
			SetLayoutIgnored(true);
			UpdateRaycastable();
		}

		public async UniTask ActiveWithChildrenAsync(
			CancellationToken cancellationToken = default,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete
		) {
			cancellationToken.ThrowIfCancellationRequested();
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			UniTask[] tasks = new UniTask[uiActivators.Length];

			for (int i = 0; i < uiActivators.Length; i++) {
				tasks[i] = uiActivators[i].ActiveAsync(cancellationToken, animationCancelBehaviour);
			}
			await UniTask.WhenAll(tasks);
		}
		public async UniTask DeactivateWithChildrenAsync(
			CancellationToken cancellationToken = default,
			AnimationCancelBehaviour animationCancelBehaviour = AnimationCancelBehaviour.Complete
		) {
			cancellationToken.ThrowIfCancellationRequested();
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			UniTask[] tasks = new UniTask[uiActivators.Length];

			for (int i = 0; i < uiActivators.Length; i++) {
				tasks[i] = uiActivators[i].DeactivateAsync(cancellationToken, animationCancelBehaviour);
			}
			await UniTask.WhenAll(tasks);
		}
		public void ActiveWithChildrenImmediate() {
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			for (int i = 0; i < uiActivators.Length; i++) {
				uiActivators[i].ActiveImmediate();
			}
		}
		public void DeactivateWithChildrenImmediate() {
			UIActivator[] uiActivators = ChildNodesEnumerable().ToArray();
			EnsureNoneTransitioning(uiActivators);
			for (int i = 0; i < uiActivators.Length; i++) {
				uiActivators[i].DeactivateImmediate();
			}
		}

		internal void RegisterActiveAnimation(ActiveAnimation activeAnimation) {
			_activeAnimations.Add(activeAnimation);
		}
		internal void UnregisterActiveAnimation(ActiveAnimation activeAnimation) {
			_activeAnimations.Remove(activeAnimation);
		}
		internal void RegisterDeactivateAnimation(DeactivateAnimation deactivateAnimation) {
			_deactivateAnimations.Add(deactivateAnimation);
		}
		internal void UnregisterDeactivateAnimation(DeactivateAnimation deactivateAnimation) {
			_deactivateAnimations.Remove(deactivateAnimation);
		}

		// ===================== Public Property =====================

		public bool Visible
		{
			get { return _visible; }
			set
			{
				_visible = value;
				UpdateVisibleAndFade();
			}
		}
		public bool Raycastable
		{
			get { return _raycastable; }
			set
			{
				_raycastable = value;
				UpdateRaycastable();
			}
		}
		public bool Interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				UpdateInteractable();
			}
		}
		public float Fade
		{
			get { return _fade; }
			set
			{
				_fade = Mathf.Clamp(value, 0.0f, 1.0f);
				UpdateVisibleAndFade();
			}
		}
		public bool DisableRaycastWhileAnimation
		{
			get { return _disableRaycastWhileAnimation; }
			set
			{
				_disableRaycastWhileAnimation = value;
				UpdateRaycastable();
			}
		}
		public UIAnimationUpdateMode UpdateMode
		{
			get { return _updateMode; }
			set { _updateMode = value; }
		}
		public bool IgnoreTimeScale
		{
			get { return _ignoreTimeScale; }
			set { _ignoreTimeScale = value; }
		}

		public UIActivationState State
		{
			get { return _state; }
		}
		public bool IsActive
		{
			get { return _state == UIActivationState.Active; }
		}
		public bool IsTransitioning
		{
			get { return _isTransitioning; }
		}

		// - Components -
		public CanvasGroup CanvasGroup
		{
			get
			{
				if (_canvasGroup == null) _canvasGroup = GetComponent<CanvasGroup>();
				return _canvasGroup;
			}
		}
		public Canvas Canvas
		{
			get
			{
				if (_canvas == null) _canvas = GetComponent<Canvas>();
				return _canvas;
			}
		}
		// ===================== Handlers =====================

		protected override void Awake() {
			base.Awake();

			if (_overrideStartLocalPosition) {
				transform.localPosition = _startLocalPosition;
			}

			Canvas.enabled = _startActiveState;
			_state = _startActiveState ? UIActivationState.Active : UIActivationState.Inactive;
			_layoutElement = GetComponent<LayoutElement>();
			SetLayoutIgnored(_state == UIActivationState.Inactive);
			_fade = Mathf.Clamp01(_fade);
			UpdateVisibleAndFade();
			UpdateInteractable();
			UpdateRaycastable();
		}
		// ===================== Internals =====================
		// - Components -
		CanvasGroup _canvasGroup;
		Canvas _canvas;
		LayoutElement _layoutElement;
		readonly List<ActiveAnimation> _activeAnimations = new List<ActiveAnimation>();
		readonly List<DeactivateAnimation> _deactivateAnimations = new List<DeactivateAnimation>();

		// - Core -
		[Title("Initial State")]
		[DisableInPlayMode, SerializeField] bool _startActiveState = true;
		[DisableInPlayMode, SerializeField] bool _overrideStartLocalPosition;
		[DisableInPlayMode, ShowIf(nameof(_overrideStartLocalPosition)), SerializeField] Vector3 _startLocalPosition;

		[Title("Visibility")]
		[OnValueChanged(nameof(UpdateVisibleAndFade)), SerializeField] bool _visible = true;
		[PropertyRange(0f, 1f), OnValueChanged(nameof(UpdateVisibleAndFade)), SerializeField] float _fade = 1f;

		[Title("Interaction")]
		[OnValueChanged(nameof(UpdateRaycastable)), SerializeField] bool _raycastable = true;
		[OnValueChanged(nameof(UpdateInteractable)), SerializeField] bool _interactable = true;
		[OnValueChanged(nameof(UpdateRaycastable)), SerializeField] bool _disableRaycastWhileAnimation = true;

		[Title("Layout")]
		[OnValueChanged(nameof(UpdateLayoutParticipation)), FormerlySerializedAs("_manageLayoutElement"), SerializeField] bool _excludeFromLayoutWhenInactive = true;

		[Title("Animation Update")]
		[SerializeField] UIAnimationUpdateMode _updateMode;
		[SerializeField] bool _ignoreTimeScale;

		UIActivationState _state;
		bool _isTransitioning;

		static void Capture(UIAnimation[] animations) {
			for (int i = 0; i < animations.Length; i++) {
				animations[i].Capture();
			}
		}
		static void Restore(UIAnimation[] animations) {
			for (int i = 0; i < animations.Length; i++) {
				animations[i].Restore();
			}
		}
		static UniTask[] ExecuteAnimations(
			UIAnimation[] animations,
			CancellationToken cancellationToken
		) {
			UniTask[] animationTasks = new UniTask[animations.Length];
			for (int i = 0; i < animations.Length; i++) {
				animationTasks[i] = animations[i].ExecuteAsync(cancellationToken);
			}
			return animationTasks;
		}
		void ApplyActiveCancellation(
			ActiveAnimation[] activeAnimations,
			AnimationCancelBehaviour animationCancelBehaviour
		) {
			Restore(activeAnimations);
			switch (animationCancelBehaviour) {
				case AnimationCancelBehaviour.Complete:
					Canvas.enabled = true;
					_state = UIActivationState.Active;
					break;
				case AnimationCancelBehaviour.ResetToStart:
					Canvas.enabled = false;
					_state = UIActivationState.Inactive;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(animationCancelBehaviour), animationCancelBehaviour, null);
			}
		}
		void ApplyDeactivateCancellation(
			DeactivateAnimation[] deactivateAnimations,
			AnimationCancelBehaviour animationCancelBehaviour
		) {
			Restore(deactivateAnimations);
			switch (animationCancelBehaviour) {
				case AnimationCancelBehaviour.Complete:
					Canvas.enabled = false;
					_state = UIActivationState.Inactive;
					break;
				case AnimationCancelBehaviour.ResetToStart:
					Canvas.enabled = true;
					_state = UIActivationState.Active;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(animationCancelBehaviour), animationCancelBehaviour, null);
			}
		}
		static void EnsureNoneTransitioning(UIActivator[] uiActivators) {
			for (int i = 0; i < uiActivators.Length; i++) {
				if (uiActivators[i]._isTransitioning) {
					throw new InvalidOperationException($"{nameof(UIActivator)} '{uiActivators[i].name}' is already transitioning.");
				}
			}
		}
		void UpdateVisibleAndFade() {
			CanvasGroup.alpha = _visible ? _fade : 0f;
		}
		void UpdateRaycastable() {
			CanvasGroup.blocksRaycasts =
				_raycastable &&
				(_disableRaycastWhileAnimation == false || _isTransitioning == false);
		}
		void UpdateInteractable() {
			CanvasGroup.interactable = _interactable;
		}
		void UpdateLayoutParticipation() {
			if (_layoutElement == null)
				_layoutElement = GetComponent<LayoutElement>();

			SetLayoutIgnored(_state == UIActivationState.Inactive);
		}
		void SetLayoutIgnored(bool ignored) {
			if (_layoutElement != null)
				_layoutElement.ignoreLayout = _excludeFromLayoutWhenInactive && ignored;
		}

#if UNITY_EDITOR
		[PropertyOrder(-100), ButtonGroup("Preview Utility", Order = -100), Button("Active"), DisableIf(nameof(IsTransitioning))] void ActiveFromInspector() {
			if (Application.isPlaying)
				ActiveAsync(cancellationToken: Application.exitCancellationToken).Forget();
			else
				ActiveImmediate();

			EditorUtility.SetDirty(gameObject);
		}
		[PropertyOrder(-100), ButtonGroup("Preview Utility", Order = -100), Button("Deactivate"), DisableIf(nameof(IsTransitioning))] void DeactivateFromInspector() {
			if (Application.isPlaying)
				DeactivateAsync(cancellationToken: Application.exitCancellationToken).Forget();
			else
				DeactivateImmediate();

			EditorUtility.SetDirty(gameObject);
		}

#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/DOTween")]
		[ButtonGroup("Add Animation Utility/DOTween/Buttons"), Button("Fade")] void AddDOTweenFadeAnimations() => AddAnimationPair<DOTweenAnimations.UIFadeActiveAnimation, DOTweenAnimations.UIFadeDeactivateAnimation>();
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/DOTween")]
		[ButtonGroup("Add Animation Utility/DOTween/Buttons"), Button("Scale")] void AddDOTweenScaleAnimations() => AddAnimationPair<DOTweenAnimations.UIScaleActiveAnimation, DOTweenAnimations.UIScaleDeactivateAnimation>();
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/DOTween")]
		[ButtonGroup("Add Animation Utility/DOTween/Buttons"), Button("Slide")] void AddDOTweenSlideAnimations() => AddAnimationPair<DOTweenAnimations.UISlideActiveAnimation, DOTweenAnimations.UISlideDeactivateAnimation>();
#endif
#if LITMOTION_SUPPORT
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Fade")] void AddLitMotionFadeAnimations() => AddAnimationPair<LitMotionAnimations.UIFadeActiveAnimation, LitMotionAnimations.UIFadeDeactivateAnimation>();
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Scale")] void AddLitMotionScaleAnimations() => AddAnimationPair<LitMotionAnimations.UIScaleActiveAnimation, LitMotionAnimations.UIScaleDeactivateAnimation>();
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Slide")] void AddLitMotionSlideAnimations() => AddAnimationPair<LitMotionAnimations.UISlideActiveAnimation, LitMotionAnimations.UISlideDeactivateAnimation>();
		[FoldoutGroup("Add Animation Utility", Order = 100)]
		[TitleGroup("Add Animation Utility/LitMotion")]
		[ButtonGroup("Add Animation Utility/LitMotion/Buttons"), Button("Move From")] void AddLitMotionMoveFromAnimations() => AddAnimationPair<LitMotionAnimations.UIMoveFromActiveAnimation, LitMotionAnimations.UIMoveFromDeactivateAnimation>();
#endif

		void AddAnimationPair<TActiveAnimation, TDeactivateAnimation>() where TActiveAnimation : Component where TDeactivateAnimation : Component {
			gameObject.GetOrAddComponent<TActiveAnimation>();
			gameObject.GetOrAddComponent<TDeactivateAnimation>();
			EditorUtility.SetDirty(gameObject);
		}
#endif
	}
}
