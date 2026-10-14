using System;
using System.Text.RegularExpressions;
using Kturker.UnityBase.Core.Logging;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Kturker.UnityBase.Tests.EditMode
{
    public sealed class UnityLogTests
    {
        [Test]
        public void Quiet_log_drops_debug_and_info_but_keeps_warnings()
        {
            var log = new UnityLog(verbose: false);
            log.Debug("d");
            log.Info("i");
            LogAssert.Expect(LogType.Warning, "[UnityBase] w");
            log.Warning("w");
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void Verbose_log_prints_debug_and_info_with_prefix()
        {
            var log = new UnityLog(verbose: true);
            LogAssert.Expect(LogType.Log, "[UnityBase] d");
            LogAssert.Expect(LogType.Log, "[UnityBase] i");
            log.Debug("d");
            log.Info("i");
        }

        [Test]
        public void Markers_print_without_prefix_even_when_quiet()
        {
            LogAssert.Expect(LogType.Log, "CMPOSE_SCENE Menu");
            new UnityLog(verbose: false).Marker("CMPOSE_SCENE Menu");
        }

        [Test]
        public void Error_logs_message_and_exception()
        {
            LogAssert.Expect(LogType.Error, "[UnityBase] e");
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException: boom"));
            new UnityLog(verbose: false).Error("e", new InvalidOperationException("boom"));
        }
    }
}
