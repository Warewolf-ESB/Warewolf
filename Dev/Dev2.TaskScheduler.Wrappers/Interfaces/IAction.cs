using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;
using Action = Microsoft.Win32.TaskScheduler.Action;

namespace Dev2.TaskScheduler.Wrappers.Interfaces
{
    public interface IAction : IDisposable, IWrappedObject<Action>
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