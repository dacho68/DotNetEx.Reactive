using System;
using System.Collections.Generic;

namespace DotNetEx.Reactive
{
	public interface IObservableDictionary<TKey, TValue> : IObservableList<TValue>
	{
		IReadOnlyDictionary<TKey, TValue> Map { get; }


		Boolean ContainsKey( TKey key );
	}
}
