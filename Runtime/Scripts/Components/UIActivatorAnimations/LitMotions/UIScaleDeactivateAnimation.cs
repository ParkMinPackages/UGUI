#if LITMOTION_SUPPORT
using LitMotion;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMScaleHideAnimation")]
	[DisallowMultipleComponent]
	public class UIScaleDeactivateAnimation : UILitMotionDeactivateAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(_capturedScale, Scale, Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => Target.localScale = x)
			              .AddTo(Target.gameObject);
		}
		public float Duration = 0.2f;
		public Ease Ease = Ease.Linear;
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
