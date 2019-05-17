using System;
using MicroBatchFramework;
using MicroBatchFramework.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Unity;
using Unity.Microsoft.DependencyInjection;
using Domain.GeneralSubDomain;
using Domain.RentalSubDomain;
using EventStore.ClientAPI;
using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.PersistentSubscriptions;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using Utf8Json;

namespace RentalProjector
{
    public class Mylogger : EventStore.ClientAPI.ILogger
    {
        private Microsoft.Extensions.Logging.ILogger<MicroBatchFramework.BatchEngine> Logger { get; }
        public Mylogger(Microsoft.Extensions.Logging.ILogger<MicroBatchFramework.BatchEngine> _logger)
        {
            Logger = _logger;
        }

        /// <summary>
        /// Writes an error to the logger
        /// </summary>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Error(string format, params object[] args) => Logger.LogError(format, args);

        /// <summary>
        /// Writes an error to the logger
        /// </summary>
        /// <param name="ex">A thrown exception.</param>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Error(Exception ex, string format, params object[] args) => Logger.LogError(ex, format, args);

        /// <summary>
        /// Writes an information message to the logger
        /// </summary>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Info(string format, params object[] args) => Logger.LogInformation(format, args);

        /// <summary>
        /// Writes an information message to the logger
        /// </summary>
        /// <param name="ex">A thrown exception.</param>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Info(Exception ex, string format, params object[] args) => Logger.LogInformation(ex, format, args);

        /// <summary>
        /// Writes a debug message to the logger
        /// </summary>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Debug(string format, params object[] args) => Logger.LogDebug(format, args);

        /// <summary>
        /// Writes a debug message to the logger
        /// </summary>
        /// <param name="ex">A thrown exception.</param>
        /// <param name="format">Format string for the log message.</param>
        /// <param name="args">Arguments to be inserted into the format string.</param>
        public void Debug(Exception ex, string format, params object[] args) => Logger.LogDebug(ex, format, args);
    }
}