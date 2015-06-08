using System;

namespace DotNetEx.Reactive
{
	public interface IObservableDictionaryKey<TKey>
	{
		TKey GetKey();
	}
}
