using System.Collections.Generic;
using System.Linq;


namespace RabbitHeadersLab
{
	internal class AllHeaders : HeaderSer
	{
		public AllHeaders(params string[] keyValuePairs): base(keyValuePairs)
		{
		}


		internal override IDictionary<string, object> ToArgs()
		{
			var dict = new Dictionary<string, object>
			{
				{ "x-match", "all" }
			};

			foreach (var kv in this)
			{
				dict.Add(kv.Key, kv.Value);
			}

			return dict;
		}


		internal override string ToAbbreviation()
		{
			return string.Format("all:{0}",
				string.Join("", from k in this.Keys.ToList() orderby k select this[k]));
		}
	}
}
