#if LITMOTION_SUPPORT
using LitMotion;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMMoveFromShowAnimation")]
	[DisallowMultipleComponent]
	public class UIMoveActiveAnimation : UILitMotionActiveAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(HiddenPosition, ShownPosition, Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => Target.localPosition = x)
			              .AddTo(Target.gameObject);
		}
		public override void ApplyStart() {
			Target.localPosition = HiddenPosition;
		}
		public override void ApplyEnd() {
			Target.localPosition = ShownPosition;
		}

		public float Duration = 0.1f;
		public Ease Ease = Ease.Linear;
		public Vector3 ShownPosition;
		public Vector3 HiddenPosition;
	}
}
#endif
