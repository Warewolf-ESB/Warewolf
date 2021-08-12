/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Warewolf.Driver.Resume;
using Warewolf.Execution;
using static Dev2.Common.Interfaces.WarewolfExecutionEnvironmentException;

namespace Warewolf.HangfireServer
{
    internal class WarewolfResumeWorkflow
    {
        private readonly PerformingContext _context;
        private readonly IResumptionFactory _resumptionFactory;
        private readonly IExecutionLogPublisher _logger;

        public WarewolfResumeWorkflow(IExecutionLogPublisher logger, PerformingContext context, IResumptionFactory resumptionFactory)
        {
            _logger = logger;
            _context = context;
            _resumptionFactory = resumptionFactory;
        }

        public void PerformResumption()
        {
            var backgroundJob = _context.BackgroundJob;
            var jobArg = backgroundJob.Job.Args[0];
            var backgroundJobId = backgroundJob.Id;
            try
            {
                var resumptionFactory = _resumptionFactory ?? new ResumptionFactory();
                var resumption = resumptionFactory.New(_logger);
                if (resumption.Connect())
                {
                    _logger.Info("Performing Resume of job {" + backgroundJobId + "}, connection established.", backgroundJobId);

                    var values = jobArg as Dictionary<string, StringBuilder>;
                    var result = resumption.Resume(values);
                    if (result.HasError)
                    {
                        //Can return exception message or details here as this might be an execution error based on user setup
                        throw new WarewolfResumeWorkflowException(result.Message?.ToString());
                    }
                }
                else
                {
                    throw new WarewolfServerConnectionException("could not establish a connection.");
                }
            }
            catch (Exception ex) when (ex is WarewolfException warewolf) 
            {
                var message = "Failed to perform job { " +backgroundJobId+ " }, "+ ex.Message;
                _logger.Error(message, backgroundJobId);
                throw new WarewolfException(message, warewolf.InnerException, warewolf.ExceptionType, warewolf.Severity);
            }
            catch (Exception ex)
            {
                //Note: this failure error or exception will mostly not be ideal to send to user for security, remove + ex.Message later
                var message = "Failed to perform job { " + backgroundJobId + " }, " + ex.Message;
                _logger.Error(message, backgroundJobId);
                throw new WarewolfException(message, null, ExceptionType.Execution, ExceptionSeverity.Critical);
            }
        }
    }
}
