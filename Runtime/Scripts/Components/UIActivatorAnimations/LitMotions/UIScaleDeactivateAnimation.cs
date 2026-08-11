#if LITMOTION_SUPPORT
using LitMotion;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMScaleHideAnimation")]
	[DisallowMultipleComponent]
	public class UIScaleDeactivateAnimation : UILitMotionDeactivateAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(StartScale, EndScale, Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => Target.localScale = x)
			              .AddTo(Target.gameObject);
		}
		public override void ApplyStart() {
			Target.localScale = StartScale;
		}
		public override void ApplyEnd() {
			Target.localScale = EndScale;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Linear;
		public Vector3 StartScale = Vector3.one;
		public Vector3 EndScale = Vector3.zero;
	}
}
#endif
