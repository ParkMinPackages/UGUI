#if LITMOTION_SUPPORT
using com.parkminpackages.expansion.Extensions;
using LitMotion;
using UnityEngine;

namespace com.parkminpackages.ugui.Components.UIAnimations
{
	[DisallowMultipleComponent]
	public class UILMFadeShowAnimation : UILitMotionShowAnimation
	{
		public override MotionHandle CreateMotion() {
			CanvasGroup.alpha = 0;
			return LMotion.Create(CanvasGroup.alpha, _defaultCanvasAlpha, Duration)
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