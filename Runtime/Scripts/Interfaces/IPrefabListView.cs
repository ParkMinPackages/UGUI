using System.Collections.Generic;
using UnityEngine;

namespace ParkMinPackages.UGUI.Interfaces
{
	public interface IPrefabListView : IReadOnlyList<GameObject>
	{
		GameObject Insert(int index);
		void RemoveAt(int index);
		void Move(int oldIndex, int newIndex);
		GameObject Replace(int index);
		void Clear();
		void Reverse(int index, int count);

		GameObject Prefab { get; set; }
	}
}
