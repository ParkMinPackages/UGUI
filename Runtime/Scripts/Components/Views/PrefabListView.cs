using System;
using System.Collections;
using System.Collections.Generic;
using ParkMinPackages.UGUI.Interfaces;
using UnityEngine;
using UnityEngine.Serialization;

namespace ParkMinPackages.UGUI.Components.Views
{
	public class PrefabListView : MonoBehaviour, IPrefabListView
	{
		public GameObject Insert(int index) {
			if (_prefab == null)
				throw new ArgumentNullException(nameof(Prefab));

			Transform parent = _prefab.transform.parent;

			if (parent == null) {
				throw new InvalidOperationException(
					$"{nameof(Prefab)} must have a parent"
				);
			}

			_prefab.SetActive(false);

			GameObject itemObject = Instantiate(
				_prefab,
				parent
			);

			_itemObjects.Insert(index, itemObject);
			ApplyItemSiblingOrder();

			return itemObject;
		}
		public void RemoveAt(int index) {
			_itemObjects.RemoveAt(index);
		}
		public void Move(int oldIndex, int newIndex) {
			if (oldIndex == newIndex)
				return;

			GameObject itemObject = _itemObjects[oldIndex];

			_itemObjects.RemoveAt(oldIndex);
			_itemObjects.Insert(newIndex, itemObject);
			ApplyItemSiblingOrder();
		}
		public GameObject Replace(int index) {
			RemoveAt(index);
			return Insert(index);
		}
		public void Clear() {
			_itemObjects.Clear();
		}
		public void Reverse(int index, int count) {
			_itemObjects.Reverse(index, count);
			ApplyItemSiblingOrder();
		}
		public IEnumerator<GameObject> GetEnumerator() {
			return _itemObjects.GetEnumerator();
		}

		public GameObject Prefab
		{
			get { return _prefab; }
			set {
				if (_prefab == value)
					return;

				if (_itemObjects.Count > 0) {
					throw new InvalidOperationException(
						$"{nameof(Prefab)} cannot be changed while the list contains items"
					);
				}

				_prefab = value;

				if (_prefab != null)
					_prefab.SetActive(false);
			}
		}
		public int Count
		{
			get { return _itemObjects.Count; }
		}
		public GameObject this[int index]
		{
			get { return _itemObjects[index]; }
		}

		void Awake() {
			if (_prefab != null)
				_prefab.SetActive(false);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		void ApplyItemSiblingOrder() {
			if (_itemObjects.Count == 0)
				return;

			Transform parent = _prefab.transform.parent;

			if (parent == null) {
				throw new InvalidOperationException(
					$"{nameof(Prefab)} must have a parent"
				);
			}

			HashSet<Transform> itemTransforms = new HashSet<Transform>();

			foreach (GameObject itemObject in _itemObjects) {
				if (itemObject != null && itemObject.transform.parent == parent)
					itemTransforms.Add(itemObject.transform);
			}

			List<int> siblingIndexes = new List<int>(itemTransforms.Count);

			for (int i = 0; i < parent.childCount; i++) {
				if (itemTransforms.Contains(parent.GetChild(i)))
					siblingIndexes.Add(i);
			}

			int count = Mathf.Min(_itemObjects.Count, siblingIndexes.Count);

			for (int i = 0; i < count; i++) {
				GameObject itemObject = _itemObjects[i];

				if (itemObject != null && itemObject.transform.parent == parent)
					itemObject.transform.SetSiblingIndex(siblingIndexes[i]);
			}
		}

		[SerializeField, FormerlySerializedAs("_itemViewPrefab")] GameObject _prefab;
		readonly List<GameObject> _itemObjects = new List<GameObject>();
	}
}
