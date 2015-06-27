using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace DotNetEx.Reactive
{
	[Serializable]
	public abstract class ObservableObject : IObservableObject
	{
		protected ObservableObject()
		{
		}


		[field: NonSerialized]
		public event PropertyChangedEventHandler PropertyChanged;


		public IObservable<PropertyChangedEventArgs> PropertyChanges
		{
			get
			{
				if ( m_propertyChanges == null )
				{
					m_propertyChanges = new Subject<PropertyChangedEventArgs>();
				}

				return m_propertyChanges;
			}
		}


		public Boolean IsChanged
		{
			get
			{
				return m_isChanged;
			}
			protected set
			{
				if ( m_isChanged != value )
				{
					m_isChanged = value;
					this.RaisePropertyChanged();
				}
			}
		}


		public Boolean IsInitializing
		{
			get
			{
				return m_init > 0;
			}
		}


		public virtual void AcceptChanges()
		{
			if ( this.IsChanged )
			{
				this.IsChanged = false;

				// Propagate the accept changes to nested items
				if ( m_trackable != null && m_trackable.Count > 0 )
				{
					foreach ( var value in m_trackable.Where( x => x.IsChanged ) )
					{
						value.AcceptChanges();
					}
				}
			}
		}


		public void BeginInit()
		{
			if ( ++m_init == 1 )
			{
				this.OnBeginInit();
				this.RaisePropertyChanged( "IsInitializing" );

				// Propagate the begin init to nested items
				if ( m_initializable != null && m_initializable.Count > 0 )
				{
					foreach ( var value in m_initializable )
					{
						value.BeginInit();
					}
				}
			}
		}


		public void EndInit()
		{
			if ( m_init == 0 )
			{
				throw new InvalidOperationException( "BeginInit must be callled before EndInit." );
			}

			if ( --m_init == 0 )
			{
				this.OnEndInit();
				this.RaisePropertyChanged( "IsInitializing" );

				// Propagate the begin init to nested items
				if ( m_initializable != null && m_initializable.Count > 0 )
				{
					foreach ( var value in m_initializable )
					{
						value.EndInit();
					}
				}
			}
		}


		protected Boolean SetValue<T>( ref T value, T newValue, [CallerMemberName] String propertyName = null )
		{
			if ( !EqualityComparer<T>.Default.Equals( value, newValue ) )
			{
				if ( typeof( IChangeTracking ).IsAssignableFrom( typeof( T ) ) )
				{
					this.Detach( (IChangeTracking)value );
					this.Attach( (IChangeTracking)newValue );
				}

				this.OnPropertyChanging( propertyName );

				value = newValue;

				this.OnPropertyChanged( propertyName );
				this.RaisePropertyChanged( propertyName );

				if ( !this.IsInitializing )
				{
					this.IsChanged = true;
				}

				return true;
			}

			return false;
		}


		protected virtual void OnPropertyChanging( String propertyName )
		{
		}


		protected virtual void OnPropertyChanged( String propertyName )
		{
		}


		protected virtual void OnBeginInit()
		{
		}


		protected virtual void OnEndInit()
		{
		}


		protected void RaisePropertyChanged( [CallerMemberName] String propertyName = null )
		{
			var handler = this.PropertyChanged;

			if ( handler != null || m_propertyChanges != null )
			{
				var args = new PropertyChangedEventArgs( propertyName );

				if ( handler != null )
				{
					handler( this, args );
				}

				if ( m_propertyChanges != null )
				{
					m_propertyChanges.OnNext( args );
				}

				foreach ( var referencePropertyName in ReferencesAttribute.Get( this.GetType(), propertyName ) )
				{
					this.RaisePropertyChanged( referencePropertyName );
				}
			}
		}


		private void OnItemPropertyChanged( Object sender, PropertyChangedEventArgs e )
		{
			if ( !this.IsChanged && !this.IsInitializing )
			{
				if ( e.PropertyName == "IsChanged" )
				{
					this.IsChanged = true;
				}
			}
		}


		[OnDeserializing]
		private void OnDeserializing( StreamingContext context )
		{
			this.BeginInit();
		}


		[OnDeserialized]
		private void OnDeserialized( StreamingContext context )
		{
			this.EndInit();
		}


		private void Attach<T>( T item )
			where T : IChangeTracking
		{
			if ( item != null )
			{
				var initializable = item as ISupportInitialize;

				if ( initializable != null )
				{
					if ( m_initializable == null )
					{
						m_initializable = new HashSet<ISupportInitialize>( ReferenceEqualityComparer<ISupportInitialize>.Instance );
					}

					m_initializable.Add( initializable );

					if ( this.IsInitializing )
					{
						initializable.BeginInit();
					}
				}

				if ( m_trackable == null )
				{
					m_trackable = new HashSet<IChangeTracking>( ReferenceEqualityComparer<IChangeTracking>.Instance );
				}

				m_trackable.Add( item );

				var notifiable = item as INotifyPropertyChanged;

				if ( notifiable != null )
				{
					notifiable.PropertyChanged += OnItemPropertyChanged;
				}
			}
		}


		private void Detach<T>( T item )
			where T : IChangeTracking
		{
			if ( item != null )
			{
				var notifiable = item as INotifyPropertyChanged;

				if ( notifiable != null )
				{
					notifiable.PropertyChanged -= OnItemPropertyChanged;
				}

				m_trackable.Remove( item );

				var initializable = item as ISupportInitialize;

				if ( initializable != null )
				{
					m_initializable.Remove( initializable );

					if ( this.IsInitializing )
					{
						initializable.EndInit();
					}
				}
			}
		}


		private Boolean m_isChanged = false;

		[NonSerialized]
		private Subject<PropertyChangedEventArgs> m_propertyChanges;
		
		[NonSerialized]
		private Int32 m_init = 0;
		
		[NonSerialized]
		private HashSet<IChangeTracking> m_trackable;

		[NonSerialized]
		private HashSet<ISupportInitialize> m_initializable;
	}
}