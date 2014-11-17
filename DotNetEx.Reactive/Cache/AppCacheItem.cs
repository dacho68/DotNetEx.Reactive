using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEx.Reactive
{
	public sealed class AppCacheItem<T>
	{
		public AppCacheItem( T value, DateTimeOffset expires )
		{
			this.Value		= value;
			this.Expires	= expires;
		}


		public T Value { get; private set; }


		public DateTimeOffset Expires { get; private set; }
	}
}
