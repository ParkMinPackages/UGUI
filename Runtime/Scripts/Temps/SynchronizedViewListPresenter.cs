// using System;
// using System.Collections.Generic;
// using System.Collections.Specialized;
// using ObservableCollections;
// using ParkMinPackages.UGUI.Interfaces;
// using UnityEngine;
//
// namespace ParkMinPackages.UGUI.Objects
// {
// 	public interface ISynchronizedViewListPresenter<TModel, TView> :
// 		IDisposable
// 		where TView : MonoBehaviour, ISyncronizedViewItem<TModel>
// 	{
// 		public void Initialize(Action<TModel, TView> createdAction = null);
//
// 		public IObservableCollection<TModel> ObservableCollection { get; set; }
// 		public TView ViewPrefab { get; set; }
// 		public IReadOnlyList<TView> PresentedViews { get; }
// 		public ISynchronizedView<TModel, TView> SynchronizedView { get; }
//
// 		public static void Dispose(
// 			ISynchronizedViewListPresenter<TModel, TView> presenter,
// 			out ISynchronizedView<TModel, TView> synchronizedView,
// 			out IReadOnlyList<TView> presentedViews
// 		) {
// 			presenter.SynchronizedView?.Dispose();
// 			synchronizedView = null;
// 			presentedViews = Array.Empty<TView>();
// 		}
// 	}
//
// 	public interface IAutoOrderSynchronizedViewListPresenter<TModel, TView> :
// 		ISynchronizedViewListPresenter<TModel, TView>
// 		where TView : MonoBehaviour, ISyncronizedViewItem<TModel>
// 	{
// 		public static void Initialize(
// 			IAutoOrderSynchronizedViewListPresenter<TModel, TView> presenter,
// 			out ISynchronizedView<TModel, TView> synchronizedView,
// 			out IReadOnlyList<TView> presentedViews,
// 			Action<TModel, TView> createdAction = null
// 		) {
// 			if (presenter.SynchronizedView != null) {
// 				throw new InvalidOperationException(
// 					$"{presenter.GetType().Name} is already initialized"
// 				);
// 			}
//
// 			if (presenter.ViewPrefab == null)
// 				throw new ArgumentNullException(nameof(presenter.ViewPrefab), "ViewPrefab cannot be null");
//
// 			if (presenter.ObservableCollection == null)
// 				throw new ArgumentNullException(nameof(presenter.ObservableCollection), "ObservableCollection cannot be null");
//
// 			presenter.ViewPrefab.gameObject.SetActive(false);
//
// 			Transform viewPrefabParent = presenter.ViewPrefab.transform.parent;
// 			List<TView> trackedViews = new List<TView>();
// 			presentedViews = trackedViews;
//
// 			ISynchronizedView<TModel, TView> createdSynchronizedView = presenter.ObservableCollection.CreateView(model =>
// 			{
// 				TView viewItem = UnityEngine.Object.Instantiate(presenter.ViewPrefab, viewPrefabParent);
//
// 				viewItem.Spawn(model);
// 				createdAction?.Invoke(model, viewItem);
// 				trackedViews.Add(viewItem);
//
// 				return viewItem;
// 			});
//
// 			createdSynchronizedView.ViewChanged += delegate(in SynchronizedViewChangedEventArgs<TModel, TView> e)
// 			{
// 				switch (e.Action) {
// 					case NotifyCollectionChangedAction.Add:
// 						SynchronizeTrackedViews(createdSynchronizedView, trackedViews);
// 						ApplyViewSiblingOrderPreserveOtherChildren(trackedViews);
// 						break;
//
// 					case NotifyCollectionChangedAction.Remove:
// 						trackedViews.Remove(e.OldItem.View);
// 						e.OldItem.View.Remove();
// 						break;
//
// 					case NotifyCollectionChangedAction.Move:
// 						SynchronizeTrackedViews(createdSynchronizedView, trackedViews);
// 						ApplyViewSiblingOrderPreserveOtherChildren(trackedViews);
// 						break;
//
// 					case NotifyCollectionChangedAction.Replace:
// 						trackedViews.Remove(e.OldItem.View);
// 						e.OldItem.View.Remove();
// 						SynchronizeTrackedViews(createdSynchronizedView, trackedViews);
// 						ApplyViewSiblingOrderPreserveOtherChildren(trackedViews);
// 						break;
//
// 					case NotifyCollectionChangedAction.Reset:
// 						if (e.SortOperation.IsClear) {
// 							TView[] removedViews = trackedViews.ToArray();
// 							trackedViews.Clear();
//
// 							foreach (TView removedView in removedViews) {
// 								if (removedView != null)
// 									removedView.Remove();
// 							}
// 						}
// 						else {
// 							SynchronizeTrackedViews(createdSynchronizedView, trackedViews);
// 							ApplyViewSiblingOrderPreserveOtherChildren(trackedViews);
// 						}
// 						break;
// 				}
// 			};
//
// 			synchronizedView = createdSynchronizedView;
// 		}
//
// 		private static void SynchronizeTrackedViews(
// 			ISynchronizedView<TModel, TView> synchronizedView,
// 			List<TView> trackedViews
// 		) {
// 			trackedViews.Clear();
//
// 			foreach ((TModel Value, TView View) item in synchronizedView.Unfiltered) {
// 				trackedViews.Add(item.View);
// 			}
// 		}
// 		private static void ApplyViewSiblingOrderPreserveOtherChildren(
// 			IReadOnlyList<TView> orderedViews
// 		) {
// 			if (orderedViews == null || orderedViews.Count == 0)
// 				return;
//
// 			Transform parent = null;
//
// 			for (int i = 0; i < orderedViews.Count; i++) {
// 				if (orderedViews[i] == null)
// 					continue;
//
// 				parent = orderedViews[i].transform.parent;
// 				break;
// 			}
//
// 			if (parent == null)
// 				return;
//
// 			HashSet<Transform> viewTransforms = new HashSet<Transform>();
//
// 			for (int i = 0; i < orderedViews.Count; i++) {
// 				if (orderedViews[i] == null)
// 					continue;
//
// 				Transform viewTransform = orderedViews[i].transform;
//
// 				if (viewTransform.parent != parent) {
// 					Debug.LogWarning(
// 						$"View parent mismatch. View={viewTransform.name}, Parent={viewTransform.parent?.name}, ExpectedParent={parent.name}"
// 					);
//
// 					continue;
// 				}
//
// 				viewTransforms.Add(viewTransform);
// 			}
//
// 			List<int> viewSiblingSlots = new List<int>(viewTransforms.Count);
//
// 			for (int i = 0; i < parent.childCount; i++) {
// 				Transform child = parent.GetChild(i);
//
// 				if (viewTransforms.Contains(child)) {
// 					viewSiblingSlots.Add(i);
// 				}
// 			}
//
// 			if (viewSiblingSlots.Count != viewTransforms.Count) {
// 				Debug.LogWarning(
// 					$"Sibling slot count mismatch. slots={viewSiblingSlots.Count}, views={viewTransforms.Count}"
// 				);
// 			}
//
// 			int count = Mathf.Min(orderedViews.Count, viewSiblingSlots.Count);
//
// 			for (int i = 0; i < count; i++) {
// 				TView orderedView = orderedViews[i];
//
// 				if (orderedView == null)
// 					continue;
//
// 				Transform target = orderedView.transform;
//
// 				if (target.parent != parent)
// 					continue;
//
// 				int targetSiblingIndex = viewSiblingSlots[i];
//
// 				if (target.GetSiblingIndex() == targetSiblingIndex)
// 					continue;
//
// 				target.SetSiblingIndex(targetSiblingIndex);
// 			}
// 		}
// 	}
//
// 	public interface IFilterableSynchronizedViewListPresenter<TModel, TView> :
// 		ISynchronizedViewListPresenter<TModel, TView>
// 		where TView : MonoBehaviour, ISyncronizedViewItem<TModel>
// 	{
// 		public void ApplyFilter(Func<TModel, bool> filter);
// 		public void ResetFilter();
//
// 		public static void ApplyFilter(
// 			IFilterableSynchronizedViewListPresenter<TModel, TView> presenter,
// 			Func<TModel, bool> filter
// 		) {
// 			if (presenter.SynchronizedView == null) {
// 				throw new InvalidOperationException(
// 					$"{presenter.GetType().Name} is not initialized"
// 				);
// 			}
//
// 			if (filter == null)
// 				throw new ArgumentNullException(nameof(filter));
//
// 			SynchronizedViewFilter<TModel, TView> synchronizedViewFilter =
// 				new SynchronizedViewFilter<TModel, TView>((model, view) =>
// 				{
// 					bool isVisible = filter(model);
//
// 					if (view != null)
// 						view.gameObject.SetActive(isVisible);
//
// 					return isVisible;
// 				});
//
// 			presenter.SynchronizedView.AttachFilter(synchronizedViewFilter);
// 		}
//
// 		public static void ResetFilter(
// 			IFilterableSynchronizedViewListPresenter<TModel, TView> presenter
// 		) {
// 			if (presenter.SynchronizedView == null) {
// 				throw new InvalidOperationException(
// 					$"{presenter.GetType().Name} is not initialized"
// 				);
// 			}
//
// 			foreach (
// 				(TModel Value, TView View) item
// 				in presenter.SynchronizedView.Unfiltered
// 			) {
// 				if (item.View != null)
// 					item.View.gameObject.SetActive(true);
// 			}
//
// 			presenter.SynchronizedView.ResetFilter();
// 		}
// 	}
//
// 	public class AutoOrderSynchronizedViewListPresenter<TModel, TView> :
// 		MonoBehaviour,
// 		IAutoOrderSynchronizedViewListPresenter<TModel, TView>,
// 		IFilterableSynchronizedViewListPresenter<TModel, TView>
// 		where TView : MonoBehaviour, ISyncronizedViewItem<TModel>
// 	{
// 		public void Initialize(Action<TModel, TView> createdAction = null) {
// 			IAutoOrderSynchronizedViewListPresenter<TModel, TView>.Initialize(
// 				this,
// 				out _synchronizedView,
// 				out _presentedViews,
// 				createdAction
// 			);
// 		}
// 		public void ApplyFilter(Func<TModel, bool> filter) {
// 			IFilterableSynchronizedViewListPresenter<TModel, TView>.ApplyFilter(
// 				this,
// 				filter
// 			);
// 		}
// 		public void ResetFilter() {
// 			IFilterableSynchronizedViewListPresenter<TModel, TView>.ResetFilter(this);
// 		}
// 		public void Dispose() {
// 			IAutoOrderSynchronizedViewListPresenter<TModel, TView>.Dispose(
// 				this,
// 				out _synchronizedView,
// 				out _presentedViews
// 			);
// 		}
//
// 		public IObservableCollection<TModel> ObservableCollection { get; set; }
// 		public TView ViewPrefab { get; set; }
// 		public ISynchronizedView<TModel, TView> SynchronizedView
// 		{
// 			get { return _synchronizedView; }
// 		}
// 		public IReadOnlyList<TView> PresentedViews
// 		{
// 			get { return _presentedViews; }
// 		}
//
// 		void OnDestroy() {
// 			Dispose();
// 		}
//
// 		ISynchronizedView<TModel, TView> _synchronizedView;
// 		IReadOnlyList<TView> _presentedViews = Array.Empty<TView>();
// 	}
// }
