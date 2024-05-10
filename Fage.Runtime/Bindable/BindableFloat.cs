using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fage.Runtime.Bindable
{
	public class BindableFloat : BindableNumber<float>
	{
		public BindableFloat(float defaultValue) : base(defaultValue)
		{

		}

		protected sealed override float DefaultMinValue => float.MinValue;
		protected sealed override float DefaultMaxValue => float.MaxValue;

		protected sealed override float ClampValue(float newValue, float minValue, float maxValue)
		{
			return MathHelper.Clamp(newValue, minValue, maxValue);
		}
	}
}