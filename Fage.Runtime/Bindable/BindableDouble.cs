using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fage.Runtime.Bindable
{
	public class BindableDouble : BindableNumber<double>
	{
		public BindableDouble(double defaultValue) : base(defaultValue)
		{

		}

		protected sealed override double DefaultMinValue => double.MinValue;
		protected sealed override double DefaultMaxValue => double.MaxValue;

		protected sealed override double ClampValue(double newValue, double minValue, double maxValue)
		{
			return Math.Clamp(newValue, minValue, maxValue);
		}
	}
}