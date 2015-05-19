/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - September 2006

using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Controls;

// ReSharper disable CheckNamespace

namespace WPF.JoshSmith.Controls.Validation
{
    /// <summary>
    ///     A <see cref="System.Windows.Controls.ValidationRule" />-derived class which
    ///     supports the use of regular expressions for validation.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/RegexValidationInWPF.aspx
    /// </remarks>
    public class RegexValidationRule : ValidationRule
    {
        #region Data

        private RegexOptions _regexOptions = RegexOptions.None;

        #endregion // Data

        #region Constructors

        /// <summary>
        ///     Parameterless constructor.
        /// </summary>
        public RegexValidationRule()
        {
        }

        /// <summary>
        ///     Creates a RegexValidationRule with the specified regular expression.
        /// </summary>
        /// <param name="regexText">The regular expression used by the new instance.</param>
        public RegexValidationRule(string regexText)
        {
            RegexText = regexText;
        }

        /// <summary>
        ///     Creates a RegexValidationRule with the specified regular expression
        ///     and error message.
        /// </summary>
        /// <param name="regexText">The regular expression used by the new instance.</param>
        /// <param name="errorMessage">The error message used when validation fails.</param>
// ReSharper disable UnusedParameter.Local
        public RegexValidationRule(string regexText, string errorMessage)
// ReSharper restore UnusedParameter.Local
            : this(regexText)
        {
            RegexOptions = _regexOptions;
        }

        /// <summary>
        ///     Creates a RegexValidationRule with the specified regular expression,
        ///     error message, and RegexOptions.
        /// </summary>
        /// <param name="regexText">The regular expression used by the new instance.</param>
        /// <param name="errorMessage">The error message used when validation fails.</param>
        /// <param name="regexOptions">The RegexOptions used by the new instance.</param>
// ReSharper disable UnusedParameter.Local
        public RegexValidationRule(string regexText, string errorMessage, RegexOptions regexOptions)
// ReSharper restore UnusedParameter.Local
            : this(regexText)
        {
            RegexOptions = regexOptions;
        }

        #endregion // Constructors

        #region Properties

        /// <summary>
        ///     Gets/sets the error message to be used when validation fails.
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Gets/sets the RegexOptions to be used during validation.
        ///     This property's default value is 'None'.
        /// </summary>
        public RegexOptions RegexOptions
        {
            get { return _regexOptions; }
            set { _regexOptions = value; }
        }

        /// <summary>
        ///     Gets/sets the regular expression used during validation.
        /// </summary>
        public string RegexText { get; set; }

        #endregion // Properties

        #region Validate

        /// <summary>
        ///     Validates the 'value' argument using the regular expression and
        ///     RegexOptions associated with this object.
        /// </summary>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            ValidationResult result = ValidationResult.ValidResult;

            // If there is no regular expression to evaluate,
            // then the data is considered to be valid.
            if (!String.IsNullOrEmpty(RegexText))
            {
                // Cast the input value to a string (null becomes empty string).
                string text = value as string ?? String.Empty;

                // If the string does not match the regex, return a value
                // which indicates failure and provide an error message.
                if (!Regex.IsMatch(text, RegexText, RegexOptions))
                    result = new ValidationResult(false, ErrorMessage);
            }

            return result;
        }

        #endregion // Validate
    }
}