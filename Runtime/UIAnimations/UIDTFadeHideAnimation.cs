#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using JCMediLab.Extensions;
using UnityEngine;

namespace com.mutant.ugui.UIAnimations
{
	public class UIDTFadeHideAnimation : UIDotTweenHideAnimation
	{
		public override void CaptureCurrent() {
			_defaultCanvasAlpha = CanvasGroup.alpha;
		}
		public override void ApplyCaptured() {
			CanvasGroup.alpha = _defaultCanvasAlpha;
		}
		public override Tween CreateTween() {
			return CanvasGroup.DOFade(0, Duration).SetEase(Ease);
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