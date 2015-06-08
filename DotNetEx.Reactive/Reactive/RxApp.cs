using System;
using System.Reactive.Subjects;

namespace DotNetEx.Reactive
{
	public static class RxApp
	{
		/// <summary>
		/// Gets the errors observable.
		/// </summary>
		public static IObservable<Exception> Errors
		{
			get
			{
				return s_errors;
			}
		}


		internal static void PublishError( Exception error )
		{
			lock ( s_errors )
			{
				s_errors.OnNext( error );
			}
		}


		private static readonly Subject<Exception> s_errors = new Subject<Exception>();
	}
}
