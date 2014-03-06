using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security;
using System.ComponentModel;
using System.Net;

namespace Infragistics.Controls.Schedules
{
	/// <summary>
	/// Represents a user which can login to the Microsoft Exchange Server. 
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_ExchangeConnector, Version = FeatureInfo.Version_11_1)]

	public sealed class ExchangeUser : INotifyPropertyChanged
	{
		#region Member Variables

		private string domain;



		private SecureString password;

		private string userName;

		#endregion  // Member Variables
		
		#region Constructor
      
		/// <summary>
		/// Initializes a new <see cref="ExchangeUser"/> instance.
		/// </summary>
		public ExchangeUser() { }

		/// <summary>
		/// Initializes a new <see cref="ExchangeUser"/> instance.
		/// </summary>
		/// <param name="userName">The user name of the user.</param>
		/// <param name="password">The password of the user.</param>
		public ExchangeUser(string userName, string password)
			: this(userName, password, null) { }


		/// <summary>
		/// Initializes a new <see cref="ExchangeUser"/> instance.
		/// </summary>
		/// <param name="userName">The user name of the user.</param>
		/// <param name="password">The password of the user.</param>
		public ExchangeUser(string userName, SecureString password)
			: this(userName, password, null) { }


		/// <summary>
		/// Initializes a new <see cref="ExchangeUser"/> instance.
		/// </summary>
		/// <param name="userName">The user name of the user.</param>
		/// <param name="password">The password of the user.</param>
		/// <param name="domain">The domain or computer to which the user belongs.</param>
		public ExchangeUser(string userName, string password, string domain)
		{
			this.userName = userName;




			if (password != null)
			{
				this.password = new SecureString();

				foreach (char c in password)
					this.password.AppendChar(c);
			}


			this.domain = domain;
		}


		/// <summary>
		/// Initializes a new <see cref="ExchangeUser"/> instance.
		/// </summary>
		/// <param name="userName">The user name of the user.</param>
		/// <param name="password">The password of the user.</param>
		/// <param name="domain">The domain or computer to which the user belongs.</param>
		public ExchangeUser(string userName, SecureString password, string domain)
		{
			this.userName = userName;
			this.password = password;
			this.domain = domain;
		}

    
	#endregion  // Constructor

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Occurs when a property value is changed on the <see cref="ExchangeUser"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion

		#region Methods

		#region CreateNetworkCredentials

		internal NetworkCredential CreateNetworkCredentials()
		{
			NetworkCredential credentials = new NetworkCredential();
			credentials.Domain = this.Domain;
			credentials.UserName = this.UserName;




			credentials.SecurePassword = this.Password;

			return credentials;
		}

		#endregion // CreateNetworkCredentials

		#region OnPropertyChanged

		private void OnPropertyChanged(string propertyName)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion // OnPropertyChanged 

		#endregion // Methods

		#region Properties

		#region Domain

		/// <summary>
		/// Gets or sets the domain or computer to which the user belongs.
		/// </summary>
		public string Domain
		{
			get
			{
				return this.domain;
			}
			set
			{
				if (this.domain == value)
					return;

				this.domain = value;
				this.OnPropertyChanged("Domain");
			}
		}

		#endregion  // Domain

		#region Password

		/// <summary>
		/// Gets or sets the password of the user.
		/// </summary>
		public



			SecureString

				Password
		{
			get
			{
				return this.password;
			}
			set
			{
				if (this.password == value)
					return;

				this.password = value;
				this.OnPropertyChanged("Password");
			}
		}

		#endregion  // Password

		#region UserName

		/// <summary>
		/// Gets or sets the user name of the user.
		/// </summary>
		public string UserName
		{
			get
			{
				return this.userName;
			}
			set
			{
				if (this.userName == value)
					return;

				this.userName = value;
				this.OnPropertyChanged("UserName");
			}
		}

		#endregion  // UserName 

		#endregion  // Properties
	}
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