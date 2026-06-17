#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using UnityEngine;

namespace com.mutant.ugui.UIAnimations
{
	[DisallowMultipleComponent]
	public class UIDTScaleShowAnimation : UIDotTweenShowAnimation
	{
		public override Tween CreateTween() {
			return Target.DOScale(_defaultLocalScale, Duration).SetEase(Ease);
		}
		public override void CaptureCurrent() {
			_defaultLocalScale = Target.localScale;
			Target.localScale = Vector3.zero;
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