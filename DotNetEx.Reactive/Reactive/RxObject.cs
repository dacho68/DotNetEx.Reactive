using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace DotNetEx.Reactive
{
	[DataContract( Namespace = "" )]
	public abstract class RxObject : INotifyPropertyChanged, INotifyPropertyChanging
	{
		protected RxObject()
		{
			m_dependencies = DependsOnAttribute.GetDependencies( this.GetType() );
		}


		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;


		public event PropertyChangingEventHandler PropertyChanging;


		/// <summary>
		/// Gets the display text that represents the current instance.
		/// </summary>
		public virtual String DisplayText
		{
			get
			{
				return String.Empty;
			}
		}


		public override String ToString()
		{
			return this.DisplayText;
		}


		protected virtual void OnPropertyChanging( String propertyName )
		{
		}


		protected virtual void OnPropertyChanged( String propertyName )
		{
		}


		/// <summary>
		/// Raises the property changed event and sets the target value if the value has changed.
		/// </summary>
		protected Boolean RaiseAndSetIfChanged<T>( ref T target, T newValue, [CallerMemberName] String propertyName = null )
		{
			Boolean changed = false;

			if ( !Object.Equals( target, newValue ) )
			{
				this.OnPropertyChanging( propertyName );
				this.RaisePropertyChanging( propertyName );

				target = newValue;
				changed = true;

				this.OnPropertyChanged( propertyName );
				this.RaisePropertyChanged( propertyName );
			}

			return changed;
		}


		/// <summary>
		/// Raises the property changed event for the provided property name.
		/// </summary>
		protected void RaisePropertyChanged( [CallerMemberName] String propertyName = null )
		{
			var handler = this.PropertyChanged;

			if ( handler != null )
			{
				handler( this, new PropertyChangedEventArgs( propertyName ) );

				HashSet<String> dependencies;

				if ( m_dependencies.TryGetValue( propertyName, out dependencies ) )
				{
					foreach ( var otherPropertyName in dependencies )
					{
						this.RaisePropertyChanged( otherPropertyName );
					}
				}
			}
		}


		protected void RaisePropertyChanging( [CallerMemberName] String propertyName = null )
		{
			var handler = this.PropertyChanging;

			if ( handler != null )
			{
				handler( this, new PropertyChangingEventArgs( propertyName ) );

				HashSet<String> dependencies;

				if ( m_dependencies.TryGetValue( propertyName, out dependencies ) )
				{
					foreach ( var otherPropertyName in dependencies )
					{
						this.RaisePropertyChanging( otherPropertyName );
					}
				}
			}
		}


		[OnDeserializing]
		private void OnDeserializing( StreamingContext context )
		{
			m_dependencies = DependsOnAttribute.GetDependencies( this.GetType() );
		}


		private IReadOnlyDictionary<String, HashSet<String>> m_dependencies;
	}
}