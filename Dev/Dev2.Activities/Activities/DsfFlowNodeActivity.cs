/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using Dev2;
using Dev2.Activities;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Microsoft.CSharp.Activities;
using Warewolf.Storage.Interfaces;


namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public abstract class DsfFlowNodeActivity<TResult> : DsfActivityAbstract<TResult>, IFlowNodeActivity, IEquatable<DsfFlowNodeActivity<TResult>>
    {
        readonly CSharpValue<TResult> _expression;
        TResult _theResult;

        #region Ctor

        protected DsfFlowNodeActivity(string displayName)
            : this(displayName, DebugDispatcher.Instance, true)
        {
        }

        // BUG 9304 - 2013.05.08 - TWR - Added this constructor for testing purposes
        protected DsfFlowNodeActivity(string displayName, IDebugDispatcher debugDispatcher, bool isAsync = false)
            : base(displayName, debugDispatcher, isAsync)
        {
            _expression = new CSharpValue<TResult>();
        }

        #endregion

        #region ExpressionText

        public string ExpressionText
        {
            get
            {
                return _expression.ExpressionText;
            }
            set
            {
                _expression.ExpressionText = value;
            }
        }
        #endregion

        #region CacheMetadata

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            //
            // Must use AddChild (which adds children as 'public') otherwise you will get the following exception:
            //
            // The private implementation of activity Decision has the following validation error:
            // Compiler error(s) encountered processing expression t.Eq(d.Get("FirstName",AmbientDataList),"Trevor").
            // 't' is not declared. It may be inaccessible due to its protection level
            // 'd' is not declared. It may be inaccessible due to its protection level
            //
            metadata.AddChild(_expression);
        }

        #endregion

        #region OnExecute

        protected override void OnExecute(NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region OnFaulted

        void OnFaulted(NativeActivityFaultContext faultContext, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Get Debug Inputs/Outputs
        
        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            return _debugInputs;
        }
        
        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {
            return _debugOutputs;
        }

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(ExpressionText);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(_theResult.ToString());
        }

        #endregion

        #region Overrides of DsfNativeActivity<TResult>
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((DsfFlowNodeActivity<TResult>) obj);
        }

        #region Overrides of Object
        
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (_expression != null ? _expression.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ EqualityComparer<TResult>.Default.GetHashCode(_theResult);
                hashCode = (hashCode * 397) ^ _dataListId.GetHashCode();
                hashCode = (hashCode * 397) ^ (_dataObject != null ? _dataObject.GetHashCode() : 0);
                return hashCode;
            }
        }

        #endregion

        #endregion

        public bool Equals(DsfFlowNodeActivity<TResult> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) 
                && Equals(ExpressionText, other.ExpressionText) 
                && EqualityComparer<TResult>.Default.Equals(_theResult, other._theResult) 
                && _dataListId.Equals(other._dataListId) 
                && Equals(_dataObject, other._dataObject);
        }
    }
}
