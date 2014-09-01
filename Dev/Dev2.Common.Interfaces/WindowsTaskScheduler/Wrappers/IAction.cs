using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IAction : IDisposable, IWrappedObject<Microsoft.Win32.TaskScheduler.Action>
    {
        /// <summary>
        ///     Gets the type of the action.
        /// </summary>
        /// <value>The type of the action.</value>
        [XmlIgnore]
        TaskActionType ActionType { get; }

        /// <summary>
        ///     Gets or sets the identifier of the action.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute(AttributeName = "id", DataType = "ID")]
        string Id { get; set; }
    }
}