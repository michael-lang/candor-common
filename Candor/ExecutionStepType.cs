namespace Candor
{
	/// <summary>
	/// The types of execution step.
	/// </summary>
	public enum ExecutionStepType
	{
		/// <summary>
		/// Indicates the step was an error condition.
		/// Not necassarily an <see cref="System.Exception"/>.
		/// </summary>
		Error = 1,
		/// <summary>
		/// Indicates the step was warning of a potential mistake.
		/// </summary>
		Warning = 2,
		/// <summary>
		/// Indicates the step is of information purposes only.
		/// </summary>
		Info = 3
	}
}