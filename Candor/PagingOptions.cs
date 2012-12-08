using System;

namespace Candor
{
	/// <summary>
	/// Contains options for paging a large collection of items.
	/// </summary>
	[Serializable]
	public class PagingOptions
	{
		private int pageIndex_ = 0;
		private int pageSize_ = 50;
		private int totalItems_ = -1;
		private bool enabled_ = true;

		/// <summary>
		/// Gets or sets an indication if paging is enabled.
		/// </summary>
		/// <remarks>
		/// If false, all applicable items should be returned.  
		/// If true (default), then only the items on the specified
		/// page should be returned.
		/// </remarks>
		public bool Enabled
		{
			get { return this.enabled_; }
			set { this.enabled_ = value; }
		}
		/// <summary>
		/// Gets or sets the index of the page.
		/// </summary>
		public int PageIndex
		{
			get { return pageIndex_; }
			set { pageIndex_ = Math.Max(0, value); }
		}
		/// <summary>
		/// Gets or sets the number of items contained on each page.
		/// </summary>
		public int PageSize
		{
			get { return pageSize_; }
			set { pageSize_ = Math.Max(1, value); }
		}
		/// <summary>
		/// Gets or sets the total number of items.
		/// </summary>
		public int TotalItemCount
		{
			get { return totalItems_; }
			set { totalItems_ = Math.Max(0, value); }
		}

		#region Methods
		/// <summary>
		/// Gets the number of items on the current page as determined by
		/// <see cref="PageIndex"/>.  This will be the lesser of 
		/// <see cref="TotalItemCount"/> minus the items on the previous pages
		/// and the <see cref="PageSize"/>; but no less than zero.
		/// </summary>
		/// <returns>
		/// An integer between zero and <see cref="PageSize"/>
		/// </returns>
		public int GetItemCountForPage()
		{
			int items = Math.Max(0, TotalItemCount);
			if (!enabled_ || PageSize > TotalItemCount)
				return items;
			else
				return Math.Max(0, Math.Min(PageSize, items - (PageIndex * PageSize)));
		}
		/// <summary>
		/// Gets the total number of pages given the current paging details.
		/// </summary>
		/// <returns></returns>
		public int GetTotalPageCount()
		{
			int items = Math.Max(0, TotalItemCount);
			if (!enabled_ || PageSize > TotalItemCount)
				return Math.Min(items, 1); // 0 or 1
			else
				return (int)Math.Ceiling((double)items / (double)PageSize);
		}
		#endregion Methods

		#region Specialty
		/// <summary>
		/// Gets a set of PagingOptions with paging disabled.
		/// </summary>
		/// <returns></returns>
		public static PagingOptions GetDisabledPagingOptions()
		{
			PagingOptions paging = new PagingOptions();
			paging.Enabled = false;
			return paging;
		}
		/// <summary>
		/// Creates a new paging options based on a nullable page and page size.
		/// </summary>
		/// <param name="page">Either null, or a 1 based page number (not page index)</param>
		/// <param name="itemsPerPage">Either null for the default, or the number of items for each page.</param>
		/// <returns></returns>
		public static PagingOptions GetPagingByPage( int? page, int? itemsPerPage )
		{
			if (!page.HasValue && !itemsPerPage.HasValue)
				return PagingOptions.GetDisabledPagingOptions();

			if (!itemsPerPage.HasValue)
				return new PagingOptions() { PageIndex = page.Value - 1 };
			if (!page.HasValue)
				return new PagingOptions() { PageIndex = 0, PageSize = itemsPerPage.Value };

			return new PagingOptions() { PageIndex = page.Value - 1, PageSize = itemsPerPage.Value };
		}
		#endregion Specialty
	}
}