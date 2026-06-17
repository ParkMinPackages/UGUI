using System.Threading;
using com.mutant.expansion;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.mutant.ugui
{
	[RequireComponent(typeof(UIActivator))]
	public class BasicUI : RootObject
	{
		public UniTask ActiveAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Cancel, CancellationToken cancellationToken = default) {
			return _uiActivator.ActiveAsync(forceExecute, cancelBehaviour, cancellationToken);
		}
		public UniTask DeActiveAsync(bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Complete, CancellationToken cancellationToken = default) {
			return _uiActivator.DeActiveAsync(forceExecute, cancelBehaviour, cancellationToken);
		}
		public void ActiveImmediate() {
			_uiActivator.ActiveImmediate();
		}
		public void DeActiveImmediate() {
			_uiActivator.DeActiveImmediate();
		}
		public UniTask ActiveAllChildAsync(float duration, bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Cancel, CancellationToken cancellationToken = default) {
			return _uiActivator.ActiveAllChildAsync(duration, forceExecute, cancelBehaviour, cancellationToken);
		}
		public UniTask DeActiveAllChildAsync(float duration, bool forceExecute = false, CancelBehavior cancelBehaviour = CancelBehavior.Cancel, CancellationToken cancellationToken = default) {
			return _uiActivator.DeActiveAllChildAsync(duration, forceExecute, cancelBehaviour, cancellationToken);
		}
		public void DeActiveAllChildImmediate() {
			_uiActivator.DeActiveAllChildImmediate();
		}
		public void DeDeActiveAllChildImmediate() {
			_uiActivator.DeDeActiveAllChildImmediate();
		}
		public bool Interactable
		{
			get { return _uiActivator.Interactable; }
			set { _uiActivator.Interactable = value; }
		}

		public UIActivator UIActivator
		{
			get { return _uiActivator; }
		}

		protected override void Awake() {
			base.Awake();
			_uiActivator = GetComponent<UIActivator>();
		}

		UIActivator _uiActivator;
	}
}