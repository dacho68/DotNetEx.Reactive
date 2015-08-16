using System;
using System.Collections.Generic;

namespace DotNetEx.Reactive
{
	internal static class Check
	{
		/// <summary>
		/// Checks the provided arguments is not null.
		/// </summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <exception cref="System.ArgumentNullException">When the provided argument is null.</exception>
		public static void NotNull( Object arg, String argName )
		{
			if ( Object.ReferenceEquals( arg, null ) )
			{
				throw new ArgumentNullException( argName );
			}
		}


		/// <summary>
		/// Checks the provided arguments is not null, empty or contains only white spaces.
		/// </summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <exception cref="System.ArgumentException">When the argument is null, empty or contains only white space.</exception>
		public static void NotEmpty( String arg, String argName )
		{
			if ( String.IsNullOrWhiteSpace( arg ) )
			{
				throw new ArgumentException( argName + " cannot be an empty string" );
			}
		}


		/// <summary>
		/// Checks the argument is in the specified range.
		/// </summary>
		/// <param name="arg">The argument.</param>
		/// <param name="argName">Name of the argument.</param>
		/// <param name="minValue">The minimum inclusive value.</param>
		/// <param name="maxValue">The maximum inclusive value.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">When the argument is out of range.</exception>
		public static void OutOfRange<T>( T arg, String argName, T minValue, T maxValue ) where T : IComparable<T>
		{
			var comparer = Comparer<T>.Default;

			if ( comparer.Compare( arg, minValue ) == -1 )
			{
				throw new ArgumentOutOfRangeException( argName + " cannot be less than " + minValue );
			}

			if ( comparer.Compare( arg, maxValue ) == 1 )
			{
				throw new ArgumentOutOfRangeException( argName + " cannot be greater than " + maxValue );
			}
		}


		/// <summary>
		/// Checks the specified condition and throws InvalidOperationException with the provided message when fails.
		/// </summary>
		/// <exception cref="System.NotImplementedException">When the condition is false.</exception>
		public static void Invariant( Boolean condition, String errorMessage )
		{
			if ( !condition )
			{
				throw new InvalidOperationException( errorMessage );
			}
		}
	}
}
