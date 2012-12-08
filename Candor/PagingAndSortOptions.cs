using System;
using System.Collections.Generic;

namespace Candor
{
	/// <summary>
	/// Contains options for paging and sorting a large collection of items.
	/// </summary>
	[Serializable]
	public class PagingAndSortOptions : PagingOptions
	{
		private List<SortOption> sortOptions_ = null;

		#region Constructors
		/// <summary>
		/// Creates a new instance of PagingAndSortOptions
		/// </summary>
		public PagingAndSortOptions() : base() { }
		/// <summary>
		/// Creates a new instance of PagingAndSortOptions
		/// </summary>
		public PagingAndSortOptions( PagingOptions paging )
			: base()
		{
			this.Enabled = paging.Enabled;
			this.PageSize = paging.PageSize;
			this.PageIndex = paging.PageIndex;
			this.TotalItemCount = paging.TotalItemCount;
			if (paging is PagingAndSortOptions)
			{
				this.SortOptions.AddRange(((PagingAndSortOptions)paging).SortOptions);
			}
		}
		#endregion Constructors

		/// <summary>
		/// Gets the sort properties
		/// </summary>
		public List<SortOption> SortOptions
		{
			get
			{
				if (sortOptions_ == null)
					sortOptions_ = new List<SortOption>();
				return sortOptions_;
			}
		}

		#region Specialty
		/// <summary>
		/// Gets a set of PagingOptions with paging disabled.
		/// </summary>
		/// <returns></returns>
		public static PagingAndSortOptions GetDisabledPagingAndSortOptions()
		{
			return new PagingAndSortOptions(PagingOptions.GetDisabledPagingOptions());
		}
		#endregion Specialty
	}
}