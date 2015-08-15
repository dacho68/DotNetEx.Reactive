using System;

namespace DotNetEx.Reactive
{
	/// <summary>
	/// Represents and observable key value pair where the value can be modified and observed.
	/// </summary>
	public sealed class ObservableKeyValuePair<TKey, TValue> : ObservableObject
	{
		/// <summary>
		/// Creas a new key value pair only using the key. The value will have default( TValue ) but can be changed at any time
		/// where the key is read only.
		/// </summary>
		public ObservableKeyValuePair( TKey key )
		{
			Check.NotNull( key, "key" );

			m_key = key;
		}


		/// <summary>
		/// Creates a new key value pair using the provided key and value.
		/// </summary>
		public ObservableKeyValuePair( TKey key, TValue value )
		{
			Check.NotNull( key, "key" );

			m_key = key;

			this.InitValue( ref m_value, value );
		}


		/// <summary>
		/// Returns the key associated with this instance.
		/// </summary>
		public TKey Key
		{
			get
			{
				return m_key;
			}
		}


		/// <summary>
		/// Gets or sets the value associated with the current key.
		/// </summary>
		public TValue Value
		{
			get
			{
				return m_value;
			}
			set
			{
				this.SetValue( ref m_value, value );
			}
		}


		private readonly TKey m_key;
		private TValue m_value;
	}
}
