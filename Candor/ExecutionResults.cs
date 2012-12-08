using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;

namespace Candor
{
	/// <summary>
	/// Tracks the results of an operation for reporting to a method callee.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This type is typically used as the return value of a method when it 
	/// needs to return a boolean flag that an operation is successful, and when 
	/// the operation is not successful it needs to report reasons to an end user.
	/// </para><para>
	/// Commonly used to return validation messages to a user when inputs are 
	/// invalid.  This type can be extended for use by methods that need to
	/// return more specific additional information.
	/// </para><para>
	/// This is not meant to replace runtime exceptions.  A method should still
	/// raise an exception when a failure occurs.
	/// </para>
	/// </remarks>
	/// <example>
	/// Here is an example of proper usage:
	/// <code>
	/// public SomeType MyMethod(int arg1, ExecutionResults result)
	/// {
	///		result.FailOnWarning = false; //default value
	///		if (validationWarning1)
	///		{
	///			result.AddWarning("Be aware that situation 1 occured.");
	///			//	result.Success=true
	///		}
	///		if (validationFailure1)
	///		{
	///			result.AddError("validation 1 failed.");
	///			//	result.Success=false
	///		}
	///		if (result.Success)
	///			return new SomeType(arg1);
	///		else
	///			return null;
	/// }
	/// </code>
	/// <code>
	/// public SomeType MyMethod2(int arg1, ExecutionResults result)
	/// {
	///		result.FailOnWarning = true;
	///		if (validationWarning1)
	///		{
	///			result.AddWarning("Be aware that situation 1 occured.");
	///			//	result.Success=false
	///		}
	///		if (validationFailure1)
	///		{
	///			result.AddError("validation 1 failed.");
	///			//	result.Success=false
	///		}
	///		if (result.Success)
	///			return new SomeType(arg1);
	///		else
	///			return null;
	/// }
	/// </code>
	/// The following is not a good design practice.
	/// <code>
	/// public SomeType MyMethod(int arg1, ExecutionResults result)
	/// {
	///		try
	///		{
	///			//Do something that may cause an exception here...
	///		}
	///		catch (Exception ex)
	///		{	//DO NOT DO THIS!!!
	///			result.AddError(ex.Message);
	///		}
	///		if (result.Success)
	///			return new SomeType(arg1);
	///		else
	///			return null;
	/// }
	/// </code>
	/// </example>
	[Serializable]
	public class ExecutionResults
	{
		#region Fields
		private bool success_ = true;

		private List<ExecutionStep> messages_ = null;
		#endregion Fields

		#region Constructors
		/// <summary>
		/// Creates a new instance of ExecutionResults.
		/// </summary>
		public ExecutionResults()
		{
			messages_ = new List<ExecutionStep>();
		}
		#endregion Constructors

		#region Properties
		/// <summary>
		/// Determines if the results of the overall action performed
		/// were successful (true), or not (false).
		/// </summary>
		public bool Success
		{
			get { return this.success_; }
		}
		/// <summary>
		/// Gets the step messages.
		/// </summary>
		public List<ExecutionStep> Messages { get { return messages_; } }
		#endregion Properties

		#region Methods
		/// <summary>
		/// Fails the result without adding a reason why.
		/// </summary>
		public void Fail()
		{
			this.success_ = false;
		}
		/// <summary>
		/// Combines the results of another ExecutionResults into this instance.
		/// </summary>
		/// <param name="results">The results to be combined.</param>
		public void Combine( ExecutionResults results )
		{
			if (results == null)
				throw new ArgumentNullException("results");
			if (results == this)
				return;

			for (int i = 0; i < results.messages_.Count; i++)
			{
				ExecutionStep step = results.messages_[i];
				this.messages_.Add(step);
				if (step.StepType == ExecutionStepType.Error)
					this.success_ = false;
			}
		}
		/// <summary>
		/// Adds a new info to the result.
		/// </summary>
		/// <param name="message">The info message.</param>
		public void AppendInfo( string message )
		{
			if (message == null)
				return;

			messages_.Add(new ExecutionStep(ExecutionStepType.Info, message));
		}
		/// <summary>
		/// Adds a new info to the result replacing the format items in the
		/// message with the text equivalent of the specified args.  Each
		/// format specification is replaced by the string representation of
		/// the corresponding item in the object argument.
		/// </summary>
		/// <param name="message">The info message format with zero or 
		/// more format items.</param>
		/// <param name="args">An <see cref="System.Object"/>Object</param> 
		/// array containing zero or more items to format.
		public void AppendInfoFormat( string message, params object[] args )
		{
			if (message == null)
				throw new ArgumentNullException("message");
			if (args == null)
				throw new ArgumentNullException("args");

			messages_.Add(new ExecutionStep(ExecutionStepType.Info, string.Format(message, args)));
		}
		/// <summary>
		/// Adds new infos to the result.
		/// </summary>
		/// <param name="messages">An array of info messages.</param>
		public void AppendInfos( string[] messages )
		{
			if (messages == null || messages.Length == 0)
				return;

			for (int i = 0; i < messages.Length; i++)
			{
				messages_.Add(new ExecutionStep(ExecutionStepType.Info, messages[i]));
			}
		}
		/// <summary>
		/// Adds new infos to the result.
		/// </summary>
		/// <param name="messages">A collection of info messages.</param>
		public void AppendInfos( StringCollection messages )
		{
			if (messages == null || messages.Count == 0)
				return;

			for (int i = 0; i < messages.Count; i++)
			{
				messages_.Add(new ExecutionStep(ExecutionStepType.Info, messages[i]));
			}
		}
		/// <summary>
		/// Adds a new warning to the result.
		/// </summary>
		/// <param name="message">The warning message.</param>
		public void AppendWarning( string message )
		{
			if (message == null)
				return;

			messages_.Add(new ExecutionStep(ExecutionStepType.Warning, message));
		}
		/// <summary>
		/// Adds a new warning to the result replacing the format items in the
		/// message with the text equivalent of the specified args.  Each
		/// format specification is replaced by the string representation of
		/// the corresponding item in the object argument.
		/// </summary>
		/// <param name="message">The warning message format with zero or 
		/// more format items.</param>
		/// <param name="args">An <see cref="System.Object"/>Object</param> 
		/// array containing zero or more items to format.
		public void AppendWarningFormat( string message, params object[] args )
		{
			if (message == null)
				throw new ArgumentNullException("message");
			if (args == null)
				throw new ArgumentNullException("args");

			messages_.Add(new ExecutionStep(ExecutionStepType.Warning, string.Format(message, args)));
		}
		/// <summary>
		/// Adds new warnings to the result.
		/// </summary>
		/// <param name="messages">An array of warning messages.</param>
		public void AppendWarnings( string[] messages )
		{
			if (messages == null || messages.Length == 0)
				return;

			for (int i = 0; i < messages.Length; i++)
			{
				messages_.Add(new ExecutionStep(ExecutionStepType.Warning, messages[i]));
			}
		}
		/// <summary>
		/// Adds new warnings to the result.
		/// </summary>
		/// <param name="messages">A collection of warning messages.</param>
		public void AppendWarnings( StringCollection messages )
		{
			if (messages == null || messages.Count == 0)
				return;

			for (int i = 0; i < messages.Count; i++)
			{
				messages_.Add(new ExecutionStep(ExecutionStepType.Warning, messages[i]));
			}
		}
		/// <summary>
		/// Adds a new error to the result.
		/// </summary>
		/// <param name="message">The error message.</param>
		public void AppendError( string message )
		{
			if (message == null)
				return;

			success_ = false;
			messages_.Add(new ExecutionStep(ExecutionStepType.Error, message));
		}
		/// <summary>
		/// Adds a new error to the result replacing the format items in the
		/// message with the text equivalent of the specified args.  Each
		/// format specification is replaced by the string representation of
		/// the corresponding item in the object argument.
		/// </summary>
		/// <param name="message">The error message format with zero or 
		/// more format items.</param>
		/// <param name="args">An <see cref="System.Object"/>Object</param> 
		/// array containing zero or more items to format.
		public void AppendErrorFormat( string message, params object[] args )
		{
			if (message == null)
				throw new ArgumentNullException("message");
			if (args == null)
				throw new ArgumentNullException("args");

			success_ = false;
			messages_.Add(new ExecutionStep(ExecutionStepType.Error, string.Format(message, args)));
		}
		/// <summary>
		/// Adds new errors to the result.
		/// </summary>
		/// <param name="messages">An array of error messages.</param>
		public void AppendErrors( string[] messages )
		{
			if (messages == null || messages.Length == 0)
				return;

			success_ = false;
			for (int i = 0; i < messages.Length; i++)
			{
				messages_.Add(new ExecutionStep(ExecutionStepType.Error, messages[i]));
			}
		}
		/// <summary>
		/// Adds new errors to the result.
		/// </summary>
		/// <param name="messages">A collection of error messages.</param>
		public void AppendErrors( StringCollection messages )
		{
			if (messages == null || messages.Count == 0)
				return;

			success_ = false;
			for (int i = 0; i < messages.Count; i++)
			{
				messages_.Add(new ExecutionStep(ExecutionStepType.Error, messages[i]));
			}
		}
		/// <summary>
		/// Converts the results of this instance into a single string.
		/// </summary>
		/// <returns>A string containing all execution steps
		/// in this instance.</returns>
		/// <example>
		/// An example of a call to a business component from a web page that
		/// contains a Label control called 'messageLabel'.
		/// <code>
		/// ExecutionResults result = SomeClass.SomeMethod(arg1);
		///	messageLabel.Text = result.ToString();
		/// </code>
		/// <code>
		/// ExecutionResults result = new ExecutionResults();
		/// SomeClass.SomeMethod(arg1, result);
		///	messageLabel.Text = result.ToString();
		/// </code>
		/// </example>
		public override string ToString()
		{
			return this.ToString("{StepType} : {Message}", Environment.NewLine);
		}
		/// <summary>
		/// Converts the results of this instance into a single
		/// string using the supplied delimeter.
		/// </summary>
		/// <param name="delimeter">Optional, a string to place
		/// between each execution step.  Pass in
		/// 'null' or an empty string for no delimeter.</param>
		/// <returns>A string containing all errors and warnings
		/// in this instance.</returns>
		/// <example>
		/// An example of a call to a business component from a web page that
		/// contains a Label control called 'messageLabel'.
		/// <code>
		/// ExecutionResults result = SomeClass.SomeMethod(arg1);
		///	messageLabel.Text = result.ToString("&lt;BR&gt;");
		/// </code>
		/// <code>
		/// ExecutionResults result = new ExecutionResults();
		/// SomeClass.SomeMethod(arg1, result);
		///	messageLabel.Text = result.ToString("&lt;BR&gt;");
		/// </code>
		/// </example>
		public string ToString( string delimeter )
		{
			return this.ToString("{StepType} : {Message}", delimeter);
		}
		/// <summary>
		/// Converts the results of this instance into a single
		/// string using the supplied delimeter.
		/// </summary>
		/// <param name="itemFormat">The format for each execution step.
		/// Use '{Message}' in the format string to locate the message.
		/// Use '{StepType}' in the format string to locate the step type name.  
		/// Do not include any other format specifications 
		/// (IE. '{1}', '{2}', etc...)</param>
		/// <param name="delimeter">A string to place between each item.</param>
		/// <returns>A string containing all execution steps
		/// in this instance.</returns>
		/// <example>
		/// An example of a call to a business component from a web page that
		/// contains a Label control called 'messageLabel'.
		/// <code>
		/// ExecutionResults result = SomeClass.SomeMethod(arg1);
		///	messageLabel.Text = result.ToString(
		///		"{StepType}: {Message}", "&lt;BR&gt;");
		/// </code>
		/// <code>
		/// ExecutionResults result = new ExecutionResults();
		/// SomeClass.SomeMethod(arg1, result);
		///	messageLabel.Text = result.ToString(
		///		"{StepType}: {Message}", "&lt;BR&gt;");
		/// </code>
		/// </example>
		public string ToString( string itemFormat, string delimeter )
		{
			if (itemFormat == null)
				itemFormat = "{StepType} : {Message}";
			if (delimeter == null)
				delimeter = string.Empty;

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < this.messages_.Count; i++)
			{
				ExecutionStep item = this.messages_[i];
				string stepTypeName = Enum.GetName(typeof(ExecutionStepType), item.StepType);
				sb.Append(itemFormat
					.Replace("{StepType}", stepTypeName)
					.Replace("{Message}", item.Message));
				if (delimeter.Length > 0)
					sb.Append(delimeter);
			}
			if (delimeter.Length > 0 && sb.Length > delimeter.Length)
			{	//remove last delimeter
				sb.Remove(sb.Length - delimeter.Length, delimeter.Length);
			}
			return sb.ToString();
		}
		/// <summary>
		/// Outputs the results as an Html string.
		/// </summary>
		/// <returns></returns>
		public string ToHtmlString()
		{
			return ToString("{Message}", "<br/>");
		}
		/// <summary>
		/// Resets this instance to a successful state and clears all
		/// error and warning messages.
		/// </summary>
		public void Reset()
		{
			this.success_ = true;
			this.messages_.Clear();
		}
		#endregion Methods
	}
}