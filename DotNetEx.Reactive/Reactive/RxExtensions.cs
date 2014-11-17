using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reactive.Linq;
using DotNetEx.Reactive.Internal;
using System.Linq;
using System.Collections.Specialized;
using System.ComponentModel;

namespace DotNetEx.Reactive
{
	public static class ReactiveExtensions
	{
		public static DerivedProperty<T> ToProperty<T>( this IObservable<T> source )
		{
			return new DerivedProperty<T>( source );
		}


		public static DerivedProperty<T> ToProperty<T>( this IObservable<T> source, T initialValue )
		{
			return new DerivedProperty<T>( source, initialValue );
		}


		public static IDisposable Execute<T>( this T source, Expression<Action<T>> expression ) where T : INotifyPropertyChanged
		{
			Check.NotNull( expression, "expression" );

			MemberExpressionVisitor<T> visitor = new MemberExpressionVisitor<T>();

			expression = (Expression<Action<T>>)visitor.Visit( expression );

			HashSet<String> members	= visitor.Members;
			Action<T> action		= expression.Compile();

			var changes = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
								x => source.PropertyChanged += x,
								x => source.PropertyChanged -= x );

			if ( members.Count > 0 )
			{
				return changes
							.Where( x => members.Contains( x.EventArgs.PropertyName ) )
							.ObserveOn( RxApp.MainScheduler )
							.Subscribe( x => action( (T)x.Sender ) );
			}

			return changes.Subscribe( x => action( (T)x.Sender ) );
		}


		/// <summary>
		/// An observable which provides values when any of the source properties used in the provided expression has changed.
		/// </summary>
		public static IObservable<Boolean> When<T>( this T source, Expression<Func<T, Boolean>> expression ) where T : INotifyPropertyChanged
		{
			Check.NotNull( expression, "expression" );

			return source.ToObservable( expression );
		}


		/// <summary>
		/// An observable which provides values when any of the source properties used in the provided expression has changed.
		/// </summary>
		public static IObservable<TValue> ToObservable<T, TValue>( this T source, Expression<Func<T, TValue>> expression ) where T : INotifyPropertyChanged
		{
			Check.NotNull( expression, "expression" );

			MemberExpressionVisitor<T> visitor = new MemberExpressionVisitor<T>();

			expression = (Expression<Func<T, TValue>>)visitor.Visit( expression );

			HashSet<String> members		= visitor.Members;
			Func<T, TValue> selector	= expression.Compile();

			var changes = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
								x => source.PropertyChanged += x,
								x => source.PropertyChanged -= x );

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


		public static IObservable<Boolean> WhenAny<T>( this T source, Func<T, Boolean> predicate ) where T : INotifyPropertyChanged
		{
			Check.NotNull( predicate, "predicate" );

			var changes = Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(
								x => source.PropertyChanged += x,
								x => source.PropertyChanged -= x );

			return changes
						.Select( x => predicate( (T)x.Sender ) )
						.Publish( predicate( source ) )
						.RefCount()
						.DistinctUntilChanged();
		}


		public static RxList<T> Filter<T>( this RxList<T> source, Expression<Func<T, Boolean>> expression )
		{
			Check.NotNull( expression, "expression" );

			if ( source == null )
			{
				return null;
			}

			MemberExpressionVisitor<T> visitor = new MemberExpressionVisitor<T>();

			expression = (Expression<Func<T, Boolean>>)visitor.Visit( expression );

			HashSet<String> members		= visitor.Members;
			Func<T, Boolean> predicate	= expression.Compile();
			RxList<T> result			= new RxList<T>( source.Where( predicate ) );

			IObservable<Boolean> collectionChanges = source.CollectionChanges.Select( x => true );
			IObservable<Boolean> propertyChanges;

			if ( members.Count > 0 )
			{
				propertyChanges = source.ChildrenPropertyChanges.Where( x => members.Contains( x.PropertyName ) ).Select( x => true );
			}
			else
			{
				propertyChanges = source.ChildrenPropertyChanges.Select( x => true );
			}

			Observable.Merge( collectionChanges, propertyChanges ).Subscribe( x =>
			{
				Int32 resultIndex = 0;

				for ( Int32 i = 0; i < source.Count; ++i )
				{
					T item = source[ i ];

					if ( predicate( item ) )
					{
						if ( result.Count > resultIndex )
						{
							if ( !Object.ReferenceEquals( result[ resultIndex ], item ) )
							{
								result[ resultIndex ] = item;
							}
						}
						else
						{
							result.Add( item );
						}

						++resultIndex;
					}
					else if ( result.Count > resultIndex )
					{
						if ( Object.ReferenceEquals( result[ resultIndex ], item ) )
						{
							result.RemoveAt( resultIndex );
						}
					}
				}
			} );

			return result;
		}
	}
}
