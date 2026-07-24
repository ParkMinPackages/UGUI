using com.parkminpackages.expansion;
using UnityEngine;

namespace com.parkminpackages.ugui
{
	[RequireComponent(typeof(UIActivator))]
	public class BasicUI : Actor
	{
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