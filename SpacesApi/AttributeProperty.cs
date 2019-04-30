namespace SpacesApi
{
	public class AttributeProperty<T>
	{
		public AttributeProperty()
		{
			this.propertyValue = default(T);
			this.isPropertySet = false;
		}

		public bool IsPropertySet
		{
			get
			{
				return this.isPropertySet;
			}
		}

		public T Value
		{
			get
			{
				return this.propertyValue;
			}
			set
			{
				this.propertyValue = value;
				this.isPropertySet = true;
			}
		}

		public static implicit operator T(AttributeProperty<T> m)
		{
			return m.propertyValue;
		}

		private T propertyValue;

		private bool isPropertySet;
	}
}
