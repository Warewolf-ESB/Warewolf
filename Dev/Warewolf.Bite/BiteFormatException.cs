/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Xml;
using System.Xml.Linq;

namespace Warewolf.Bite
{
    /// <summary>
    /// A <c>.bite</c> resource the reader cannot turn into a graph. Always names the element and line, and the
    /// reader never returns a partial graph alongside it (WF removal T1.6).
    /// </summary>
    public sealed class BiteFormatException : Exception
    {
        public BiteFormatException(string message, XObject? at = null, Exception? inner = null)
            : base(Describe(message, at), inner)
        {
            if (at is IXmlLineInfo info && info.HasLineInfo())
            {
                Line = info.LineNumber;
                Column = info.LinePosition;
            }
        }

        public int? Line { get; }
        public int? Column { get; }

        static string Describe(string message, XObject? at)
        {
            if (at == null)
            {
                return message;
            }
            var where = at is XElement element ? $"<{element.Name.LocalName}>" : at is XAttribute attribute ? $"@{attribute.Name.LocalName}" : at.NodeType.ToString();
            return at is IXmlLineInfo info && info.HasLineInfo()
                ? $"{message} (at {where}, line {info.LineNumber}, column {info.LinePosition})"
                : $"{message} (at {where})";
        }
    }
}
