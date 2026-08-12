#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UIDTScaleHideAnimation")]
	[DisallowMultipleComponent]
	public class UIScaleDeactivateAnimation : UIDOTweenDeactivateAnimation
	{
		public override Tween CreateTween() {
			return Target.DOScale(Scale, Duration).From(_capturedScale).SetEase(Ease);
		}
		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;
		[FormerlySerializedAs("EndScale")]
		public Vector3 Scale = Vector3.zero;

		protected override void CaptureValues() {
			_capturedScale = Target.localScale;
		}
		protected override void RestoreCapturedValues() {
			Target.localScale = _capturedScale;
		}
		Vector3 _capturedScale;
	}
}
#endif
