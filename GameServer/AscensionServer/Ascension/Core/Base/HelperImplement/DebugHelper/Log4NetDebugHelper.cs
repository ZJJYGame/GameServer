using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmos;
using ExitGames.Logging;

namespace AscensionServer
{
    public class Log4NetDebugHelper : Utility.Debug.IDebugHelper
    {
        ILogger _Logger ;
        public Log4NetDebugHelper()
        {
            _Logger = LogManager.GetCurrentClassLogger();
        }
        public void LogError(object msg, object context=null)
        {
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace str = new StackTrace(new StackFrame(3, true));
            StackTrace stri = new StackTrace(new StackFrame(4, true));
            context = context == null ? "null" : context;
            _Logger.Error($"Message :{msg}, Context:{context};\nStackTrace[ - ]：\n{st}{str}{stri}");
        }

        public void LogInfo(object msg, object context=null)
        {
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace str = new StackTrace(new StackFrame(3, true));
            StackTrace stri = new StackTrace(new StackFrame(4, true));
            context = context == null ? "null" : context;
            _Logger.Info($"Message :{msg}, Context:{context};\nStackTrace[ - ]：\n{st}{str}{stri}");
        }
        public void LogInfo(object msg, string msgColor, object context=null)
        {
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace str = new StackTrace(new StackFrame(3, true));
            StackTrace stri = new StackTrace(new StackFrame(4, true));
            context = context == null ? "null" : context;
            _Logger.Info($"Message :{msg}, Context:{context};\nStackTrace[ - ]：\n{st}{str}{stri}");
        }
        public void LogWarning(object msg, object context)
        {
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace str = new StackTrace(new StackFrame(3, true));
            StackTrace stri = new StackTrace(new StackFrame(4, true));
            context = context == null ? "null" : context;
            _Logger.Warn($"Warning:{msg}, Context:{context};\nStackTrace[ - ]：\n{st}{str}{stri}");
        }
        /// <summary>
        /// 谨慎使用；
        /// 最高级别的报错；
        /// 仅在会导致程序奔溃的位置进行调用；
        /// </summary>
        public void LogFatal(object msg, object context)
        {
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackTrace str = new StackTrace(new StackFrame(3, true));
            StackTrace stri = new StackTrace(new StackFrame(4, true));
            context = context == null ? "null" : context;
            _Logger.Fatal($"Fatal :{msg}, Context:{context};\nStackTrace[ - ]：\n{st}{str}{stri}");
        }
    }
}


