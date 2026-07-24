using System.Threading;
using com.parkminpackages.expansion;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.parkminpackages.ugui.UIAnimations
{
	[RequireComponent(typeof(UIActivator))]
	public abstract class UIAnimation : ExtendedBehaviour
	{
		public abstract UniTask ExecuteAsync(CancellationToken cancellationToken = default);
		public abstract void CaptureCurrent();
		public abstract void ApplyCaptured();


		public UIActivator UIActivator { get; private set; }

		public RectTransform Target
		{
			get { return target; }
			set { target = value; }
		}

		protected virtual void Awake() {
			UIActivator = GetComponent<UIActivator>();
		}
		protected virtual void Reset() {
			Target = GetComponent<RectTransform>();
		}

		[SerializeField] RectTransform target;
	}

	public abstract class ShowAnimation : UIAnimation
	{
		protected override void OnEnable() {
			base.OnEnable();
			UIActivator.RegisterShowAnimation(this);
		}
		protected override void OnDisable() {
			base.OnDisable();
			UIActivator.UnregisterShowAnimation(this);
		}
	}
	public abstract class HideAnimation : UIAnimation
	{
		protected override void OnEnable() {
			base.OnEnable();
			UIActivator.RegisterHideAnimation(this);
		}
		protected override void OnDisable() {
			base.OnDisable();
			UIActivator.UnRegisterHideAnimation(this);
		}
	}
}