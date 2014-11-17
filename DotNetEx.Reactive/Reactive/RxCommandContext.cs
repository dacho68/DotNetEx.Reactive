using System;

namespace DotNetEx.Reactive
{
	public sealed class RxCommandContext
	{
		internal RxCommandContext( Object parameter )
		{
			this.Parameter	= parameter;
			this.Cancel		= false;
		}


		/// <summary>
		/// Gets or sets the parameter with which the command will be executed.
		/// </summary>
		public Object Parameter { get; set; }


		/// <summary>
		/// Gets or sets a value indicating whether the command will be cancelled. Default is False.
		/// </summary>
		public Boolean Cancel { get; set; }
	}
}