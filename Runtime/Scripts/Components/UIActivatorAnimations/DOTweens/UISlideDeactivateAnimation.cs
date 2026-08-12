#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System;
using DG.Tweening;
using ParkMinPackages.UGUI.Enums;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UIDTSlideHideAnimation")]
	[DisallowMultipleComponent]
	public class UISlideDeactivateAnimation : UIDOTweenDeactivateAnimation
	{
		public override Tween CreateTween() {
			return Target.DOLocalMove(GetHiddenPosition(), Duration)
			             .From(_capturedPosition)
			             .SetEase(Ease);
		}
		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;
		public Direction DisappearingDirection;

		protected override void Awake() {
			base.Awake();
			_rootCanvasRectTransform = GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
		}
		protected override void CaptureValues() {
			_capturedPosition = Target.localPosition;
		}
		protected override void RestoreCapturedValues() {
			Target.localPosition = _capturedPosition;
		}
		RectTransform _rootCanvasRectTransform;
		Vector3 _capturedPosition;

		Vector3 GetHiddenPosition() {
			Vector3 canvasSize = _rootCanvasRectTransform.rect.size;
			switch (DisappearingDirection) {
				case Direction.Left:
					return _capturedPosition + new Vector3(-canvasSize.x, 0, 0);
				case Direction.Right:
					return _capturedPosition + new Vector3(canvasSize.x, 0, 0);
				case Direction.Top:
					return _capturedPosition + new Vector3(0, canvasSize.y, 0);
				case Direction.Bottom:
					return _capturedPosition + new Vector3(0, -canvasSize.y, 0);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
#endif
