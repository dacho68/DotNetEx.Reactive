using System;
using System.Collections.Generic;

namespace DotNetEx.Reactive
{
	public interface IObservableDictionary<TKey, TValue> : IObservableList<TValue>
	{
		Int32 IndexOfKey( TKey key );

		Boolean ContainsKey( TKey key );
		Boolean RemoveKey( TKey key );

		Boolean TryGetValue( TKey key, out TValue value );
		TValue GetValue( TKey key );

		void AddOrUpdate( TValue value );
	}
}
