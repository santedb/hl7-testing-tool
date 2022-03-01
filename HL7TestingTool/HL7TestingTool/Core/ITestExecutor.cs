using NHapi.Base.Model;
using System.Collections.Generic;
using HL7TestingTool.Core.Impl;

namespace HL7TestingTool.Core
{
    /// <summary>
    /// Represents a test executor.
    /// </summary>
    internal interface ITestExecutor
    {
        /// <summary>
        /// Executes a series of test steps.
        /// </summary>
        /// <returns>Returns a list of response messages.</returns>
        IEnumerable<IMessage> ExecuteTestSteps();
    }
}
