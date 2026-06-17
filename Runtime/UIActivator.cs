using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using com.mutant.ugui.UIAnimations;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace com.mutant.ugui
{
	[RequireComponent(typeof(Canvas))]
	[RequireComponent(typeof(CanvasGroup))]
	[RequireComponent(typeof(BaseRaycaster))]
	[DisallowMultipleComponent]
	public class UIActivator : TreeNode<UIActivator>
	{
		public enum State
		{
			ActiveAnimation,
			Active,
			DeactiveAnimation,
			DeActive,
		}

		// ===================== Public API =====================

		public async UniTask ActiveAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Cancel, CancellationToken cancellationToken = default) {
			if (forceExecute == false && (_state.Value == State.Active || _state.Value == State.ActiveAnimation)) {
				return;
			}

			ShowAnimation[] showAnimations = _showAnimations.ToArray(); // 원본 변경을 방지하기 위해 복사본

			if (showAnimations.Any() == false) {
				ActiveImmediate();
				return;
			}

			TryCancelAndDisposeAnimation();
			AllocateToRecentShowHideCTS(cancellationToken);

			try {
				_state.Value = State.ActiveAnimation;

				UniTask[] showAnimationTasks = new UniTask[showAnimations.Length];
				for (int i = 0; i < showAnimations.Length; i++) {
					int j = i;
					showAnimations[j].CaptureCurrent(); // 정상 상태 캡처
					showAnimationTasks[j] = showAnimations[j].ExecuteAsync(_recentShowHideCTS.Token);
				}

				await UniTask.WhenAll(showAnimationTasks);

				for (int i = 0; i < showAnimations.Length; i++) showAnimations[i].ApplyCaptured();
				_state.Value = State.Active;
			}
			catch (OperationCanceledException e) {
				for (int i = 0; i < showAnimations.Length; i++) showAnimations[i].ApplyCaptured();
				if (cancelBehaviour == CancelBehavior.Complete) {
					_state.Value = State.Active;
				}
				else if (cancelBehaviour == CancelBehavior.Cancel) {
					_state.Value = State.DeActive;
				}
				throw e;
			}
		}
		public async UniTask DeActiveAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Complete, CancellationToken cancellationToken = default) {
			if (forceExecute == false && (_state.Value == State.DeActive || _state.Value == State.DeactiveAnimation)) {
				return;
			}

			HideAnimation[] hideAnimations = _hideAnimations.ToArray(); // 원본 변경을 방지하기 위해 복사본

			if (hideAnimations.Any() == false) {
				DeActiveImmediate();
				return;
			}

			TryCancelAndDisposeAnimation();
			AllocateToRecentShowHideCTS(cancellationToken);

			try {
				_state.Value = State.DeactiveAnimation;

				UniTask[] hideAnimationTasks = new UniTask[hideAnimations.Length];
				for (int i = 0; i < hideAnimations.Length; i++) {
					int j = i;
					hideAnimations[j].CaptureCurrent(); // 정상 상태 캡처
					hideAnimationTasks[j] = hideAnimations[j].ExecuteAsync(_recentShowHideCTS.Token);
				}

				await UniTask.WhenAll(hideAnimationTasks);

				for (int i = 0; i < hideAnimations.Length; i++) hideAnimations[i].ApplyCaptured();
				_state.Value = State.DeActive;
			}
			catch (OperationCanceledException e) {
				for (int i = 0; i < hideAnimations.Length; i++) hideAnimations[i].ApplyCaptured();
				if (cancelBehaviour == CancelBehavior.Complete) {
					_state.Value = State.DeActive;
				}
				else if (cancelBehaviour == CancelBehavior.Cancel) {
					_state.Value = State.Active;
				}
				throw e;
			}
		}
		public void ActiveImmediate() {
			TryCancelAndDisposeAnimation();
			_state.Value = State.Active;
		}
		public void DeActiveImmediate() {
			TryCancelAndDisposeAnimation();
			_state.Value = State.DeActive;
		}
		public async UniTask ActiveAllChildAsync(float duration, bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Cancel, CancellationToken cancellationToken = default) {
			int count = ChildNodesEnumerable().Count();
			UniTask[] tasks = new UniTask[count];

			int i = 0;
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				tasks[i++] = eachUIActivator.ActiveAsync(forceExecute, cancelBehaviour, cancellationToken);
			}
			await UniTask.WhenAll(tasks);
		}
		public async UniTask DeActiveAllChildAsync(float duration, bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Cancel, CancellationToken cancellationToken = default) {
			int count = ChildNodesEnumerable().Count();
			UniTask[] tasks = new UniTask[count];

			int i = 0;
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				tasks[i++] = eachUIActivator.DeActiveAsync(forceExecute, cancelBehaviour, cancellationToken);
			}
			await UniTask.WhenAll(tasks);
		}
		public void DeActiveAllChildImmediate() {
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				eachUIActivator.ActiveImmediate();
			}
		}
		public void DeDeActiveAllChildImmediate() {
			foreach (UIActivator eachUIActivator in ChildNodesEnumerable()) {
				eachUIActivator.DeActiveImmediate();
			}
		}

#if UNITY_EDITOR
		public void ActiveInEditor() {
			SetVisibility(true);
			SetInteractable(true);
		}
		public void DeActiveInEditor() {
			SetVisibility(false);
			SetInteractable(false);
		}
#endif

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
		[ShowInInspector, HideInEditorMode]
#endif
		public bool Interactable
		{
			get { return _interactable; }
			set
			{
				print("Test");
				_interactable = value;
				SetInteractable(_readyInHieraraky.CurrentValue);
			}
		}

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
		public BaseRaycaster[] BaseRaycasters
		{
			get
			{
				if (_baseRaycasters == null) _baseRaycasters = GetComponents<BaseRaycaster>();
				return _baseRaycasters;
			}
		}
		public LayoutElement LayoutElement
		{
			get
			{
				if (_layoutElement == null) _layoutElement = GetComponent<LayoutElement>();
				return _layoutElement;
			}
		}

		public ReadOnlyReactiveProperty<State> State_
		{
			get { return _state; }
		}
		public ReadOnlyReactiveProperty<bool> Visibility
		{
			get { return _visibility; }
		}
		public ReadOnlyReactiveProperty<bool> ParentsVisibility
		{
			get { return _parentsVisibility; }
		}
		public ReadOnlyReactiveProperty<bool> VisibilityInHieraraky
		{
			get { return _visibilityInHieraraky; }
		}
		public ReadOnlyReactiveProperty<bool> Ready
		{
			get { return _ready; }
		}
		public ReadOnlyReactiveProperty<bool> ParentsReady
		{
			get { return _parentsReady; }
		}
		public ReadOnlyReactiveProperty<bool> ReadyInHieraraky
		{
			get { return _readyInHieraraky; }
		}
		public ReadOnlyReactiveProperty<bool> ActiveState
		{
			get { return _activeState; }
		}
		public ReadOnlyReactiveProperty<bool> ParentsActiveState
		{
			get { return _parentsActiveState; }
		}
		public ReadOnlyReactiveProperty<bool> ActiveStateInHieraraky
		{
			get { return _activeStateInHieraraky; }
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

		// ===================== Internals =====================

#if ODIN_INSPECTOR
		[HideInPlayMode]
#endif
		[SerializeField]
		bool _startActiveState = true;

#if ODIN_INSPECTOR
		[HideInPlayMode]
#endif
		[SerializeField]
		bool _interactable = true;

#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<State> _state = new SerializableReactiveProperty<State>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _visibility = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _parentsVisibility = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _visibilityInHieraraky = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _ready = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _parentsReady = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _readyInHieraraky = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _activeState = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _parentsActiveState = new SerializableReactiveProperty<bool>();
#if ODIN_INSPECTOR
		[SerializeField, FoldoutGroup("Debug", expanded: false), ReadOnly]
#endif
		SerializableReactiveProperty<bool> _activeStateInHieraraky = new SerializableReactiveProperty<bool>();

		CanvasGroup _canvasGroup;
		Canvas _canvas;
		BaseRaycaster[] _baseRaycasters;
		LayoutElement _layoutElement;

		List<ShowAnimation> _showAnimations = new List<ShowAnimation>();
		List<HideAnimation> _hideAnimations = new List<HideAnimation>();

		CompositeDisposable _compositeDisposable;
		CancellationTokenSource _recentShowHideCTS;

		protected override void Init() {
			base.Init();

			_compositeDisposable?.Dispose();
			_compositeDisposable = new CompositeDisposable();

			//핵심 변수들 업데이트
			foreach (UIActivator uiActivator in ParentNodesEnumerable()) {
				uiActivator._state.Subscribe(state =>
				{
					// //가시성 상태 업데이트
					// _visibility.Value = _state.Value == State.Active ||
					//                     _state.Value == State.ActiveAnimation ||
					//                     _state.Value == State.DeactiveAnimation;
					//
					// _parentsVisibility.Value = ParentNodesEnumerable(false).All(eachUIActivator =>
					// {
					// 	return eachUIActivator._state.Value == State.Active ||
					// 	       eachUIActivator._state.Value == State.ActiveAnimation ||
					// 	       eachUIActivator._state.Value == State.DeactiveAnimation;
					// });
					//
					// _visibilityInHieraraky.Value = _visibility.Value && _parentsVisibility.Value;
					//
					// //준비완료 상태 업데이트
					// _ready.Value = _state.Value == State.Active;
					//
					// _parentsReady.Value = ParentNodesEnumerable(false).All(eachUIActivator => eachUIActivator._state.Value == State.Active);
					//
					// _readyInHieraraky.Value = _ready.Value && _parentsReady.Value;
					//
					// //ActiveState 상태 업데이트
					// _activeState.Value = _state.Value == State.Active || _state.Value == State.ActiveAnimation;
					//
					// _parentsActiveState.Value = ParentNodesEnumerable(false).All(eachUIActivator => eachUIActivator._state.Value == State.Active || eachUIActivator._state.Value == State.ActiveAnimation);
					//
					// _activeStateInHieraraky.Value = _activeState.Value && _parentsActiveState.Value;

					// === 위 코드를 AI로 최적화한 코드 ===
					bool IsVisibleState(State s) {
						switch (s) {
							case State.Active:
							case State.ActiveAnimation:
							case State.DeactiveAnimation:
								return true;
							default:
								return false;
						}
					}

					bool IsReadyState(State s) {
						return s == State.Active;
					}

					bool IsActiveState(State s) {
						return s == State.Active || s == State.ActiveAnimation;
					}

					State selfState = _state.Value;

					bool selfVisible = IsVisibleState(selfState);
					bool selfReady = IsReadyState(selfState);
					bool selfActive = IsActiveState(selfState);

					List<UIActivator> parents = ParentNodesEnumerable(false).ToList();

					bool parentsVisible = true;
					bool parentsReady = true;
					bool parentsActive = true;

					foreach (UIActivator parent in parents) {
						State ps = parent._state.Value;

						if (parentsVisible && !IsVisibleState(ps)) parentsVisible = false;
						if (parentsReady && !IsReadyState(ps)) parentsReady = false;
						if (parentsActive && !IsActiveState(ps)) parentsActive = false;

						if (!parentsVisible && !parentsReady && !parentsActive) break;
					}

					_visibility.Value = selfVisible;
					_parentsVisibility.Value = parentsVisible;
					_visibilityInHieraraky.Value = selfVisible && parentsVisible;

					_ready.Value = selfReady;
					_parentsReady.Value = parentsReady;
					_readyInHieraraky.Value = selfReady && parentsReady;

					_activeState.Value = selfActive;
					_parentsActiveState.Value = parentsActive;
					_activeStateInHieraraky.Value = selfActive && parentsActive;
				}).AddTo(_compositeDisposable);
			}

			//Visibility 기능 구현
			_visibilityInHieraraky.Subscribe(SetVisibility).AddTo(_compositeDisposable);

			//Interactable 기능 구현
			_readyInHieraraky.Subscribe(SetInteractable).AddTo(_compositeDisposable);

			if (_startActiveState) {
				ActiveImmediate();
			}
			else {
				DeActiveImmediate();
			}
		}

		void AllocateToRecentShowHideCTS(CancellationToken cancellationToken) {
			_recentShowHideCTS = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken, cancellationToken);
		}
		void TryCancelAndDisposeAnimation() {
			if (_recentShowHideCTS != null) {
				_recentShowHideCTS.Cancel();
				_recentShowHideCTS.Dispose();
				_recentShowHideCTS = null;
			}
		}

		void SetInteractable(bool interactable) {
			CanvasGroup.blocksRaycasts = interactable && _interactable;
			CanvasGroup.interactable = interactable && _interactable;
			if (BaseRaycasters != null && BaseRaycasters.Length != 0) {
				foreach (BaseRaycaster baseRaycaster in BaseRaycasters) {
					baseRaycaster.enabled = interactable;
				}
			}
		}
		void SetVisibility(bool visibility) {
			if (Canvas) Canvas.enabled = visibility;

			if (LayoutElement) LayoutElement.ignoreLayout = !visibility;
		}
	}
}