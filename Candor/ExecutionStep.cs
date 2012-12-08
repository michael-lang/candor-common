using System;

namespace Candor
{
	/// <summary>
	/// A single completed step within an overall logical execution plan.
	/// </summary>
	[Serializable]
	public class ExecutionStep
	{
		private ExecutionStepType type_ = ExecutionStepType.Info;
		private string message_ = string.Empty;

		/// <summary>
		/// Creates a new instance of ExecutionStep
		/// </summary>
		/// <param name="type">The type of step.</param>
		/// <param name="message">A descriptive message.</param>
		public ExecutionStep( ExecutionStepType type, string message )
		{
			this.type_ = type;
			if (message != null)
				this.message_ = message;
		}

		/// <summary>
		/// Gets the type of execution step.
		/// </summary>
		public ExecutionStepType StepType
		{
			get { return this.type_; }
		}
		/// <summary>
		/// Gets the associated descriptive message.
		/// </summary>
		public string Message
		{
			get { return this.message_; }
		}
	}
}