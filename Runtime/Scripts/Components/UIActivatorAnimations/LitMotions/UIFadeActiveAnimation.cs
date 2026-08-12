#if LITMOTION_SUPPORT
using LitMotion;
using ParkMinPackages.Foundation.Extensions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMFadeShowAnimation")]
	[DisallowMultipleComponent]
	public class UIFadeActiveAnimation : UILitMotionActiveAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(Alpha, _capturedAlpha, Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => CanvasGroup.alpha = x)
			              .AddTo(CanvasGroup.gameObject);
		}
		public float Duration = 0.3f;
		public Ease Ease = Ease.Linear;
		[FormerlySerializedAs("StartAlpha")]
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
