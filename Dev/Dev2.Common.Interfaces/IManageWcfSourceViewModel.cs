using System;
using System.Windows.Input;

namespace Dev2.Common.Interfaces
{
    public interface IManageWcfSourceViewModel
    {
        string EndpointUrl { get; set; }

        /// <summary>
        /// Test if connection is successful
        /// </summary>
        ICommand TestCommand { get; set; }

        ICommand CancelTestCommand { get; set; }

        /// <summary>
        /// The message that will be set if the test is either successful or not
        /// </summary>
        string TestMessage { get; }
        /// <summary>
        /// Command for save/ok
        /// </summary>
        ICommand SaveCommand { get; set; }

        /// <summary>
        /// Header text that is used on the view
        /// </summary>
        string HeaderText { get; set; }

        /// <summary>
        /// Has test passed
        /// </summary>
        bool TestPassed { get; set; }

        /// <summary>
        /// has test failed
        /// </summary>
        bool TestFailed { get; set; }
        /// <summary>
        /// IsTesting
        /// </summary>
        bool Testing { get; }

        /// <summary>
        /// The name of the resource
        /// </summary>
        // ReSharper disable UnusedMemberInSuper.Global
        string ResourceName { get; set; }
        // ReSharper restore UnusedMemberInSuper.Global

    }

    public interface IWcfSourceModel
    {
        void TestConnection(IWcfServerSource resource);

        void Save(IWcfServerSource toSource);

        string ServerName { get; set; }
        IWcfServerSource FetchSource(Guid resourceID);
    }
}
