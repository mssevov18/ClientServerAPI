using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationLibrary.Models.Pairs
{
	public class Pair<TA, TB> : IPair<TA, TB>
	{
		public virtual TA A
		{
			get => _innerValue;
			set => this._innerValue = value;
		}
		public virtual TB B
		{
			get => (TB)Convert.ChangeType(_innerValue, typeof(TA));
			set => _innerValue = (TA)Convert.ChangeType(value, typeof(TA));
		}
		private TA _innerValue;

		public static implicit operator TA(Pair<TA, TB> pair) => pair.A;
		public static implicit operator TB(Pair<TA, TB> pair) => pair.B;

		public static implicit operator Pair<TA, TB>(TA a) => new Pair<TA, TB> { A = a };
		public static implicit operator Pair<TA, TB>(TB b) => new Pair<TA, TB> { B = b };
	}
}
