using System.Collections.Generic;


namespace RabbitHeadersLab
{
	public abstract class HeaderSer : Dictionary<string, string>
	{
		public HeaderSer(params string[] keyValuePairs)
		{
			for (var i = 0; i < keyValuePairs.Length; i += 2)
			{
				this.Add(keyValuePairs[i], keyValuePairs[i + 1]);
			}
		}


		internal abstract IDictionary<string, object> ToArgs();


		internal abstract string ToAbbreviation();
	}
}
