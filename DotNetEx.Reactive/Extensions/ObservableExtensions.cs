using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using DotNetEx.Reactive.Internal;

namespace DotNetEx.Reactive
{
	public static class ObservableExtensions
	{
		public static DerivedProperty<T> ToProperty<T>( this IObservable<T> source )
		{
			if ( source == null )
			{
				return null;
			}

			return new DerivedProperty<T>( source );
		}


		public static DerivedProperty<T> ToProperty<T>( this IObservable<T> source, T initialValue )
		{
			return new DerivedProperty<T>( source ?? Observable.Empty<T>(), initialValue );
		}


		/// <summary>
		/// An observable which provides values when any of the source properties used in the provided expression has changed.
		/// </summary>
		public static IObservable<TValue> Observe<T, TValue>( this T source, Expression<Func<T, TValue>> expression ) where T : INotifyPropertyChanged
		{
			Check.NotNull( expression, "expression" );

			MemberExpressionVisitor<T> visitor = new MemberExpressionVisitor<T>();
			expression = (Expression<Func<T, TValue>>)visitor.Visit( expression );

			HashSet<String> members = visitor.Members;
			Func<T, TValue> selector = expression.Compile();

			var changes = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>( x => source.PropertyChanged += x, x => source.PropertyChanged -= x );

			if ( members.Count > 0 )
			{
				return changes
					.Where( x => members.Contains( x.EventArgs.PropertyName ) )
					.Select( x => selector( (T)x.Sender ) )
					.Publish( selector( source ) )
					.RefCount()
					.DistinctUntilChanged();
			}

			return changes
				.Select( x => selector( (T)x.Sender ) )
				.Publish( selector( source ) )
				.RefCount()
				.DistinctUntilChanged();
		}


		public static IObservable<RxPropertyChange<T>> Observe<T>( this T source ) where T : INotifyPropertyChanged
		{
			var changes = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>( x => source.PropertyChanged += x, x => source.PropertyChanged -= x );

			return changes.Select( x => new RxPropertyChange<T>( source, x.EventArgs.PropertyName ) );
		}


		public static Boolean RaiseWhenChanged<TSource, TValue>( this PropertyChangedEventHandler handler, TSource source, ref TValue oldValue, TValue newValue, [CallerMemberName] String propertyName = null )
			where TSource : INotifyPropertyChanged
		{
			if ( !Object.Equals( oldValue, newValue ) )
			{
				oldValue = newValue;
				handler.Raise( source, propertyName );

				return true;
			}

			return false;
		}


		public static void Raise<TSource>( this PropertyChangedEventHandler handler, TSource source, [CallerMemberName] String propertyName = null )
			where TSource : INotifyPropertyChanged
		{
			if ( handler != null )
			{
				handler( source, new PropertyChangedEventArgs( propertyName ) );

				foreach ( var reference in ReferencesAttribute.Get( typeof( TSource ), propertyName ) )
				{
					handler.Raise( source, reference );
				}
			}
		}
	}
}
