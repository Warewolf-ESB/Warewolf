/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Hangfire.Server;
using System;
using System.Collections.Generic;
using System.Text;
using Warewolf.Driver.Resume;
using Warewolf.Execution;

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
            var jobArg = _context.BackgroundJob.Job.Args[0];
            var backgroundJobId = _context.BackgroundJob.Id;
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
                        throw new InvalidOperationException(result.Message?.ToString(), new Exception(result.Message?.ToString()));
                    }
                }
                else
                {
                    _logger.Error("Failed to perform job {" + backgroundJobId + "}, could not establish a connection.", backgroundJobId);
                    throw new InvalidOperationException("Failed to perform job " + backgroundJobId + " could not establish a connection.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to perform job {" + backgroundJobId + "}" + ex.InnerException);
                throw;
            }
        }
    }
}
