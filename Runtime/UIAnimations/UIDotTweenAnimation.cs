#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;

namespace com.mutant.ugui.UIAnimations
{
	public abstract class UIDotTweenShowAnimation : ShowAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateTween().SetAutoKill(true).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
		}
	}
	public abstract class UIDotTweenHideAnimation : HideAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			await CreateTween().SetAutoKill(true).ToUniTask(TweenCancelBehaviour.Kill, cancellationToken);
		}
	}
}
#endif