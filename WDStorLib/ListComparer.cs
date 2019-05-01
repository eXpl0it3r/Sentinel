using System;
using System.Collections.Generic;

namespace Stor
{
	public class ListComparer<T>
	{
		public bool AnythingChanged
		{
			get
			{
				return this.addedItems.Count != 0 || this.removedItems.Count != 0 || this.changedItems.Count != 0;
			}
		}

		public bool Compare(List<T> oldList, List<T> newList, Func<T, T, bool> comparer)
		{
			this.addedItems = new List<ListComparer<T>.ItemData>();
			this.removedItems = new List<ListComparer<T>.ItemData>();
			this.changedItems = new List<ListComparer<T>.ItemData>();
			for (int i = 0; i < oldList.Count; i++)
			{
				T t = oldList[i];
				int num = newList.IndexOf(t);
				if (num != -1)
				{
					T t2 = newList[num];
					if (!comparer(t, t2))
					{
						this.changedItems.Add(new ListComparer<T>.ItemData(t2, t));
					}
				}
				else
				{
					this.removedItems.Add(new ListComparer<T>.ItemData(default(T), t));
				}
			}
			foreach (T t3 in newList)
			{
				int num2 = oldList.IndexOf(t3);
				if (num2 == -1)
				{
					this.addedItems.Add(new ListComparer<T>.ItemData(t3, default(T)));
				}
			}
			return this.addedItems.Count != 0 || this.removedItems.Count != 0 || this.changedItems.Count != 0;
		}

		public List<ListComparer<T>.ItemData> addedItems;

		public List<ListComparer<T>.ItemData> removedItems;

		public List<ListComparer<T>.ItemData> changedItems;

		public class ItemData
		{
			public ItemData(T newItem, T oldItem)
			{
				this.newItem = newItem;
				this.oldItem = oldItem;
			}

			public T newItem;

			public T oldItem;
		}
	}
}
