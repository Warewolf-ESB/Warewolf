/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Xaml;

namespace Dev2.Common
{
    public class ViewStateCleaningWriter : XamlWriter
    {
        public ViewStateCleaningWriter(XamlWriter innerWriter)
        {
            this.InnerWriter = innerWriter;
            this.MemberStack = new Stack<XamlMember>();
        }

        XamlWriter InnerWriter { get; set; }
        Stack<XamlMember> MemberStack { get; set; }

        protected override void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                if (InnerWriter != null)
                {
                    ((IDisposable)InnerWriter).Dispose();
                    InnerWriter = null;
                }

                MemberStack.Clear();
            }

            base.Dispose(disposing);
        }

        public override XamlSchemaContext SchemaContext
        {
            get
            {
                return InnerWriter.SchemaContext;
            }
        }

        public override void WriteEndMember()
        {
            var xamlMember = MemberStack.Pop();
            if (m_attachedPropertyDepth > 0)
            {
                if (IsDesignerAttachedProperty(xamlMember))
                {
                    m_attachedPropertyDepth--;
                }
                return;
            }

            InnerWriter.WriteEndMember();
        }

        public override void WriteEndObject()
        {
            if (m_attachedPropertyDepth > 0)
            {
                return;
            }

            InnerWriter.WriteEndObject();
        }

        public override void WriteGetObject()
        {
            if (m_attachedPropertyDepth > 0)
            {
                return;
            }

            InnerWriter.WriteGetObject();
        }

        public override void WriteNamespace(NamespaceDeclaration namespaceDeclaration)
        {
            if (m_attachedPropertyDepth > 0)
            {
                return;
            }

            InnerWriter.WriteNamespace(namespaceDeclaration);
        }

        public override void WriteStartMember(XamlMember xamlMember)
        {
            MemberStack.Push(xamlMember);
            if (IsDesignerAttachedProperty(xamlMember))
            {
                m_attachedPropertyDepth++;
            }

            if (m_attachedPropertyDepth > 0)
            {
                return;
            }

            InnerWriter.WriteStartMember(xamlMember);
        }

        public override void WriteStartObject(XamlType type)
        {
            if (m_attachedPropertyDepth > 0)
            {
                return;
            }

            InnerWriter.WriteStartObject(type);
        }

        public override void WriteValue(Object value)
        {
            if (m_attachedPropertyDepth > 0)
            {
                return;
            }

            InnerWriter.WriteValue(value);
        }

        static Boolean IsDesignerAttachedProperty(XamlMember xamlMember)
        {
            return xamlMember.IsAttachable &&
            (xamlMember.PreferredXamlNamespace.Equals(c_sapNamespaceURI, StringComparison.OrdinalIgnoreCase) ||
            xamlMember.PreferredXamlNamespace.Equals(c_sap2010NamespaceURI, StringComparison.OrdinalIgnoreCase));
        }

        private Int32 m_attachedPropertyDepth;
        const String c_sapNamespaceURI = "http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation";
        const String c_sap2010NamespaceURI = "http://schemas.microsoft.com/netfx/2010/xaml/activities/presentation";
    }
}