#if LITMOTION_SUPPORT
using System;
using LitMotion;
using UnityEngine;

namespace com.parkminpackages.ugui.UIAnimations
{
	[DisallowMultipleComponent]
	public class UILMSlideShowAnimation : UILitMotionShowAnimation
	{
		public override MotionHandle CreateMotion() {
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

			return LMotion.Create(Target.localPosition, _defaultLocalPos, Duration)
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