#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System;
using DG.Tweening;
using ParkMinPackages.UGUI.Enums;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UIDTSlideShowAnimation")]
	[DisallowMultipleComponent]
	public class UISlideActiveAnimation : UIDOTweenActiveAnimation
	{
		public override Tween CreateTween() {
			return Target.DOLocalMove(ShownPosition, Duration)
			             .From(GetHiddenPosition())
			             .SetEase(Ease);
		}
		public override void ApplyStart() {
			Target.localPosition = GetHiddenPosition();
		}
		public override void ApplyEnd() {
			Target.localPosition = ShownPosition;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Unset;
		public Direction AppearingDirection;
		public Vector3 ShownPosition;

		protected override void Awake() {
			base.Awake();
			_rootCanvasRectTransform = GetComponentInParent<Canvas>().rootCanvas.GetComponent<RectTransform>();
		}

		RectTransform _rootCanvasRectTransform;

		Vector3 GetHiddenPosition() {
			Vector3 canvasSize = _rootCanvasRectTransform.rect.size;
			switch (AppearingDirection) {
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
