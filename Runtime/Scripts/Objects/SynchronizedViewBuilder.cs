using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using ObservableCollections;
using ParkMinPackages.UGUI.Interfaces;
using UnityEngine;

namespace ParkMinPackages.UGUI.Objects
{
	public static class SynchronizedViewBuilder<TData, TView>
		where TView : MonoBehaviour, ISyncronizedViewItem<TData>
	{
		public static ISynchronizedView<TData, TView> BuildDefault(
			IObservableCollection<TData> observableCollection,
			TView viewPrefab,
			Action<TData, TView> createdAction = null
		) {
			viewPrefab.gameObject.SetActive(false);

			Transform viewPrefabParent = viewPrefab.transform.parent;
			List<TView> trackedViews = new List<TView>();

			ISynchronizedView<TData, TView> view = observableCollection.CreateView(data =>
			{
				TView viewItem = UnityEngine.Object.Instantiate(viewPrefab, viewPrefabParent);

				viewItem.Spawn(data);
				createdAction?.Invoke(data, viewItem);
				trackedViews.Add(viewItem);

				return viewItem;
			});

			view.ViewChanged += delegate(in SynchronizedViewChangedEventArgs<TData, TView> e)
			{
				switch (e.Action) {
					case NotifyCollectionChangedAction.Add:
						ApplyViewSiblingOrderPreserveOtherChildren(view.ToViewList());
						break;

					case NotifyCollectionChangedAction.Remove:
						trackedViews.Remove(e.OldItem.View);
						e.OldItem.View.Remove();
						break;

					case NotifyCollectionChangedAction.Move:
						ApplyViewSiblingOrderPreserveOtherChildren(view.ToViewList());
						break;

					case NotifyCollectionChangedAction.Replace:
						trackedViews.Remove(e.OldItem.View);
						e.OldItem.View.Remove();
						ApplyViewSiblingOrderPreserveOtherChildren(view.ToViewList());
						break;

					case NotifyCollectionChangedAction.Reset:
						if (e.SortOperation.IsClear) {
							TView[] removedViews = trackedViews.ToArray();
							trackedViews.Clear();

							foreach (TView removedView in removedViews) {
								if (removedView != null)
									removedView.Remove();
							}
						}
						else {
							ApplyViewSiblingOrderPreserveOtherChildren(view.ToViewList());
						}
						break;
				}
			};

			return view;
		}

		static void ApplyViewSiblingOrderPreserveOtherChildren(
			ISynchronizedViewList<TView> orderedViews
		) {
			if (orderedViews == null || orderedViews.Count == 0)
				return;

			Transform parent = null;

			for (int i = 0; i < orderedViews.Count; i++) {
				if (orderedViews[i] == null)
					continue;

				parent = orderedViews[i].transform.parent;
				break;
			}

			if (parent == null)
				return;

			HashSet<Transform> viewTransforms = new HashSet<Transform>();

			for (int i = 0; i < orderedViews.Count; i++) {
				if (orderedViews[i] == null)
					continue;

				Transform viewTransform = orderedViews[i].transform;

				if (viewTransform.parent != parent) {
					Debug.LogWarning(
						$"View parent mismatch. View={viewTransform.name}, Parent={viewTransform.parent?.name}, ExpectedParent={parent.name}"
					);

					continue;
				}

				viewTransforms.Add(viewTransform);
			}

			List<int> viewSiblingSlots = new List<int>(viewTransforms.Count);

			for (int i = 0; i < parent.childCount; i++) {
				Transform child = parent.GetChild(i);

				if (viewTransforms.Contains(child)) {
					viewSiblingSlots.Add(i);
				}
			}

			if (viewSiblingSlots.Count != viewTransforms.Count) {
				Debug.LogWarning(
					$"Sibling slot count mismatch. slots={viewSiblingSlots.Count}, views={viewTransforms.Count}"
				);
			}

			int count = Mathf.Min(orderedViews.Count, viewSiblingSlots.Count);

			for (int i = 0; i < count; i++) {
				TView view = orderedViews[i];

				if (view == null)
					continue;

				Transform target = view.transform;

				if (target.parent != parent)
					continue;

				int targetSiblingIndex = viewSiblingSlots[i];

				if (target.GetSiblingIndex() == targetSiblingIndex)
					continue;

				target.SetSiblingIndex(targetSiblingIndex);
			}
		}
	}
}
