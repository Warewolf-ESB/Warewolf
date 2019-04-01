#pragma warning disable
﻿using System;

namespace WarewolfCOMIPC.Client
{
    public interface IDev2IpcClient
    {
        /// <summary>
        /// Executes a call to a library.
        /// </summary>
        /// <param name="clsid"></param>
        /// <param name="function">Name of the function to call.</param>
        /// <param name="execute"></param>
        /// <param name="args">Array of args to pass to the function.</param>
        /// <returns>Result object returned by the library.</returns>
        /// <exception cref="Exception">This Method will rethrow all exceptions thrown by the wrapper.</exception>
        object Invoke(Guid clsid, string function, Execute execute, ParameterInfoTO[] args);
    }
}