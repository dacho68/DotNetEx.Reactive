using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using DotNetEx.Internal;

namespace DotNetEx.Reactive
{
	[Serializable]
	public abstract class ObservableObject : IObservableObject, IChangeTracking, ISupportInitialize
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
				return m_init;
			}
		}


		public virtual void AcceptChanges()
		{
			if ( this.IsChanged )
			{
				this.IsChanged = false;

				// Propagate the accept changes to nested items
				if ( m_acceptChanges != null )
				{
					m_acceptChanges();
				}
			}
		}


		public void BeginInit()
		{
			if ( m_init )
			{
				throw new InvalidOperationException( "The object is already being initialized." );
			}

			m_init = true;
			this.RaisePropertyChanged( "IsInitializing" );
		}


		public void EndInit()
		{
			if ( !m_init )
			{
				throw new InvalidOperationException( "BeginInit must be callled before EndInit." );
			}

			m_init = false;
			this.RaisePropertyChanged( "IsInitializing" );
		}


		/// <summary>
		/// Sets the value at the target location without triggering any events. Use this method
		/// instead of the combination BeginInit + SetValue + EndInit.
		/// </summary>
		protected void InitValue<T>( ref T target, T value )
		{
			this.Detach( target );
			this.Attach( value );

			target = value;
		}


		/// <summary>
		/// Sets the value at the target location only if it is different. The comparison is done using EqualityComparer&lt;T&gt;.Default.
		/// If the value is changed then property changed notifications are triggered.
		/// </summary>
		/// <returns>whether the value has changed</returns>
		protected Boolean SetValue<T>( ref T value, T newValue, [CallerMemberName] String propertyName = null )
		{
			if ( !EqualityComparer<T>.Default.Equals( value, newValue ) )
			{
				this.Detach( value );
				this.Attach( newValue );

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

				var referencedProperties = ReferencesAttribute.Get( this.GetType(), propertyName );

				if ( referencedProperties.Count > 0 )
				{
					foreach ( var referencePropertyName in referencedProperties )
					{
						this.RaisePropertyChanged( referencePropertyName );
					}
				}
			}
		}


		private void OnItemPropertyChanged( Object sender, PropertyChangedEventArgs e )
		{
			if ( !this.IsChanged && !this.IsInitializing )
			{
				if ( ( (IChangeTracking)sender ).IsChanged )
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
		{
			if ( !Object.ReferenceEquals( item, null ) )
			{
				if ( ReflectionTraits.Assignable<IChangeTracking, T>.Value )
				{
					m_acceptChanges += ( (IChangeTracking)item ).AcceptChanges;

					if ( ReflectionTraits.Assignable<INotifyPropertyChanged, T>.Value )
					{
						( (INotifyPropertyChanged)item ).PropertyChanged += OnItemPropertyChanged;
					}
				}
			}
		}


		private void Detach<T>( T item )
		{
			if ( !Object.ReferenceEquals( item, null ) )
			{
				if ( ReflectionTraits.Assignable<IChangeTracking, T>.Value )
				{
					m_acceptChanges -= ( (IChangeTracking)item ).AcceptChanges;

					if ( ReflectionTraits.Assignable<INotifyPropertyChanged, T>.Value )
					{
						( (INotifyPropertyChanged)item ).PropertyChanged -= OnItemPropertyChanged;
					}
				}
			}
		}


		private Boolean m_isChanged = false;

		[NonSerialized]
		private Subject<PropertyChangedEventArgs> m_propertyChanges;
		
		[NonSerialized]
		private Boolean m_init = false;
		
		[NonSerialized]
		private Action m_acceptChanges = null;
	}
}