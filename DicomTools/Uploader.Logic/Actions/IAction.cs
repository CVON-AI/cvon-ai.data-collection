namespace Uploader.Logic.Actions
{
    internal interface IAction
    {
        ActionResult Perform();
        //bool Perform(IEnumerable<IActionArgument> arguments);
    }
}
