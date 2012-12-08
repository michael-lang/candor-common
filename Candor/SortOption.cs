using System;

namespace Candor
{
	/// <summary>
	/// Contains options for sorting a large collection of items.
	/// </summary>
	[Serializable]
	public class SortOption
	{
		private string propertyName_ = null;
		private short direction_ = 1;

		/// <summary>
		/// Creates a new instance of a SortOption.
		/// </summary>
		/// <param name="propertyName">The name of the property to sort by.</param>
		/// <param name="direction">The direction of the sort.</param>
		public SortOption( string propertyName, short direction )
		{
			propertyName_ = propertyName;
			direction_ = direction;
		}

		/// <summary>
		/// Gets or sets the name of the property to sort by.
		/// </summary>
		public string PropertyName
		{
			get { return propertyName_; }
			set { propertyName_ = value ?? string.Empty; }
		}
		/// <summary>
		/// Gets or sets the direction of the sort.  positive (1) for ascending (default), negative (-1) for descending
		/// </summary>
		public short Direction
		{
			get { return direction_; }
			set { direction_ = value; }
		}
	}
}