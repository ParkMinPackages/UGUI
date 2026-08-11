#if LITMOTION_SUPPORT
using LitMotion;
using ParkMinPackages.Foundation.Extensions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMFadeShowAnimation")]
	[DisallowMultipleComponent]
	public class UIFadeActiveAnimation : UILitMotionActiveAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(StartAlpha, EndAlpha, Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => CanvasGroup.alpha = x)
			              .AddTo(CanvasGroup.gameObject);
		}
		public override void ApplyStart() {
			CanvasGroup.alpha = StartAlpha;
		}
		public override void ApplyEnd() {
			CanvasGroup.alpha = EndAlpha;
		}

		public float Duration = 0.3f;
		public Ease Ease = Ease.Linear;
		public float StartAlpha;
		public float EndAlpha = 1f;

		CanvasGroup _canvasGroup;

		CanvasGroup CanvasGroup
		{
			get
			{
				if (_canvasGroup == null)
					_canvasGroup = Target.gameObject.GetOrAddComponent<CanvasGroup>();

				return _canvasGroup;
			}
		}
	}
}
#endif
