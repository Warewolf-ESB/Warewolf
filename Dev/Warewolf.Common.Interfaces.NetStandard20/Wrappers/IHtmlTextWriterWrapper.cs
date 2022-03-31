/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;

namespace Warewolf.Common.Interfaces.NetStandard20.Wrappers
{
    public static class WarewolfHtmlTextWriterStyle
    {
        public const string FontWeight = "Font-Weight";
        public const string FontFamily = "Font-Family";

        public const string Display = "Display";
        public const string Margin = "Margin";
        public const string Padding = "Padding";
        public const string FontSize = "Font-Size";
        public const string Color = "Color";
        public const string Width = "Width";
        public const string BackgroundColor = "Background-Color";
        public const string TextAlign = "Text-Align";
        public const string Height = "Height";
    }

    public enum WarewolfHtmlTextWriterAttribute
    {
        Class,
        Target,
        Href
    }

    public enum WarewolfHtmlTextWriterTag
    {
        Td,
        Table,
        Div,
        Tr,
        A,
        Br,
        Li
    }

    public interface IHtmlTextWriterWrapper : IDisposable
    {
        void AddStyleAttribute(string key, string value);
        void AddAttribute(string key, string value);
        void RenderBeginTag(string tagKey);
        void RenderEndTag();
        void Write(string text);
    }
}
