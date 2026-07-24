#if LITMOTION_SUPPORT
using LitMotion;
using UnityEngine;

namespace Components.UIAnimations
{
	[DisallowMultipleComponent]
	public class UILMScaleHideAnimation : UILitMotionHideAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(Target.localScale, Vector3.zero, Duration)
			              .WithEase(Ease)
			              .Bind(x => Target.localScale = x);
		}
		public override void CaptureCurrent() {
			_defaultLocalScale = Target.localScale;
		}
		public override void ApplyCaptured() {
			Target.localScale = _defaultLocalScale;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Linear;

		Vector3 _defaultLocalScale;
	}
}
#endif