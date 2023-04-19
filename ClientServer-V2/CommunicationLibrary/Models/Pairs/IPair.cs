using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Models.Pairs
{
	public interface IPair<TA, TB>
	{
		public TA A { get; set; }
		public TB B { get; set; }
	}
}
