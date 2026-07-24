#if LITMOTION_SUPPORT
using LitMotion;
using UnityEngine;

namespace Components.UIAnimations
{
	[DisallowMultipleComponent]
	public class UILMScaleShowAnimation : UILitMotionShowAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(Target.localScale, _defaultLocalScale, Duration)
			              .WithEase(Ease)
			              .Bind(x => Target.localScale = x);
		}
		public override void CaptureCurrent() {
			_defaultLocalScale = Target.localScale;
			Target.localScale = Vector3.zero;
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