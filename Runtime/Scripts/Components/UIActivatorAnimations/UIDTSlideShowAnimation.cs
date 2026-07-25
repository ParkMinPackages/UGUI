#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System;
using DG.Tweening;
using ParkMinPackages.UGUI.Enums;
using UnityEngine;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations
{
	[DisallowMultipleComponent]
	public class UIDTSlideShowAnimation : UIDotTweenShowAnimation
	{
		public override Tween CreateTween() {
			Vector3 canvasSize = _rootCanvasRectTransform.rect.size;
			switch (AppearingDirection) {
				case Direction.Left:
					Target.localPosition += new Vector3(-canvasSize.x, 0, 0);
					break;
				case Direction.Right:
					Target.localPosition += new Vector3(canvasSize.x, 0, 0);
					break;
				case Direction.Top:
					Target.localPosition += new Vector3(0, canvasSize.y, 0);
					break;
				case Direction.Bottom:
					Target.localPosition += new Vector3(0, -canvasSize.y, 0);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return Target.DOLocalMove(_defaultLocalPos, Duration).SetEase(Ease).SetAutoKill(true);
		}
		public override void CaptureCurrent() {
			_defaultLocalPos = Target.localPosition;
		}
		public override void ApplyCaptured() {
			Target.localPosition = _defaultLocalPos;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;
		public Direction AppearingDirection;

		protected override void Awake() {
			base.Awake();
			_rootCanvasRectTransform = GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
		}

		Vector3 _defaultLocalPos;
		RectTransform _rootCanvasRectTransform;
	}
}
#endif