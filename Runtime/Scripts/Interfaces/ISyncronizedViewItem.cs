namespace Interfaces
{
	public interface ISyncronizedViewItem<TData>
	{
		void Spawn(TData data);
		void Remove();
	}
}