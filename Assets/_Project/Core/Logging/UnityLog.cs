using System;
using Kturker.UnityBase.Core.Domain;

namespace Kturker.UnityBase.Core.Logging
{
    // Debug and Info reach the console only when verbose (development builds); Warning, Error and markers always do
    public sealed class UnityLog : ILog
    {
        private const string Prefix = "[UnityBase] ";
        private readonly bool verbose;

        public UnityLog(bool verbose) => this.verbose = verbose;

        public void Debug(string message)
        {
            if (verbose) UnityEngine.Debug.Log(Prefix + message);
        }

        public void Info(string message)
        {
            if (verbose) UnityEngine.Debug.Log(Prefix + message);
        }

        public void Warning(string message) => UnityEngine.Debug.LogWarning(Prefix + message);

        public void Error(string message, Exception exception = null)
        {
            UnityEngine.Debug.LogError(Prefix + message);
            if (exception != null) UnityEngine.Debug.LogException(exception);
        }

        public void Marker(string line) => UnityEngine.Debug.Log(line);
    }
}
