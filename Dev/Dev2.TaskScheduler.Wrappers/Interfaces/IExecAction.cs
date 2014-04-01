using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public interface IExecAction : IAction, IWrappedObject<ExecAction>
    {
        /// <summary>
        ///     Gets or sets the path to an executable file.
        /// </summary>
        [XmlElement("Command")]
        string Path { get; set; }

        /// <summary>
        ///     Gets or sets the arguments associated with the command-line operation.
        /// </summary>
        [DefaultValue("")]
        string Arguments { get; set; }

        /// <summary>
        ///     Gets or sets the directory that contains either the executable file or the files that are used by the executable file.
        /// </summary>
        [DefaultValue("")]
        string WorkingDirectory { get; set; }
    }
}