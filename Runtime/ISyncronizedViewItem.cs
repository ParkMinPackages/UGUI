namespace com.mutant.ugui
{
	public interface ISyncronizedViewItem<TData>
	{
		void Spawn(TData data);
		void Remove();
	}
}