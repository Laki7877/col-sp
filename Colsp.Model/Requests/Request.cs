namespace Colsp.Model.Requests
{
    public abstract class Request
	{
		public static T GetValueOrDefault<T>(T obj, T def)
		{
			return obj == null ? def : obj;
		}
		public virtual bool Validate()
		{
			return false;
		}
		public virtual void DefaultOnNull()
		{

		}
	}
}
