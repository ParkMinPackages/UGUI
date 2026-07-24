namespace com.parkminpackages.ugui
{
	public interface ISyncronizedViewItem<TData>
	{
		void Spawn(TData data);
		void Remove();
	}
}