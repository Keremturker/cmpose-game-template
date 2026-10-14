using System;

namespace Kturker.UnityBase.Core.Domain
{
    // Logging contract for code that must not depend on the engine
    public interface ILog
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception exception = null);

        // Machine-readable CMPOSE_* lines read by the gate and the smoke run; printed in every build
        void Marker(string line);
    }
}
