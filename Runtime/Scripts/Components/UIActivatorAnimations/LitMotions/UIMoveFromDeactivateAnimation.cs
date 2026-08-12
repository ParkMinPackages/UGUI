#if LITMOTION_SUPPORT
using LitMotion;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMMoveFromHideAnimation")]
	[DisallowMultipleComponent]
	public class UIMoveFromDeactivateAnimation : UILitMotionDeactivateAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(_capturedPosition, GetOffsetPosition(), Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => Target.localPosition = x)
			              .AddTo(Target.gameObject);
		}
		public float Duration = 0.1f;
		public Ease Ease = Ease.Linear;
		public Vector3 Offset;

		protected override void CaptureValues() {
			_capturedPosition = Target.localPosition;
		}
		protected override void RestoreCapturedValues() {
			Target.localPosition = _capturedPosition;
		}
		Vector3 _capturedPosition;

		Vector3 GetOffsetPosition() {
			return _capturedPosition + Offset;
		}
	}
}
#endif
