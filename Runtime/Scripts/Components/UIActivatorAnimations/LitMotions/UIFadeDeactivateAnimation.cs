#if LITMOTION_SUPPORT
using LitMotion;
using ParkMinPackages.Foundation.Extensions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMFadeHideAnimation")]
	[DisallowMultipleComponent]
	public class UIFadeDeactivateAnimation : UILitMotionDeactivateAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(_capturedAlpha, Alpha, Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => CanvasGroup.alpha = x)
			              .AddTo(CanvasGroup.gameObject);
		}
		public float Duration = 0.3f;
		public Ease Ease = Ease.Linear;
		[FormerlySerializedAs("EndAlpha")]
		public float Alpha;

		protected override void CaptureValues() {
			_capturedAlpha = CanvasGroup.alpha;
		}
		protected override void RestoreCapturedValues() {
			CanvasGroup.alpha = _capturedAlpha;
		}
		CanvasGroup _canvasGroup;
		float _capturedAlpha;

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
