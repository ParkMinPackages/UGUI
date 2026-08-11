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
			             .From(ShownPosition)
			             .SetEase(Ease);
		}
		public override void ApplyStart() {
			Target.localPosition = ShownPosition;
		}
		public override void ApplyEnd() {
			Target.localPosition = GetHiddenPosition();
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;
		public Direction DisappearingDirection;
		public Vector3 ShownPosition;

		protected override void Awake() {
			base.Awake();
			_rootCanvasRectTransform = GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
		}

		RectTransform _rootCanvasRectTransform;

		Vector3 GetHiddenPosition() {
			Vector3 canvasSize = _rootCanvasRectTransform.rect.size;
			switch (DisappearingDirection) {
				case Direction.Left:
					return ShownPosition + new Vector3(-canvasSize.x, 0, 0);
				case Direction.Right:
					return ShownPosition + new Vector3(canvasSize.x, 0, 0);
				case Direction.Top:
					return ShownPosition + new Vector3(0, canvasSize.y, 0);
				case Direction.Bottom:
					return ShownPosition + new Vector3(0, -canvasSize.y, 0);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}
#endif
