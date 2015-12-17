namespace KVCrawler
{
	public class Workitem
	{
		public int Plz;
		public int Start;
		public bool recurse;

		public override string ToString()
		{
			if (recurse)
				return string.Format("{0} (Alle)", Plz);
			else
				return string.Format("{0} ({1})", Plz, Start / 10 + 1);
		}
	}
}