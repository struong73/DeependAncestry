using System;
using DeependAncestry.Web.Interface;
using log4net;

namespace DeependAncestry.Web.Helper
{
    public class Logger : ILogger
    {
        private static ILog _log = LogManager.GetLogger("ErrorLogger");

        public void Debug(string msg)
        {
            _log.Debug(msg);
        }

        public void Error(Exception ex)
        {
            _log.Error(ex);
        }

        public void Error(string msg, Exception ex)
        {
            _log.Error(msg, ex);
        }

        void ILogger.Info(object msg)
        {
            _log.Info(msg);
        }
    }
}