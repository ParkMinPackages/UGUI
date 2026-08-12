#if LITMOTION_SUPPORT
using System;
using LitMotion;
using ParkMinPackages.UGUI.Enums;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	[MovedFrom(true, "ParkMinPackages.UGUI.Components.UIActivatorAnimations", "ParkMinPackages.UGUI", "UILMSlideShowAnimation")]
	[DisallowMultipleComponent]
	public class UISlideActiveAnimation : UILitMotionActiveAnimation
	{
		public override MotionHandle CreateMotion() {
			return LMotion.Create(GetHiddenPosition(), _capturedPosition, Duration)
			              .WithEase(Ease)
			              .WithCancelOnError()
			              .Bind(x => Target.localPosition = x)
			              .AddTo(Target.gameObject);
		}
		public float Duration = 0.2f;
		public Ease Ease = Ease.Linear;
		public Direction AppearingDirection;

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
			switch (AppearingDirection) {
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
