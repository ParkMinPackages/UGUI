using System;
using ObservableCollections;
using ParkMinPackages.UGUI.Interfaces;
using R3;
using UnityEngine;

namespace ParkMinPackages.UGUI.Objects.UILogics
{
	public class ObservableCollectionItemListPresenter<TModel, TItemView> : IDisposable
		where TItemView : MonoBehaviour, IItemView
	{
		public ObservableCollectionItemListPresenter(
			IObservableCollection<TModel> observableCollection,
			IItemListView<TModel, TItemView> itemListView,
			Action<TModel, TItemView> createdAction = null
		) {
			ObservableCollection = observableCollection ?? throw new ArgumentNullException(nameof(observableCollection));
			ItemListView = itemListView ?? throw new ArgumentNullException(nameof(itemListView));

			ItemListView.Initialize(createdAction);
			Rebuild();

			_subscriptions = new CompositeDisposable();

			ObservableCollection.ObserveAdd().Subscribe(e => {
				ItemListView.Insert(e.Index, e.Value);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveRemove().Subscribe(e => {
				ItemListView.RemoveAt(e.Index);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveMove().Subscribe(e => {
				ItemListView.Move(e.OldIndex, e.NewIndex);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveReplace().Subscribe(e => {
				ItemListView.Replace(e.Index, e.NewValue);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveClear().Subscribe(_ => {
				ItemListView.Clear();
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveReverse().Subscribe(e => {
				ItemListView.Reverse(e.Index, e.Count);
			}).AddTo(_subscriptions);
			ObservableCollection.ObserveSort().Subscribe(e => {
				ItemListView.Sort(e.Index, e.Count, e.Comparer);
			}).AddTo(_subscriptions);
		}
		public void Dispose() {
			_subscriptions.Dispose();
		}

		public IObservableCollection<TModel> ObservableCollection { get; }
		public IItemListView<TModel, TItemView> ItemListView { get; }

		void Rebuild() {
			ItemListView.Clear();

			int index = 0;

			foreach (TModel model in ObservableCollection) {
				ItemListView.Insert(index, model);
				index++;
			}
		}

		readonly CompositeDisposable _subscriptions;
	}
}
