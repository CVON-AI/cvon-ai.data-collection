namespace Uploader.Logic.Actions
{
    /// <summary>
    /// Contains the result of an <see cref="IAction.Perform"/> method.
    /// </summary>
    public class ActionResult
    {
        public ActionResult(bool succeeded, string causeOfFailure = "")
        {
            this.Succeeded = succeeded;
            this.CauseOfFailure = causeOfFailure;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IAction"/>
        /// instance that returned the current <see cref="ActionResult"/>
        /// instance succeeded when <see cref="IAction.Perform"/> was called.
        /// </summary>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets a cause of failue when the current <see cref="ActionResult.Succeeded"/>
        /// was set to <c>false</c> because an error occurred when <see cref="IAction.Perform"/> was called.
        /// </summary>
        public string CauseOfFailure { get; }
    }
}