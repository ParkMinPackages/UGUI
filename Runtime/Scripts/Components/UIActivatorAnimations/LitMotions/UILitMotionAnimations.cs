#if LITMOTION_SUPPORT
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using ParkMinPackages.UGUI.Components.UIActivatorAnimations;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.LitMotions
{
	public abstract class UILitMotionActiveAnimation : ActiveAnimation
	{
		public abstract MotionHandle CreateMotion();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateMotion().ToUniTask(LitMotion.CancelBehavior.Cancel, cancellationToken);
		}
	}

	public abstract class UILitMotionDeactivateAnimation : DeactivateAnimation
	{
		public abstract MotionHandle CreateMotion();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateMotion().ToUniTask(LitMotion.CancelBehavior.Cancel, cancellationToken);
		}
	}
}
#endif
