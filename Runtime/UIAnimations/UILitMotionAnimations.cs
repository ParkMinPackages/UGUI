#if LITMOTION_SUPPORT
using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;

namespace com.mutant.ugui.UIAnimations
{
	public abstract class UILitMotionShowAnimation : ShowAnimation
	{
		public abstract MotionHandle CreateMotion();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateMotion().ToUniTask(LitMotion.CancelBehavior.None, cancellationToken);
		}
	}

	public abstract class UILitMotionHideAnimation : HideAnimation
	{
		public abstract MotionHandle CreateMotion();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken) {
			await CreateMotion().ToUniTask(LitMotion.CancelBehavior.None, cancellationToken);
		}
	}
}
#endif