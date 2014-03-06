using System.Collections;

using System.Text;


namespace Infragistics.Controls.Charts.Messaging
{
    internal class MessageChannel
    {
        private Queue _sendQueue = new Queue();
    
        public void SendMessage(Message message)
        {
            if (MessageSent != null)
            {
                MessageSent(message);
            }
            else
            {
                _sendQueue.Enqueue(message);
            }
        }

        public void AttachTarget(MessageEventHandler m)
        {
            MessageSent += m;
            while (_sendQueue.Count > 0)
            {
                Message mess = (Message)_sendQueue.Dequeue();
                MessageSent(mess);
            }
        }

        private event MessageEventHandler MessageSent;

        public void DetachTarget(MessageEventHandler m)
        {
            MessageSent -= m;
        }

        private MessageChannel _connectedTo;

        public void ConnectTo(MessageChannel m)
        {
            _connectedTo = m;
            AttachTarget(SendToNext);
        }

        public void DetachFromNext()
        {
            if (_connectedTo == null)
            {
                return;
            }
            DetachTarget(SendToNext);
            _connectedTo = null;
        }

        public void SendToNext(Message m)
        {
            if (_connectedTo != null)
            {
                _connectedTo.SendMessage(m);
            }
        }

        public override string ToString()
        {

            StringBuilder builder = new StringBuilder();
            foreach (var message in _sendQueue)
            {
                builder.AppendLine(message.ToString());
            }
            return builder.ToString();



        }
    }

    internal delegate void MessageEventHandler(Message message);
}
#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved