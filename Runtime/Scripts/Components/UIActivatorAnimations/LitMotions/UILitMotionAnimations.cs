#if LITMOTION_SUPPORT
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using ParkMinPackages.UGUI.Components.UIActivatorAnimations;
using ParkMinPackages.UGUI.Enums;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	public abstract class UILitMotionActiveAnimation : ActiveAnimation
	{
		public abstract MotionHandle CreateMotion(IMotionScheduler scheduler);
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			IMotionScheduler scheduler = UILitMotionAnimationUtility.GetScheduler(UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await CreateMotion(scheduler).ToUniTask(LitMotion.CancelBehavior.Cancel, cancellationToken);
		}
	}

	public abstract class UILitMotionDeactivateAnimation : DeactivateAnimation
	{
		public abstract MotionHandle CreateMotion(IMotionScheduler scheduler);
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			IMotionScheduler scheduler = UILitMotionAnimationUtility.GetScheduler(UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await CreateMotion(scheduler).ToUniTask(LitMotion.CancelBehavior.Cancel, cancellationToken);
		}
	}

	static class UILitMotionAnimationUtility
	{
		public static IMotionScheduler GetScheduler(UIAnimationUpdateMode updateMode, bool ignoreTimeScale) {
			return updateMode switch
			{
				UIAnimationUpdateMode.Update => ignoreTimeScale ? MotionScheduler.UpdateIgnoreTimeScale : MotionScheduler.Update,
				UIAnimationUpdateMode.LateUpdate => ignoreTimeScale ? MotionScheduler.PreLateUpdateIgnoreTimeScale : MotionScheduler.PreLateUpdate,
				_ => throw new ArgumentOutOfRangeException(nameof(updateMode), updateMode, null)
			};
		}
	}
}
#endif
