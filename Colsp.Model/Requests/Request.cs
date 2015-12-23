using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Colsp.Model.Requests
{
	public abstract class Request
	{
		public static T GetValueOrDefault<T>(T obj, T def)
		{
			return obj == null ? def : obj;
		}
		public bool Validate()
		{
			return _Validate();
		}
		public void DefaultOnNull()
		{
			_DefaultOnNull();
		}
		protected virtual bool _Validate()
		{
			return false;
		}
		protected virtual void _DefaultOnNull()
		{

		}
	}
}
