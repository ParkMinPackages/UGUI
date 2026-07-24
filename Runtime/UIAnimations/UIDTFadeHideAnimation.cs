#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using com.parkminpackages.expansion.Extensions;
using DG.Tweening;
using UnityEngine;

namespace com.parkminpackages.ugui.UIAnimations
{
	[DisallowMultipleComponent]
	public class UIDTFadeHideAnimation : UIDotTweenHideAnimation
	{
		public override Tween CreateTween() {
			return CanvasGroup.DOFade(0, Duration).SetEase(Ease);
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