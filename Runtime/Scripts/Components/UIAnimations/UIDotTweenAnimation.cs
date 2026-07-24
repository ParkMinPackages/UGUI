#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace com.parkminpackages.ugui.Components.UIAnimations
{
	public abstract class UIDotTweenShowAnimation : ShowAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateTween().SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
	public abstract class UIDotTweenHideAnimation : HideAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateTween().SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
}
#endif