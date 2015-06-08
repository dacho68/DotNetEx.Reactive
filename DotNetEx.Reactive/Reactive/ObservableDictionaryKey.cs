using System;
using System.Collections.Generic;

namespace DotNetEx.Reactive
{
	public abstract class ObservableDictionaryKey<T, TKey>
	{
		public virtual IEqualityComparer<TKey> Comparer
		{
			get
			{
				return EqualityComparer<TKey>.Default;
			}
		}


		public abstract TKey GetKey( T item );
	}
}
