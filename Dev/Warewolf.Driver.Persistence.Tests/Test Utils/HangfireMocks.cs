/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Warewolf.Driver.Drivers.HangfireScheduler.Test_Utils
{
    public class HangfireMocks
    {
    }

    class PerformContextMock
    {
        private readonly Lazy<PerformContext> _context;

        public PerformContextMock(string jobId, Dictionary<string, StringBuilder> values)
        {
            Connection = new Mock<IStorageConnection>();
            BackgroundJob = new BackgroundJobMock(jobId, values);
            CancellationToken = new Mock<IJobCancellationToken>();
            Storage= new Mock<JobStorage>();

            _context = new Lazy<PerformContext>(
                () => new PerformContext(Storage.Object, Connection.Object, BackgroundJob.Object, CancellationToken.Object));
        }

        public Mock<IStorageConnection> Connection { get; set; }
        public BackgroundJobMock BackgroundJob { get; set; }
        public Mock<IJobCancellationToken> CancellationToken { get; set; }
        public Mock<JobStorage> Storage { get; set; }

        public PerformContext Object => _context.Value;

        public static void SomeMethod()
        {
        }
    }

    class BackgroundJobMock
    {
        private readonly Lazy<BackgroundJob> _object;

        public BackgroundJobMock(string jobId, Dictionary<string, StringBuilder> values)
        {
            Id = jobId;
            Job = Job.FromExpression(() => ResumeWorkflow(values, null));
            CreatedAt = DateTime.UtcNow;

            _object = new Lazy<BackgroundJob>(
                () => new BackgroundJob(Id, Job, CreatedAt));
        }

        public string Id { get; set; }
        public Job Job { get; set; }
        public DateTime CreatedAt { get; set; }

        public BackgroundJob Object => _object.Value;

        public static void ResumeWorkflow(Dictionary<string, StringBuilder> values, PerformContext context)
        {
        }
    }
}
