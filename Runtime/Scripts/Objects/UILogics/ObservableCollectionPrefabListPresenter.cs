using System;
using System.Collections.Generic;
using ObservableCollections;
using ParkMinPackages.UGUI.Interfaces;
using R3;
using UnityEngine;

namespace ParkMinPackages.UGUI.Objects.UILogics
{
	public class ObservableCollectionPrefabListPresenter<TModel, TView> : IDisposable
		where TView : MonoBehaviour
	{
		public ObservableCollectionPrefabListPresenter(
			IObservableCollection<TModel> observableCollection,
			IPrefabListView prefabListView,
			Func<TModel, TView, IDisposable> createdFunc = null,
			Action<TModel, TView> removeAction = null
		) {
			ObservableCollection = observableCollection ?? throw new ArgumentNullException(nameof(observableCollection));
			PrefabListView = prefabListView ?? throw new ArgumentNullException(nameof(prefabListView));
			_createdFunc = createdFunc;
			_removeAction = removeAction ?? ((model, view) => {
				UnityEngine.Object.Destroy(view.gameObject);
			});

			Rebuild();

			_subscriptions = new CompositeDisposable();

			ObservableCollection.ObserveAdd().Subscribe(e => {
				Insert(e.Index, e.Value);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveRemove().Subscribe(e => {
				RemoveAt(e.Index);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveMove().Subscribe(e => {
				Move(e.OldIndex, e.NewIndex);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveReplace().Subscribe(e => {
				Replace(e.Index, e.NewValue);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveClear().Subscribe(_ => {
				Clear();
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveReverse().Subscribe(e => {
				Reverse(e.Index, e.Count);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveSort().Subscribe(e => {
				Sort(e.Index, e.Count, e.Comparer);
			}).AddTo(_subscriptions);
		}
		public void Dispose() {
			_subscriptions.Dispose();

			(TModel Model, TView View, IDisposable Disposable)[] items = _items.ToArray();
			_items.Clear();

			foreach ((TModel Model, TView View, IDisposable Disposable) item in items) {
				item.Disposable?.Dispose();
			}
		}

		public IObservableCollection<TModel> ObservableCollection { get; }
		public IPrefabListView PrefabListView { get; }

		void Insert(int index, TModel model) {
			GameObject prefabObject = PrefabListView.Insert(index);
			AddCreatedView(index, model, prefabObject);
		}
		void AddCreatedView(int index, TModel model, GameObject prefabObject) {
			TView view = prefabObject.GetComponent<TView>();

			if (view == null) {
				PrefabListView.RemoveAt(index);
				UnityEngine.Object.Destroy(prefabObject);
				throw new InvalidOperationException(
					$"{nameof(PrefabListView)} prefab must contain {typeof(TView).Name}"
				);
			}

			try {
				IDisposable disposable;

				if (_createdFunc != null)
					disposable = _createdFunc(model, view);
				else {
					view.gameObject.SetActive(true);
					disposable = null;
				}

				_items.Insert(index, (model, view, disposable));
			}
			catch {
				PrefabListView.RemoveAt(index);
				UnityEngine.Object.Destroy(prefabObject);
				throw;
			}
		}
		void RemoveAt(int index) {
			(TModel Model, TView View, IDisposable Disposable) item = _items[index];

			_items.RemoveAt(index);
			PrefabListView.RemoveAt(index);
			item.Disposable?.Dispose();
			_removeAction(item.Model, item.View);
		}
		void Move(int oldIndex, int newIndex) {
			if (oldIndex == newIndex)
				return;

			(TModel Model, TView View, IDisposable Disposable) item = _items[oldIndex];

			_items.RemoveAt(oldIndex);
			_items.Insert(newIndex, item);
			PrefabListView.Move(oldIndex, newIndex);
		}
		void Replace(int index, TModel model) {
			(TModel Model, TView View, IDisposable Disposable) removedItem = _items[index];
			_items.RemoveAt(index);

			GameObject prefabObject = PrefabListView.Replace(index);

			try {
				AddCreatedView(index, model, prefabObject);
			}
			catch {
				removedItem.Disposable?.Dispose();
				_removeAction(removedItem.Model, removedItem.View);
				throw;
			}

			removedItem.Disposable?.Dispose();
			_removeAction(removedItem.Model, removedItem.View);
		}
		void Clear() {
			(TModel Model, TView View, IDisposable Disposable)[] removedItems = _items.ToArray();

			_items.Clear();
			PrefabListView.Clear();

			foreach ((TModel Model, TView View, IDisposable Disposable) item in removedItems) {
				item.Disposable?.Dispose();
				_removeAction(item.Model, item.View);
			}
		}
		void Reverse(int index, int count) {
			_items.Reverse(index, count);
			PrefabListView.Reverse(index, count);
		}
		void Sort(int index, int count, IComparer<TModel> comparer) {
			IComparer<TModel> modelComparer = comparer ?? Comparer<TModel>.Default;
			List<(TModel Model, TView View, IDisposable Disposable)> items =
				new List<(TModel Model, TView View, IDisposable Disposable)>(count);

			for (int i = index; i < index + count; i++) {
				items.Add(_items[i]);
			}

			items.Sort((left, right) => modelComparer.Compare(left.Model, right.Model));

			for (int i = 0; i < count; i++) {
				int targetIndex = index + i;
				TView targetView = items[i].View;
				int currentIndex = _items.FindIndex(
					index,
					count,
					item => item.View == targetView
				);

				if (currentIndex < 0) {
					throw new InvalidOperationException(
						$"{nameof(PrefabListView)} item order is out of sync"
					);
				}

				if (currentIndex == targetIndex)
					continue;

				(TModel Model, TView View, IDisposable Disposable) movedItem = _items[currentIndex];

				_items.RemoveAt(currentIndex);
				_items.Insert(targetIndex, movedItem);
				PrefabListView.Move(currentIndex, targetIndex);
			}
		}
		void Rebuild() {
			_items.Clear();
			PrefabListView.Clear();

			int index = 0;

			foreach (TModel model in ObservableCollection) {
				Insert(index, model);
				index++;
			}
		}

		readonly CompositeDisposable _subscriptions;
		readonly Func<TModel, TView, IDisposable> _createdFunc;
		readonly Action<TModel, TView> _removeAction;
		readonly List<(TModel Model, TView View, IDisposable Disposable)> _items =
			new List<(TModel Model, TView View, IDisposable Disposable)>();
	}
}
