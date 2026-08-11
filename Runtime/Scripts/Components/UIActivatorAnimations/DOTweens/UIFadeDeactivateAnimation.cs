#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using ParkMinPackages.Foundation.Extensions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UIDTFadeHideAnimation")]
	[DisallowMultipleComponent]
	public class UIFadeDeactivateAnimation : UIDOTweenDeactivateAnimation
	{
		public override Tween CreateTween() {
			return CanvasGroup.DOFade(EndAlpha, Duration).From(StartAlpha).SetEase(Ease);
		}
		public override void ApplyStart() {
			CanvasGroup.alpha = StartAlpha;
		}
		public override void ApplyEnd() {
			CanvasGroup.alpha = EndAlpha;
		}

		public float Duration = 0.3f;
		public Ease Ease = Ease.Unset;
		public float StartAlpha = 1f;
		public float EndAlpha;

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
