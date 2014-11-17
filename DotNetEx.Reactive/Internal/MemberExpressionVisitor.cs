using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DotNetEx.Reactive
{
	internal sealed class MemberExpressionVisitor<T> : ExpressionVisitor
	{
		public MemberExpressionVisitor()
		{
			this.Members = new HashSet<String>();
		}


		/// <summary>
		/// Gets the members used in the expression.
		/// </summary>
		public HashSet<String> Members { get; private set; }


		protected override Expression VisitMember( MemberExpression node )
		{
			if ( node.Member.DeclaringType == typeof( T ) )
			{
				this.Members.Add( node.Member.Name );
			}

			return base.VisitMember( node );
		}
	}
}
