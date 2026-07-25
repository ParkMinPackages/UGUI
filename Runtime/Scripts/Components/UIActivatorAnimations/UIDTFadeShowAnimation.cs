#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using ParkMinPackages.Foundation.Extensions;
using UnityEngine;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations
{
	[DisallowMultipleComponent]
	public class UIDTFadeShowAnimation : UIDotTweenShowAnimation
	{
		public override Tween CreateTween() {
			CanvasGroup.alpha = 0;
			return CanvasGroup.DOFade(_defaultCanvasAlpha, Duration).SetEase(Ease);
		}
		public override void CaptureCurrent() {
			_defaultCanvasAlpha = CanvasGroup.alpha;
		}
		public override void ApplyCaptured() {
			CanvasGroup.alpha = _defaultCanvasAlpha;
		}

		public float Duration = 0.3f;
		public Ease Ease = Ease.Unset;

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