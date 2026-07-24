#if LITMOTION_SUPPORT
using System;
using Enums;
using LitMotion;
using UnityEngine;

namespace Components.UIAnimations
{
	[DisallowMultipleComponent]
	public class UILMSlideHideAnimation : UILitMotionHideAnimation
	{
		public override MotionHandle CreateMotion() {
			Vector3 canvasSize = _rootCanvasRectTransform.rect.size;

			Vector3 targetPos = Target.localPosition;

			switch (DisappearingDirection) {
				case Direction.Left:
					targetPos += new Vector3(-canvasSize.x, 0, 0);
					break;

				case Direction.Right:
					targetPos += new Vector3(canvasSize.x, 0, 0);
					break;

				case Direction.Top:
					targetPos += new Vector3(0, canvasSize.y, 0);
					break;

				case Direction.Bottom:
					targetPos += new Vector3(0, -canvasSize.y, 0);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			return LMotion.Create(Target.localPosition, targetPos, Duration)
			              .WithEase(Ease)
			              .Bind(x => Target.localPosition = x)
			              .AddTo(Target.gameObject);
		}
		public override void CaptureCurrent() {
			_defaultLocalPos = Target.localPosition;
		}
		public override void ApplyCaptured() {
			Target.localPosition = _defaultLocalPos;
		}

		public float Duration = 0.2f;
		public Ease Ease = Ease.Linear;
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