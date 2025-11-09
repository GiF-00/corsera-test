namespace DataAcquisition.Logger.DefaultLogger
{
    /// <summary>
    /// Class <c>BlankLogger</c> implements <c>ILogger</c> interface to discard any log messages.
    /// </summary>
    public class BlankLogger : ILogger
    {
        public void Info(params string[] messages)
        {
        }

        public void Warning(params string[] messages)
        {
        }

        public void Error(params string[] messages)
        {
        }

		public void Dispose()
		{
		}
	}
}
