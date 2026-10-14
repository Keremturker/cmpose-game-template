using System;

namespace Kturker.UnityBase.Editor.Build
{
    // Bad build input: BuildScript logs 'CMPOSE_BUILD_RESULT result=BadInput reason=<message>' and exits with 2
    public sealed class BuildInputException : Exception
    {
        public BuildInputException(string message) : base(message)
        {
        }
    }
}
