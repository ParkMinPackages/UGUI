#if LITMOTION_SUPPORT
using LitMotion;
using UnityEngine;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations
{
	[DisallowMultipleComponent]
	public class UILMMoveFromShowAnimation : UILitMotionShowAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(_defaultLocalPos + Offset, _defaultLocalPos, Duration)
			              .WithEase(Ease)
			              .Bind(x => Target.localPosition = x)
			              .AddTo(Target.gameObject);
		}
		public override void CaptureCurrent() {
			_defaultLocalPos = Target.localPosition;
		}
		public override void ApplyCaptured() {
			Target.localPosition = _defaultLocalPos;
		}

		public float Duration = 0.1f;
		public Ease Ease = Ease.Linear;
		public Vector3 Offset;

		Vector3 _defaultLocalPos;
	}
}
#endif