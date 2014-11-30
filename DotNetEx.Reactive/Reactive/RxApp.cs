using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using DotNetEx.Reactive.Internal;

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
