using System;
using System.Threading.Tasks;

namespace DotNetEx.Reactive
{
	public interface IAppCache
	{
		Boolean Contains( String key );
		Boolean Contains( String key, String regionName );

		T Get<T>( String key );
		T Get<T>( String key, String regionName );

		Task<T> GetAsync<T>( String key );
		Task<T> GetAsync<T>( String key, String regionName );

		T GetOrAdd<T>( String key, Func<Object, AppCacheItem<T>> factory );
		T GetOrAdd<T>( String key, String regionName, Func<Object, AppCacheItem<T>> factory );

		Task<T> GetOrAddAsync<T>( String key, Func<Object, Task<AppCacheItem<T>>> factory );
		Task<T> GetOrAddAsync<T>( String key, String regionName, Func<Object, Task<AppCacheItem<T>>> factory );

		void Remove( String key );
		void Remove( String key, String regionName );

		Task RemoveAsync( String key );
		Task RemoveAsync( String key, String regionName );

		void AddOrUpdate<T>( String key, T value, DateTimeOffset expires );
		void AddOrUpdate<T>( String key, T value, String regionName, DateTimeOffset expires );
	}
}
