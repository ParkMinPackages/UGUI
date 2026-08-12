#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using DG.Tweening;
using ParkMinPackages.Foundation.Extensions;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.Serialization;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UIDTFadeHideAnimation")]
	[DisallowMultipleComponent]
	public class UIFadeDeactivateAnimation : UIDOTweenDeactivateAnimation
	{
		public override Tween CreateTween() {
			return CanvasGroup.DOFade(Alpha, Duration).From(_capturedAlpha).SetEase(Ease);
		}
		public float Duration = 0.3f;
		public Ease Ease = Ease.Unset;
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
