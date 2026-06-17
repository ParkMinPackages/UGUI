using UnityEngine;
using UnityEngine.UI;

namespace com.mutant.ugui
{
	public class GameObjectTransitionButton : Button
	{
		public GameObject NormalState
		{
			get { return _normalState; }
			set { _normalState = value; }
		}
		public GameObject HighlightedState
		{
			get { return _highlightedState; }
			set { _highlightedState = value; }
		}
		public GameObject PressedState
		{
			get { return _pressedState; }
			set { _pressedState = value; }
		}
		public GameObject SelectedState
		{
			get { return _selectedState; }
			set { _selectedState = value; }
		}
		public GameObject DisabledState
		{
			get { return _disabledState; }
			set { _disabledState = value; }
		}


		[SerializeField] GameObject _normalState;
		[SerializeField] GameObject _highlightedState;
		[SerializeField] GameObject _pressedState;
		[SerializeField] GameObject _selectedState;
		[SerializeField] GameObject _disabledState;
		protected override void DoStateTransition(SelectionState state, bool instant) {
			base.DoStateTransition(state, instant); // 선택사항: 애니메이션 등 유지하려면

			if (!IsActive() || !IsInteractable()) {
				ActivateState(_disabledState);
				return;
			}

			switch (state) {
				case SelectionState.Normal:
					ActivateState(_normalState);
					break;
				case SelectionState.Highlighted:
					ActivateState(_highlightedState);
					break;
				case SelectionState.Selected:
					ActivateState(_selectedState);
					break;
				case SelectionState.Pressed:
					ActivateState(_pressedState);
					break;
				case SelectionState.Disabled:
					ActivateState(_disabledState);
					break;
				default:
					ActivateState(_normalState);
					break;
			}
		}
#if UNITY_EDITOR
		protected override void OnValidate() {
			base.OnValidate();
			transition = Transition.None;
		}
#endif
		void ActivateState(GameObject target) {
			if (_normalState != null) _normalState.SetActive(target == _normalState);
			if (_highlightedState != null) _highlightedState.SetActive(target == _highlightedState);
			if (_pressedState != null) _pressedState.SetActive(target == _pressedState);
			if (_selectedState != null) _selectedState.SetActive(target == _selectedState);
			if (_disabledState != null) _disabledState.SetActive(target == _disabledState);
		}
	}
}