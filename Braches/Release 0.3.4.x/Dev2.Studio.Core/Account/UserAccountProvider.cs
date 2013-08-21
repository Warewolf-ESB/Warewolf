using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Account
{
    // Sashen : 24-01-2013 : This class is used to facilitate User Account information supplied to the server for authentication
    public class UserAccountProvider : IUserAccountProvider
    {
        #region Properties

        public string UserName
        {
            get
            {
                if (string.IsNullOrEmpty(_userName))
                {
                    _userName = Guid.NewGuid().ToString();
                }
                return _userName;
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
        }

        #endregion Properties

        #region Members

        private string _userName;
        private string _password;

        // Sashen: 24-01:2013 - This is only hardcoded because the server allows for it,
        // please change when we introduce a security module on the server.

        private const string _hardPassword = "asd";

        #endregion Members

        #region CTOR


        // Sashen : 24-01-2013 : The default constructor will generate a new GUID for a UserName and a hardcoded password
        public UserAccountProvider()
        {
            _userName = Guid.NewGuid().ToString();
            _password = _hardPassword;
        }

        // Sashen: 24-01-2013 : The constructor takes the user Name and Password and merely sets these values that can be retrieved from the
        // Properties of the class
        public UserAccountProvider(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
                throw new ArgumentNullException("User Name or password must not be null");
            _userName = userName;
            _password = password;
        }

        #endregion CTOR
    }
}
