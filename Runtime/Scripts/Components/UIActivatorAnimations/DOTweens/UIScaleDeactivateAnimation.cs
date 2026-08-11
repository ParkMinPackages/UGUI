#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UIDTScaleHideAnimation")]
	[DisallowMultipleComponent]
	public class UIScaleDeactivateAnimation : UIDOTweenDeactivateAnimation
	{
		public override Tween CreateTween() {
			return Target.DOScale(EndScale, Duration).From(StartScale).SetEase(Ease);
		}
		public override void ApplyStart() {
			Target.localScale = StartScale;
		}
		public override void ApplyEnd() {
			Target.localScale = EndScale;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;
		public Vector3 StartScale = Vector3.one;
		public Vector3 EndScale = Vector3.zero;
	}
}
#endif
