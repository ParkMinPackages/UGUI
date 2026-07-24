#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using UnityEngine;

namespace Components.UIAnimations
{
	[DisallowMultipleComponent]
	public class UIDTScaleHideAnimation : UIDotTweenHideAnimation
	{
		public override Tween CreateTween() {
			return Target.DOScale(Vector3.zero, Duration).SetEase(Ease);
		}
		public override void CaptureCurrent() {
			_defaultLocalScale = Target.localScale;
		}
		public override void ApplyCaptured() {
			Target.localScale = _defaultLocalScale;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;

		Vector3 _defaultLocalScale;
	}
}
#endif