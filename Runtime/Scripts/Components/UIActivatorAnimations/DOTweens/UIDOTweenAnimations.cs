#if DOTWEEN && UNITASK_DOTWEEN_SUPPORT
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ParkMinPackages.UGUI.Components.UIActivatorAnimations;
using ParkMinPackages.UGUI.Enums;

namespace ParkMinPackages.UGUI.Components.UIActivatorAnimations.DOTweens
{
	public abstract class UIDOTweenActiveAnimation : ActiveAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			Tween tween = UIDOTweenAnimationUtility.ApplyUpdateSettings(CreateTween(), UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await tween.SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}
	public abstract class UIDOTweenDeactivateAnimation : DeactivateAnimation
	{
		public abstract Tween CreateTween();
		public override async UniTask ExecuteAsync(CancellationToken cancellationToken = default) {
			Tween tween = UIDOTweenAnimationUtility.ApplyUpdateSettings(CreateTween(), UIActivator.UpdateMode, UIActivator.IgnoreTimeScale);
			await tween.SetAutoKill(true).ToUniTask(TweenCancelBehaviour.KillAndCancelAwait, cancellationToken);
		}
	}

	static class UIDOTweenAnimationUtility
	{
		public static Tween ApplyUpdateSettings(Tween tween, UIAnimationUpdateMode updateMode, bool ignoreTimeScale) {
			UpdateType updateType = updateMode switch
			{
				UIAnimationUpdateMode.Update => UpdateType.Normal,
				UIAnimationUpdateMode.LateUpdate => UpdateType.Late,
				_ => throw new ArgumentOutOfRangeException(nameof(updateMode), updateMode, null)
			};
			return tween.SetUpdate(updateType, ignoreTimeScale);
		}
	}
}
#endif
