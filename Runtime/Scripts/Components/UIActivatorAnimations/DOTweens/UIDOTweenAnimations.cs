#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ParkMinPackages.UGUI.Components.UIActivatorAnimations;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	public abstract class UIDOTweenActiveAnimation : ActiveAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateTween().SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
	public abstract class UIDOTweenDeactivateAnimation : DeactivateAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateTween().SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
}
#endif
