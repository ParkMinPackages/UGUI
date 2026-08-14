using System;
using System.Collections.Generic;
using ParkMinPackages.UGUI.Interfaces;
using UnityEngine;

namespace ParkMinPackages.UGUI.Components.Views
{
	public class ItemListView<TModel, TItemView> :
		MonoBehaviour,
		IItemListView<TModel, TItemView>
		where TItemView : MonoBehaviour, IItemView
	{
		public void Initialize(Action<TModel, TItemView> createdAction = null) {
			if (_initialized) {
				throw new InvalidOperationException(
					$"{nameof(ItemListView<TModel, TItemView>)} is already initialized"
				);
			}

			if (_itemViewPrefab == null)
				throw new ArgumentNullException(nameof(ItemViewPrefab));

			_createdAction = createdAction;
			_itemViewPrefab.gameObject.SetActive(false);
			_initialized = true;
		}
		public TItemView Insert(int index, TModel model) {
			if (!_initialized) {
				throw new InvalidOperationException(
					$"{nameof(ItemListView<TModel, TItemView>)} is not initialized"
				);
			}

			TItemView itemView = Instantiate(
				_itemViewPrefab,
				_itemViewPrefab.transform.parent
			);

			_models.Insert(index, model);
			_itemViews.Insert(index, itemView);

			try {
				_createdAction?.Invoke(model, itemView);
				itemView.Spawn();
				ApplyItemViewSiblingOrder();
			}
			catch {
				_models.RemoveAt(index);
				_itemViews.Remove(itemView);
				Destroy(itemView.gameObject);
				throw;
			}

			return itemView;
		}
		public void RemoveAt(int index) {
			TItemView itemView = _itemViews[index];

			_models.RemoveAt(index);
			_itemViews.RemoveAt(index);

			if (itemView != null)
				itemView.Remove();
		}
		public void Move(int oldIndex, int newIndex) {
			if (oldIndex == newIndex)
				return;

			TModel model = _models[oldIndex];
			TItemView itemView = _itemViews[oldIndex];

			_models.RemoveAt(oldIndex);
			_itemViews.RemoveAt(oldIndex);
			_models.Insert(newIndex, model);
			_itemViews.Insert(newIndex, itemView);
			ApplyItemViewSiblingOrder();
		}
		public void Replace(int index, TModel model) {
			RemoveAt(index);
			Insert(index, model);
		}
		public void Clear() {
			TItemView[] removedItemViews = _itemViews.ToArray();

			_models.Clear();
			_itemViews.Clear();

			foreach (TItemView itemView in removedItemViews) {
				if (itemView != null)
					itemView.Remove();
			}
		}
		public void Reverse(int index, int count) {
			_models.Reverse(index, count);
			_itemViews.Reverse(index, count);
			ApplyItemViewSiblingOrder();
		}
		public void Sort(int index, int count, IComparer<TModel> comparer) {
			IComparer<TModel> itemComparer = comparer ?? Comparer<TModel>.Default;
			List<(TModel Model, TItemView ItemView)> items =
				new List<(TModel Model, TItemView ItemView)>(count);

			for (int i = index; i < index + count; i++) {
				items.Add((_models[i], _itemViews[i]));
			}

			items.Sort((left, right) => itemComparer.Compare(left.Model, right.Model));

			for (int i = 0; i < count; i++) {
				_models[index + i] = items[i].Model;
				_itemViews[index + i] = items[i].ItemView;
			}

			ApplyItemViewSiblingOrder();
		}

		public TItemView ItemViewPrefab
		{
			get { return _itemViewPrefab; }
			set {
				if (_initialized) {
					throw new InvalidOperationException(
						$"{nameof(ItemViewPrefab)} cannot be changed after initialization"
					);
				}

				_itemViewPrefab = value;
			}
		}
		public IReadOnlyList<TItemView> ItemViews
		{
			get { return _itemViews; }
		}

		void ApplyItemViewSiblingOrder() {
			if (_itemViews.Count == 0)
				return;

			Transform parent = _itemViewPrefab.transform.parent;
			HashSet<Transform> itemViewTransforms = new HashSet<Transform>();

			foreach (TItemView itemView in _itemViews) {
				if (itemView != null && itemView.transform.parent == parent)
					itemViewTransforms.Add(itemView.transform);
			}

			List<int> siblingIndexes = new List<int>(itemViewTransforms.Count);

			for (int i = 0; i < parent.childCount; i++) {
				if (itemViewTransforms.Contains(parent.GetChild(i)))
					siblingIndexes.Add(i);
			}

			int count = Mathf.Min(_itemViews.Count, siblingIndexes.Count);

			for (int i = 0; i < count; i++) {
				TItemView itemView = _itemViews[i];

				if (itemView != null && itemView.transform.parent == parent)
					itemView.transform.SetSiblingIndex(siblingIndexes[i]);
			}
		}

		[SerializeField] TItemView _itemViewPrefab;
		readonly List<TModel> _models = new List<TModel>();
		readonly List<TItemView> _itemViews = new List<TItemView>();
		Action<TModel, TItemView> _createdAction;
		bool _initialized;
	}
}
