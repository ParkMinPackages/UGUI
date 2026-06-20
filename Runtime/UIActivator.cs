using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using com.mutant.expansion;
using com.mutant.ugui.UIAnimations;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace com.mutant.ugui
{
	[RequireComponent(typeof(Canvas))]
	[RequireComponent(typeof(CanvasGroup))]
	[DefaultExecutionOrder(-100)]
	[DisallowMultipleComponent]
	public class UIActivator : TreeNode<UIActivator>
	{
		public enum AnimationState
		{
			Activating,
			ActiveComplete,
			Deactivating,
			DeactiveComplete,
		}

		// ===================== Public API =====================

		public async UniTask ActiveAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.RollBack, CancellationToken cancellationToken = default) {
			if (forceExecute == false && _activeState.CurrentValue) {
				return;
			}

			ShowAnimation[] showAnimations = _showAnimations.ToArray(); // 원본 변경을 방지하기 위해 복사본
			if (showAnimations.Any()) {
				TryCancelAndDisposeAnimation();
				AllocateToRecentShowHideCTS(cancellationToken);
				try {
					_activeState.Value = true;
					_animationState.Value = AnimationState.Activating;

					Canvas.enabled = true;
					UpdateRaycastable();

					UniTask[] showAnimationTasks = new UniTask[showAnimations.Length];
					for (int i = 0; i < showAnimations.Length; i++) {
						int j = i;
						showAnimations[j].CaptureCurrent(); // 정상 상태 캡처
						showAnimationTasks[j] = showAnimations[j].ExecuteAsync(_recentShowHideCTS.Token);
					}
					await UniTask.WhenAll(showAnimationTasks);

					_animationState.Value = AnimationState.ActiveComplete;
				}
				catch (OperationCanceledException e) {
					if (cancelBehaviour == CancelBehavior.RollBack) {
						DeActiveImmediate();
					}
					else if (cancelBehaviour == CancelBehavior.Complete) {
						ActiveImmediate();
					}
					throw e;
				}
				finally {
					for (int i = 0; i < showAnimations.Length; i++) showAnimations[i].ApplyCaptured();
					UpdateRaycastable();
				}
			}
			else {
				ActiveImmediate();
			}
		}
		public async UniTask DeActiveAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.RollBack, CancellationToken cancellationToken = default) {
			if (forceExecute == false && _activeState.CurrentValue == false) {
				return;
			}

			HideAnimation[] hideAnimations = _hideAnimations.ToArray(); // 원본 변경을 방지하기 위해 복사본

			if (hideAnimations.Any()) {
				TryCancelAndDisposeAnimation();
				AllocateToRecentShowHideCTS(cancellationToken);
				try {
					_activeState.Value = false;
					_animationState.Value = AnimationState.Deactivating;

					Canvas.enabled = true;
					UpdateRaycastable();

					UniTask[] hideAnimationTasks = new UniTask[hideAnimations.Length];
					for (int i = 0; i < hideAnimations.Length; i++) {
						int j = i;
						hideAnimations[j].CaptureCurrent(); // 정상 상태 캡처
						hideAnimationTasks[j] = hideAnimations[j].ExecuteAsync(_recentShowHideCTS.Token);
					}

					await UniTask.WhenAll(hideAnimationTasks);

					Canvas.enabled = false;

					_animationState.Value = AnimationState.DeactiveComplete;
				}
				catch (OperationCanceledException e) {
					if (cancelBehaviour == CancelBehavior.RollBack) {
						ActiveImmediate();
					}
					else if (cancelBehaviour == CancelBehavior.Complete) {
						DeActiveImmediate();
					}
					throw e;
				}
				finally {
					for (int i = 0; i < hideAnimations.Length; i++) hideAnimations[i].ApplyCaptured();
					UpdateRaycastable();
				}
			}
			else {
				DeActiveImmediate();
			}
		}
		public void ActiveImmediate() {
			TryCancelAndDisposeAnimation();
			Canvas.enabled = true;
			_activeState.Value = true;
			_animationState.Value = AnimationState.ActiveComplete;
		}
		public void DeActiveImmediate() {
			TryCancelAndDisposeAnimation();
			Canvas.enabled = false;
			_activeState.Value = false;
			_animationState.Value = AnimationState.DeactiveComplete;
		}

		public async UniTask ActiveWithChildrenAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.RollBack, CancellationToken cancellationToken = default) {
			int count = ChildNodesEnumerable().Count();
			UniTask[] tasks = new UniTask[count];

			int i = 0;
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				tasks[i++] = eachUIActivator.ActiveAsync(forceExecute, cancelBehaviour, cancellationToken);
			}
			await UniTask.WhenAll(tasks);
		}
		public async UniTask DeActiveWithChildrenAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Complete, CancellationToken cancellationToken = default) {
			int count = ChildNodesEnumerable().Count();
			UniTask[] tasks = new UniTask[count];

			int i = 0;
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				tasks[i++] = eachUIActivator.DeActiveAsync(forceExecute, cancelBehaviour, cancellationToken);
			}
			await UniTask.WhenAll(tasks);
		}
		public void ActiveWithChildrenImmediate() {
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				eachUIActivator.ActiveImmediate();
			}
		}
		public void DeActiveWithChildrenImmediate() {
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				eachUIActivator.DeActiveImmediate();
			}
		}

		public void RegisterShowAnimation(ShowAnimation showAnimation) {
			_showAnimations.Add(showAnimation);
		}
		public void UnregisterShowAnimation(ShowAnimation showAnimation) {
			_showAnimations.Remove(showAnimation);
		}
		public void RegisterHideAnimation(HideAnimation hideAnimation) {
			_hideAnimations.Add(hideAnimation);
		}
		public void UnRegisterHideAnimation(HideAnimation hideAnimation) {
			_hideAnimations.Remove(hideAnimation);
		}

		// ===================== Public Property =====================

#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool Visible
		{
			get { return _visible; }
			set
			{
				_visible = value;
				UpdateVisbleAndFade();
			}
		}
#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool Raycastable
		{
			get { return _raycastable; }
			set
			{
				_raycastable = value;
				UpdateRaycastable();
			}
		}

#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool Interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				CanvasGroup.interactable = _interactable;
			}
		}
#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public float Fade
		{
			get { return _fade; }
			set
			{
				_fade = Mathf.Clamp(value, 0.0f, 1.0f);
				UpdateVisbleAndFade();
			}
		}
#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool DisableRaycastWhileAnimation
		{
			get { return _disableRaycastWhileAnimation; }
			set { _disableRaycastWhileAnimation = value; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool EnableActiveStateInHierarchyVariable
		{
			get { return _enableActiveStateInHierarchyVariable; }
			set
			{
				_enableActiveStateInHierarchyVariable = value;
				if (Application.isPlaying == false) return;
				_activeStateInHierarchyDisposable?.Dispose();
				if (_enableActiveStateInHierarchyVariable) {
					IEnumerable<SerializableReactiveProperty<bool>> observables = ParentNodesEnumerable().Select(uiActivator => uiActivator._activeState);
					// 모든 부모가 true일때만 true
					_activeStateInHierarchyDisposable = Observable.CombineLatest(observables)
					                                              .Subscribe(values =>
					                                               {
						                                               _activeStateInHierarchy.Value = values.All(x => x);
					                                               });
				}
			}
		}
#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool EnableActiveCompleteInHierarchyVariable
		{
			get { return _enableActiveCompleteInHierarchyVariable; }
			set
			{
				_enableActiveCompleteInHierarchyVariable = value;
				if (Application.isPlaying == false) return;
				_activeCompleteInHierarchyDisposable?.Dispose();
				if (_enableActiveCompleteInHierarchyVariable) {
					IEnumerable<SerializableReactiveProperty<bool>> observables = ParentNodesEnumerable().Select(uiActivator => uiActivator._activeComplete);
					// 모든 부모가 true일때만 true
					_activeCompleteInHierarchyDisposable = Observable.CombineLatest(observables)
					                                                 .Subscribe(values =>
					                                                  {
						                                                  _activeCompleteInHierarchy.Value = values.All(x => x);
					                                                  });
				}
			}
		}
#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool EnableDeActiveCompleteInHierarchyVariable
		{
			get { return _enableDeActiveCompleteInHierarchyVariable; }
			set
			{
				_enableDeActiveCompleteInHierarchyVariable = value;
				if (Application.isPlaying == false) return;
				_deActiveCompleteInHierarchyDisposable?.Dispose();
				if (_enableDeActiveCompleteInHierarchyVariable) {
					IEnumerable<SerializableReactiveProperty<bool>> observables = ParentNodesEnumerable().Select(uiActivator => uiActivator._deActiveComplete);
					// 부모중 하나라도 true면 true
					_deActiveCompleteInHierarchyDisposable = Observable.CombineLatest(observables)
					                                                   .Subscribe(values =>
					                                                    {
						                                                    _deActiveCompleteInHierarchy.Value = values.Any(x => x);
					                                                    });
				}
			}
		}
#if ODIN_INSPECTOR
		[ShowInInspector]
#endif
		public bool EnableDisplayStateInHierarchyVariable
		{
			get { return _enableDisplayStateInHierarchyVariable; }
			set
			{
				_enableDisplayStateInHierarchyVariable = value;
				if (Application.isPlaying == false) return;
				_displayStateInHierarchyDisposable?.Dispose();
				if (_enableDisplayStateInHierarchyVariable) {
					IEnumerable<SerializableReactiveProperty<bool>> observables = ParentNodesEnumerable().Select(uiActivator => uiActivator._displayState);
					// 부모중 하나라도 false면 false, 모든 부모가 true일때만 true
					_displayStateInHierarchyDisposable = Observable.CombineLatest(observables)
					                                               .Subscribe(values =>
					                                                {
						                                                _displayStateInHierarchy.Value = values.All(x => x);
					                                                });
				}
			}
		}

#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> ActiveState
		{
			get { return _activeState; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<AnimationState> AnimationState_
		{
			get { return _animationState; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> ActiveComplete
		{
			get { return _activeComplete; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> DeActiveComplete
		{
			get { return _deActiveComplete; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> DisplayState
		{
			get { return _displayState; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> ActiveStateInHierarchy
		{
			get { return _activeStateInHierarchy; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> ActiveCompleteInHierarchy
		{
			get { return _activeCompleteInHierarchy; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> DeActiveCompleteInHierarchy
		{
			get { return _deActiveCompleteInHierarchy; }
		}
#if ODIN_INSPECTOR
		[ShowInInspector, HideInEditorMode]
#endif
		public ReadOnlyReactiveProperty<bool> DisplayStateInHierarchy
		{
			get { return _displayStateInHierarchy; }
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
		public IReadOnlyList<ShowAnimation> ShowAnimations
		{
			get { return _showAnimations; }
		}
		public IReadOnlyList<HideAnimation> HideAnimations
		{
			get { return _hideAnimations; }
		}

		// ===================== Handlers =====================

		protected override void Awake() {
			base.Awake();

			if (_startActiveState) {
				ActiveImmediate();
			}
			else {
				DeActiveImmediate();
			}
			Visible = _visible;
			Raycastable = _raycastable;
			Interactable = _interactable;
			Fade = _fade;
			DisableRaycastWhileAnimation = _disableRaycastWhileAnimation;

			//Ready
			_activeState.CombineLatest(_animationState, (active, animationState) => active && animationState == AnimationState.ActiveComplete)
			            .Subscribe(activeComplete =>
			             {
				             _activeComplete.Value = activeComplete;
			             })
			            .AddTo(gameObject);

			_activeState.CombineLatest(_animationState, (active, animationState) => active == false && animationState == AnimationState.DeactiveComplete)
			            .Subscribe(deActiveComplete =>
			             {
				             _deActiveComplete.Value = deActiveComplete;
			             })
			            .AddTo(gameObject);

			_animationState.Subscribe(animationState =>
			{
				switch (animationState) {
					case AnimationState.Activating:
					case AnimationState.ActiveComplete:
					case AnimationState.Deactivating:
						_displayState.Value = true;
						break;
					case AnimationState.DeactiveComplete:
						_displayState.Value = false;
						break;
				}
			}).AddTo(gameObject);
		}
		protected override void Start() {
			base.Start();
			UpdateStateInHierarchyVariables(); // 부모를 고려해야하는 함수 이므로 TreeNode 초기화 후에 사용
		}
		protected override void OnTreeNodeInited() {
			if (IsStarted) {
				UpdateStateInHierarchyVariables(); // 부모를 고려해야하는 함수 이므로 TreeNode 상태 변경될때도 사용
			}
		}


		// ===================== Internals =====================
		// - Components -
		CanvasGroup _canvasGroup;
		Canvas _canvas;
		List<ShowAnimation> _showAnimations = new List<ShowAnimation>();
		List<HideAnimation> _hideAnimations = new List<HideAnimation>();

		// - Core -
#if ODIN_INSPECTOR
		[HideInPlayMode]
#endif
		[SerializeField] bool _startActiveState = true;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _visible = true;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _raycastable = true;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _interactable = true;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField, Range(0.0f, 1.0f)] float _fade = 1f;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _disableRaycastWhileAnimation = true;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _enableActiveStateInHierarchyVariable;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _enableActiveCompleteInHierarchyVariable;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _enableDeActiveCompleteInHierarchyVariable;
#if ODIN_INSPECTOR
		[HideInInspector]
#endif
		[SerializeField] bool _enableDisplayStateInHierarchyVariable;

		SerializableReactiveProperty<bool> _activeState = new SerializableReactiveProperty<bool>();
		SerializableReactiveProperty<AnimationState> _animationState = new SerializableReactiveProperty<AnimationState>();
		SerializableReactiveProperty<bool> _activeComplete = new SerializableReactiveProperty<bool>();
		SerializableReactiveProperty<bool> _deActiveComplete = new SerializableReactiveProperty<bool>();
		SerializableReactiveProperty<bool> _displayState = new SerializableReactiveProperty<bool>(); //실질적으로 눈에 보이는지

		SerializableReactiveProperty<bool> _activeStateInHierarchy = new SerializableReactiveProperty<bool>(); // 모든 부모가 true일때만 true
		SerializableReactiveProperty<bool> _activeCompleteInHierarchy = new SerializableReactiveProperty<bool>(); // 모든 부모가 true일때만 true
		SerializableReactiveProperty<bool> _deActiveCompleteInHierarchy = new SerializableReactiveProperty<bool>(); // 부모중 하나라도 true면 true
		SerializableReactiveProperty<bool> _displayStateInHierarchy = new SerializableReactiveProperty<bool>(); // 부모중 하나라도 false면 false, 모든 부모가 true일때만 true

		IDisposable _activeStateInHierarchyDisposable;
		IDisposable _activeCompleteInHierarchyDisposable;
		IDisposable _deActiveCompleteInHierarchyDisposable;
		IDisposable _displayStateInHierarchyDisposable;

		void UpdateVisbleAndFade() {
			if (_visible) {
				CanvasGroup.alpha = _fade;
			}
			else {
				CanvasGroup.alpha = 0f;
			}
		}
		void UpdateRaycastable() {
			if (_disableRaycastWhileAnimation) {
				if (_animationState.Value == AnimationState.Activating || _animationState.Value == AnimationState.Deactivating) {
					CanvasGroup.blocksRaycasts = false;
				}
				else {
					CanvasGroup.blocksRaycasts = _raycastable;
				}
			}
			else {
				CanvasGroup.blocksRaycasts = _raycastable;
			}
		}
		void UpdateStateInHierarchyVariables() { // 부모를 고려해야하는 함수 이므로 TreeNode 초기화 후에 사용해야함
			EnableActiveStateInHierarchyVariable = _enableActiveStateInHierarchyVariable;
			EnableActiveCompleteInHierarchyVariable = _enableActiveCompleteInHierarchyVariable;
			EnableDeActiveCompleteInHierarchyVariable = _enableDeActiveCompleteInHierarchyVariable;
			EnableDisplayStateInHierarchyVariable = _enableDisplayStateInHierarchyVariable;
		}

		// - CancellationToken Utility -
		CancellationTokenSource _recentShowHideCTS;
		void AllocateToRecentShowHideCTS(CancellationToken cancellationToken) {
			_recentShowHideCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken);
		}
		void TryCancelAndDisposeAnimation() {
			if (_recentShowHideCTS != null) {
				CancellationTokenSource cts = _recentShowHideCTS;
				_recentShowHideCTS = null;
				cts.Cancel();
				cts.Dispose();
			}
		}
	}
}