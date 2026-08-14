using System;
using System.Collections.Generic;
using UnityEngine;

namespace ParkMinPackages.UGUI.Interfaces
{
	public interface IItemListView<TModel, TItemView>
		where TItemView : MonoBehaviour, IItemView
	{
		void Initialize(Action<TModel, TItemView> createdAction = null);
		TItemView Insert(int index, TModel model);
		void RemoveAt(int index);
		void Move(int oldIndex, int newIndex);
		void Replace(int index, TModel model);
		void Clear();
		void Reverse(int index, int count);
		void Sort(
			int index,
			int count,
			IComparer<TModel> comparer
		);
	}
}
