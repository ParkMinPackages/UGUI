#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System;
using com.parkminpackages.ugui.Enums;
using DG.Tweening;
using UnityEngine;

namespace com.parkminpackages.ugui.Components.UIAnimations
{
	[DisallowMultipleComponent]
	public class UIDTSlideHideAnimation : UIDotTweenHideAnimation
	{
		public override Tween CreateTween() {
			Tween tween = null;
			Vector3 canvasSize = _rootCanvasRectTransform.rect.size;
			switch (DisappearingDirection) {
				case Direction.Left:
					tween = Target.DOLocalMove(Target.localPosition + new Vector3(-canvasSize.x, 0, 0), Duration).SetEase(Ease).SetAutoKill(true);
					break;
				case Direction.Right:
					tween = Target.DOLocalMove(Target.localPosition + new Vector3(canvasSize.x, 0, 0), Duration).SetEase(Ease).SetAutoKill(true);
					break;
				case Direction.Top:
					tween = Target.DOLocalMove(Target.localPosition + new Vector3(0, canvasSize.y, 0), Duration).SetEase(Ease).SetAutoKill(true);
					break;
				case Direction.Bottom:
					tween = Target.DOLocalMove(Target.localPosition + new Vector3(0, -canvasSize.y, 0), Duration).SetEase(Ease).SetAutoKill(true);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
			return tween;
		}
		public override void CaptureCurrent() {
			_defaultLocalPos = Target.localPosition;
		}
		public override void ApplyCaptured() {
			Target.localPosition = _defaultLocalPos;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;
		public Direction DisappearingDirection;

		protected override void Awake() {
			base.Awake();
			_rootCanvasRectTransform = GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
		}

		Vector3 _defaultLocalPos;
		RectTransform _rootCanvasRectTransform;
	}
}
#endif