using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Irc
{
	public class HostMask
	{
		public string Mask { get; private set; }

		public string Nickname
		{
			get
			{
				if (!Mask.Contains('!'))
					return Mask;
				return Mask.Remove(Mask.IndexOf('!'));
			}
		}

		public string Server
		{
			get
			{
				return Mask.Trim('@', '!', '+', '&', '$', '^', '%', '?', '~');
			}
		}

		public HostMask(string mask)
		{
			Mask = mask;
		}
	}
}
