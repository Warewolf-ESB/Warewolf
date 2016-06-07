/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// Copyright (C) Josh Smith - September 2006

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Warewolf.Resource.Errors;

namespace WPF.JoshSmith.Controls.Validation
{
    /// <summary>
    ///     This static class provides attached properties which supply
    ///     validation of the text in a TextBox, using regular expressions.
    /// </summary>
    /// <remarks>
    ///     Documentation: http://www.codeproject.com/KB/WPF/RegexValidationInWPF.aspx
    /// </remarks>
    public static class RegexValidator
    {
        #region Static Constructor

        static RegexValidator()
        {
            RegexTextProperty = DependencyProperty.RegisterAttached(
                "RegexText",
                typeof (string),
                typeof (RegexValidator),
                new UIPropertyMetadata(null, OnAttachedPropertyChanged));

            ErrorMessageProperty = DependencyProperty.RegisterAttached(
                "ErrorMessage",
                typeof (string),
                typeof (RegexValidator),
                new UIPropertyMetadata(null, OnAttachedPropertyChanged));
        }

        #endregion // Static Constructor

        #region Attached Properties

        #region ErrorMessage

        /// <summary>
        ///     Identifies the RegexValidator's ErrorMessage attached property.
        ///     This field is read-only.
        /// </summary>
        public static readonly DependencyProperty ErrorMessageProperty;




        #endregion // ErrorMessage

        #region RegexText

        /// <summary>
        ///     Identifies the RegexValidator's RegexText attached property.
        ///     This field is read-only.
        /// </summary>
        public static readonly DependencyProperty RegexTextProperty;




        #endregion // RegexText

        #endregion // Attached Properties

        #region Event Handling Methods

        #region OnAttachedPropertyChanged

        /// <summary>
        ///     Invoked whenever an attached property of the
        ///     RegexValidator is modified for a TextBox.
        /// </summary>
        /// <param name="depObj">A TextBox.</param>
        /// <param name="e"></param>
        private static void OnAttachedPropertyChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            var textBox = depObj as TextBox;
            if (textBox == null)
                throw new InvalidOperationException(ErrorResource.RegexValidatorUsedWithTexyBoxs);

            VerifyRegexValidationRule(textBox);
        }

        #endregion // OnAttachedPropertyChanged

        #endregion // Event Handling Methods

        #region Private Helpers

        #region GetRegexValidationRuleForTextBox

        /// <summary>
        ///     Returns a RegexValidationRule to be used for validating the specified TextBox.
        ///     If the TextBox is not yet initialized, this method returns null.
        /// </summary>
        private static RegexValidationRule GetRegexValidationRuleForTextBox(TextBox textBox)
        {
            if (!textBox.IsInitialized)
            {
                // If the TextBox.Text property is bound, but the TextBox is not yet
                // initialized, the property's binding can be null.  In that situation,
                // hook its Initialized event and verify the validation rule again when 
                // that event fires.  At that point in time, the Text property's binding
                // will be non-null.
                EventHandler callback = null;
                callback = delegate
                {
                    textBox.Initialized -= callback;
                    VerifyRegexValidationRule(textBox);
                };
                textBox.Initialized += callback;
                return null;
            }

            // Get the binding expression associated with the TextBox's Text property.
            BindingExpression expression = textBox.GetBindingExpression(TextBox.TextProperty);
            if (expression == null)
                throw new InvalidOperationException(ErrorResource.TexBoxMustBeBoundForRegexValidation);

            // Get the binding which owns the binding expression.
            Binding binding = expression.ParentBinding;
            if (binding == null)
                throw new ApplicationException(ErrorResource.TextBoxTextBindingHasNoParent);

            // Look for an existing instance of the RegexValidationRule class in the
            // binding.  If there is more than one instance in the ValidationRules
            // then throw an exception because we don't know which one to modify.
            RegexValidationRule regexRule = null;
            foreach (ValidationRule rule in binding.ValidationRules)
            {
                if (rule is RegexValidationRule)
                {
                    if (regexRule == null)
                        regexRule = rule as RegexValidationRule;
                    else
                        throw new InvalidOperationException(ErrorResource.RegexValidationRuleShouldHaveOneRule);
                }
            }

            // If the TextBox.Text property's binding does not yet have an 
            // instance of RegexValidationRule in its ValidationRules,
            // add an instance now and return it.
            if (regexRule == null)
            {
                regexRule = new RegexValidationRule();
                binding.ValidationRules.Add(regexRule);
            }

            return regexRule;
        }

        #endregion // GetRegexValidationRuleForTextBox

        #region VerifyRegexValidationRule

        /// <summary>
        ///     Creates or modifies the RegexValidationRule in the TextBox's Text property binding
        ///     to use the current values of the attached properties exposed by this class.
        /// </summary>
        /// <param name="textBox">The TextBox being validated.</param>
        private static void VerifyRegexValidationRule(TextBox textBox)
        {
            RegexValidationRule regexRule = GetRegexValidationRuleForTextBox(textBox);
            if (regexRule != null)
            {
                regexRule.RegexText = textBox.GetValue(RegexTextProperty) as string;
                regexRule.ErrorMessage = textBox.GetValue(ErrorMessageProperty) as string;
            }
        }

        #endregion // VerifyRegexValidationRule

        #endregion // Private Helpers
    }
}