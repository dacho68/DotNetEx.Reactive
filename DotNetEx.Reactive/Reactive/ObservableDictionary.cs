using System;
using System.Collections.Generic;

namespace DotNetEx.Reactive
{
	/// <summary>
	/// Represents an observable collection of keys and values.
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public class ObservableDictionary<TKey, TValue> : ObservableList<ObservableKeyValuePair<TKey, TValue>>
	{
		/// <summary>
		/// Creates a new instance of an observable dictionary using EqualityComparer&lt;TKey&gt;.Default
		/// as default key comparer.
		/// </summary>
		public ObservableDictionary() :
			this( EqualityComparer<TKey>.Default )
		{
		}


		/// <summary>
		/// Creates a new instance of an observable dictionary.
		/// </summary>
		/// <param name="comparer">The equality comparer to be used when comparing keys.</param>
		public ObservableDictionary( IEqualityComparer<TKey> comparer )
		{
			Check.NotNull( comparer, "comparer" );

			m_keys = new Dictionary<TKey, Int32>( comparer );
		}


		/// <summary>
		/// Creates a new instance of an observable dictionary and copies the values using the provided 
		/// key function to extract associated keys.
		/// </summary>
		public ObservableDictionary( Func<TValue, TKey> key, IEnumerable<TValue> values ) :
			this( EqualityComparer<TKey>.Default )
		{
			Check.NotNull( key, "key" );
			Check.NotNull( values, "values" );

			foreach ( var value in values )
			{
				this.Add( key( value ), value );
			}
		}


		/// <summary>
		///  Determines whether the collection contains the specified key. 
		/// </summary>
		public Boolean ContainsKey( TKey key )
		{
			return m_keys.ContainsKey( key );
		}


		/// <summary>
		/// Removes the value located at the specified key.
		/// </summary>
		/// <returns>True if the key existed, False when it didn't</returns>
		public Boolean Remove( TKey key )
		{
			Int32 index = this.IndexOf( key );

			if ( index > -1 )
			{
				this.RemoveAt( index );

				return true;
			}

			return false;
		}


		/// <summary>
		/// Returns the index of the provided key inside the current collection or -1 if not found.
		/// </summary>
		public Int32 IndexOf( TKey key )
		{
			Int32 index;

			if ( !m_keys.TryGetValue( key, out index ) )
			{
				index = -1;
			}

			return index;
		}


		/// <summary>
		/// Gets the value associated with the specified key. Returns True if the key was found, False otherwise.
		/// </summary>
		public Boolean TryGetValue( TKey key, out TValue value )
		{
			Int32 index = this.IndexOf( key );
			value = index > -1 ? this[ index ].Value : default( TValue );

			return index > -1;
		}


		/// <summary>
		/// Returns the value associated with the specified key.
		/// </summary>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">the key doesn't exist</exception>
		public TValue GetValue( TKey key )
		{
			Int32 index = this.IndexOf( key );

			if ( index < 0 )
			{
				throw new KeyNotFoundException( String.Format( "The key {0} was not found.", key ) );
			}

			return this[ index ].Value;
		}


		/// <summary>
		/// Adds the specified key and value to the dictionary.
		/// </summary>
		/// <exception cref="System.ArgumentException">An element with the same key already exists in the collection.</exception>
		public void Add( TKey key, TValue value )
		{
			this.Add( new ObservableKeyValuePair<TKey, TValue>( key, value ) );
		}


		/// <summary>
		/// Adds the value in case the key doesn't already exist in the dictionary
		/// or updates it otherwise.
		/// </summary>
		public void AddOrUpdate( TKey key, TValue value )
		{
			var index = this.IndexOf( key );

			if ( index > -1 )
			{
				this[ index ].Value = value;
			}
			else
			{
				this.Add( key, value );
			}
		}


		protected override void OnInsert( ObservableKeyValuePair<TKey, TValue> item, Int32 index )
		{
			try
			{
				m_keys.Add( item.Key, index );
			}
			catch ( ArgumentException )
			{
				throw new ArgumentException( String.Format( "An element with the key '{0}' already exists in the collection.", item.Key ) );
			}

			base.OnInsert( item, index );
		}


		protected override void OnRemove( ObservableKeyValuePair<TKey, TValue> item )
		{
			m_keys.Remove( item.Key );

			base.OnRemove( item );
		}


		protected override void OnMove( ObservableKeyValuePair<TKey, TValue> item, Int32 newIndex )
		{
			m_keys[ item.Key ] = newIndex;

			base.OnMove( item, newIndex );
		}


		private readonly Dictionary<TKey, Int32> m_keys; // Mapping between a key and the position of that key in the current collection
	}
}
