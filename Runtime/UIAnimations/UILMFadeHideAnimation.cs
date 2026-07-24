#if LITMOTION_SUPPORT
using com.parkminpackages.expansion.Extensions;
using LitMotion;
using UnityEngine;

namespace com.parkminpackages.ugui.UIAnimations
{
	[DisallowMultipleComponent]
	public class UILMFadeHideAnimation : UILitMotionHideAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(CanvasGroup.alpha, 0f, Duration)
			              .WithEase(Ease)
			              .Bind(x => CanvasGroup.alpha = x)
			              .AddTo(CanvasGroup.gameObject);
		}
		public override void CaptureCurrent() {
			_defaultCanvasAlpha = CanvasGroup.alpha;
		}
		public override void ApplyCaptured() {
			CanvasGroup.alpha = _defaultCanvasAlpha;
		}

		public float Duration = 0.3f;
		public Ease Ease = Ease.Linear;

		CanvasGroup _canvasGroup;
		float _defaultCanvasAlpha;

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