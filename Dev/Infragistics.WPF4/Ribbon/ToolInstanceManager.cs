using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using Infragistics.Windows.Ribbon.Internal;
using Infragistics.Windows.Helpers;
using Infragistics.Shared;

namespace Infragistics.Windows.Ribbon
{
	internal class ToolInstanceManager
	{
		#region Member Variables

		private Dictionary<string, IdInfo>					_idInfos = new Dictionary<string, IdInfo>(50);
		private XamRibbon									_ribbon = null;
		private static Random								_randomizer = new Random();
		private ObservableCollection<FrameworkElement>		_allToolInstances = new ObservableCollection<FrameworkElement>();

		#endregion //Member Variables

		#region Constructor

		internal ToolInstanceManager(XamRibbon xamRibbon)
		{
			Debug.Assert(xamRibbon != null, "XamRibbon is null!");
			this._ribbon = xamRibbon;
		}

		#endregion //Constructor

		#region Properties

			#region Internal Properties

				#region AllToolInstances

		internal ObservableCollection<FrameworkElement> AllToolInstances
		{
			get { return this._allToolInstances; }
		}

				#endregion //AllToolInstances	
    
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region ChangeToolInstanceRegistrationId



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal bool ChangeToolInstanceRegistrationId(FrameworkElement tool, string oldId)
		{
			string newId = RibbonToolHelper.GetId(tool);
			Debug.Assert(newId != oldId, "Old and New tool ID equal - registration not changing!");
			if (newId == oldId)
				return true;


			// Don't allow a tool with the same Id to be registered unless it is on the QAT.  We may remove this restriction in a future release.
			bool allowRegistration = this.CanRegisterTool(tool, newId);
			if (allowRegistration == false)
				throw new NotSupportedException(XamRibbon.GetString("LE_ToolRegisteredWithSameId", newId));


			Debug.Assert(this._idInfos.ContainsKey(oldId) == true, "Old tool id never registered!");
			if (this._idInfos.ContainsKey(oldId) == true)
				this._idInfos[oldId].RemoveTool(tool);


			return this.RegisterToolInstance(tool);
		}

				#endregion //ChangeToolInstanceRegistrationId	

				#region GetAllInstancesWithId



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal IEnumerator GetAllInstancesWithId(string toolId)
		{
			if (this._idInfos.ContainsKey(toolId))
				return this._idInfos[toolId].ToolInstances.GetEnumerator();

			return null;
		}

				#endregion //GetAllInstancesWithId	
    
				#region GetNewId (static)

		internal static string GetNewId()
		{
			return "(ID)" + DateTime.Now.Ticks.ToString() + _randomizer.Next().ToString();
		}

				#endregion //GetNewId (static)	

				#region GetToolInstanceFromToolId






			internal FrameworkElement GetToolInstanceFromToolId(string toolId)
			{
				if (this._idInfos.ContainsKey(toolId) && this._idInfos[toolId].ToolInstances.Count > 0)
					return this._idInfos[toolId].ToolInstances[0] as FrameworkElement;

				return null;
			}

				#endregion //GetToolInstanceFromToolId

				#region IsRegistered

		internal bool IsRegistered(FrameworkElement tool)
		{
			string toolId = RibbonToolHelper.GetId(tool);

			return IsRegistered(tool, toolId);
		}

		internal bool IsRegistered(FrameworkElement tool, string toolId)
		{
			if (string.IsNullOrEmpty(toolId))
				return false;

			if (this._idInfos.ContainsKey(toolId) == false)
				return false;

			return this._idInfos[toolId].ContainsTool(tool);
		}

				#endregion //IsRegistered	
    
				// AS 11/12/07 BR28406
				#region OutputInstanceTree
		[Conditional("DEBUG")]
		internal void OutputInstanceTree()
		{
			Debug.WriteLine("Instance Tree:");
			Debug.WriteLine(new string('=', 30));
			Debug.WriteLine(string.Empty);

			foreach (KeyValuePair<string, IdInfo> item in this._idInfos)
			{
				if (item.Value.ToolInstances.Count == 0)
					continue;

				Debug.WriteLine("Item Key = " + item.Key);

				Debug.Assert(item.Value.ToolInstances.Count > 0, "There are no instances of this tool remaining!");

				Debug.WriteLine("  Type = " + item.Value.ToolInstances[0].GetType());
				Debug.WriteLine("  Caption = " + RibbonToolHelper.GetCaption(item.Value.ToolInstances[0]));

				foreach (FrameworkElement instance in item.Value.ToolInstances)
				{
					ToolLocation location = XamRibbon.GetLocation(instance);
					Debug.WriteLine("  Location = " + location.ToString());

					Debug.Assert(XamRibbon.GetRibbon(instance) != null && XamRibbon.GetRibbon(instance) == Utilities.GetAncestorFromType(instance, typeof(XamRibbon), true), "This tool is not within the ribbon!");
				}

				Debug.WriteLine(string.Empty);
			}
		} 
				#endregion //OutputInstanceTree

				#region RegisterToolInstance

		internal bool RegisterToolInstance(FrameworkElement tool)
		{
			if (tool == null)
				throw new ArgumentNullException("tool");


			string toolId = RibbonToolHelper.GetId(tool);
			if (toolId == null || toolId == string.Empty)
			{
				toolId = ToolInstanceManager.GetNewId();
				tool.SetValue(RibbonToolHelper.IdProperty, toolId);
			}


			if (this._idInfos.ContainsKey(toolId))
			{
				// Don't allow a tool with the same Id to be registered unless it is on the QAT.  We may remove this restriction in a future release.
				bool allowRegistration = this.CanRegisterTool(tool, toolId);
				if (allowRegistration == false)
					throw new NotSupportedException(XamRibbon.GetString("LE_ToolRegisteredWithSameId",toolId));

				// AS 10/11/07
				Debug.Assert(XamRibbon.GetClonedFromTool(tool) == null || RibbonToolProxy.GetRootSourceTool(tool) == XamRibbon.GetClonedFromTool(tool), "A tool being added is not a clone of the root tool but is a clone of a cloned tool.");

				// AS 10/11/07
				//bool toolAdded = this._idInfos[toolId].AddTool(tool);
				IdInfo info = this._idInfos[toolId];

				Debug.Assert(info.ToolInstances.Count == 0 || XamRibbon.GetClonedFromTool(tool) != null, "We already have a registered tool for this id which must be a source tool but it seems that we're adding another source tool because the tool being registered is not marked as cloned from another!");

				bool toolAdded = info.AddTool(tool);

				if (toolAdded)
				{
					// AS 10/11/07
					// When a tool is cloned and there is a tool already on the qat, we need to 
					// update the IsOnQat of the new tool being registered.
					//
					if (info.ToolInstances.Count > 0 && RibbonToolHelper.GetIsOnQat(info.ToolInstances[0]))
						tool.SetValue(RibbonToolHelper.IsOnQatPropertyKey, KnownBoxes.TrueBox);

					this._allToolInstances.Add(tool);
				}

				return true;
			}
			else
			{
				IdInfo	idi			= new IdInfo(toolId);
				bool	toolAdded	= idi.AddTool(tool);

				if (toolAdded)
					this._allToolInstances.Add(tool);

				this._idInfos.Add(toolId, idi);
				return true;
			}
		}

				#endregion //RegisterToolInstance	

				#region SetPropertyValueOnAllRelatedToolInstances

		internal void SetPropertyValueOnAllRelatedToolInstances(string toolId, DependencyProperty dp, object value)
		{
			Debug.Assert(toolId != null && toolId != string.Empty && dp != null, "Null parameters in 'SetPropertyValueOnAllRelatedToolInstances'!");
			if (toolId	== null			||
				toolId	== string.Empty ||
				dp		== null)
				return;

			this.SetPropertyValueOnAllRelatedToolInstances(toolId, dp, null, value);
		}

		internal void SetPropertyValueOnAllRelatedToolInstances(string toolId, DependencyPropertyKey dpKey, object value)
		{
			Debug.Assert(toolId != null && toolId != string.Empty && dpKey != null, "Null parameters in 'SetPropertyValueOnAllRelatedToolInstances'!");
			if (toolId	== null			||
				toolId	== string.Empty ||
				dpKey	== null)
				return;

			this.SetPropertyValueOnAllRelatedToolInstances(toolId, null, dpKey, value);
		}

		private void SetPropertyValueOnAllRelatedToolInstances(string toolId, DependencyProperty dp, DependencyPropertyKey dpKey, object value)
		{
			if (this._idInfos.ContainsKey(toolId) == false)
				return;

			List<FrameworkElement> toolInstances = this._idInfos[toolId].ToolInstances;
			int	count = toolInstances.Count;

			for (int i = 0; i < count; i++)
			{
				DependencyObject d = toolInstances[i] as DependencyObject;
				if (d != null)
				{
					if (dp != null)
						d.SetValue(dp, value);
					else
					if (dpKey != null)
						d.SetValue(dpKey, value);
				}
			}
		}

				#endregion //SetPropertyValueOnAllRelatedToolInstances	
    
				#region UnRegisterToolInstance

		internal bool UnRegisterToolInstance(FrameworkElement tool)
		{
			if (tool == null)
				throw new ArgumentNullException("tool");


			string toolId = RibbonToolHelper.GetId(tool);
			if (toolId == null || toolId == string.Empty)
				// AS 10/25/07 BR27712
				// If a tool wasn't registered, we shouldn't blow up since the unregister is not
				// something called explicitly by the programmer but something that we handle.
				//
				//throw new ArgumentException("Tool.ID is null!");
				return false;


			Debug.Assert(this._idInfos.ContainsKey(toolId), "Can't UnRegister tool - ID not found!");
			if (this._idInfos.ContainsKey(toolId) == false)
				return false;


			bool toolRemoved = this._idInfos[toolId].RemoveTool(tool);
			if (toolRemoved)
			{
				// AS 10/12/07
				// Moved to here from the UnregisterTool method on XamRibbon so we are
				// sure to only fire this once for a cloned tool.
				//
				// 
				// if this is a cloned tool then fire the clonediscarded and 
				// unhook any events that were hooked on it
				FrameworkElement sourceTool = XamRibbon.GetClonedFromTool(tool);

				if (sourceTool != null)
					RibbonToolProxy.ReleaseToolClone(tool);

				Debug.Assert(this._allToolInstances.Contains(tool), "'allTools' list does not contain tool being un-registered!");

				// AS 10/11/07 Optimization
				//if (this._allToolInstances.Contains(tool))
				//	this._allToolInstances.Remove(tool);
				int index = this._allToolInstances.IndexOf(tool);

				if (index >= 0)
					this._allToolInstances.RemoveAt(index);
			}

			return toolRemoved;
		}

				#endregion //UnRegisterToolInstance	
        
			#endregion //Internal Methods

			#region Private Methods

				#region CanRegisterTool

		private bool CanRegisterTool(FrameworkElement tool, string toolId)
		{
			IdInfo info;

			// if a tool has not been registered for this id then its ok
			if (this._idInfos.TryGetValue(toolId, out info) == false)
				return true;

			List<FrameworkElement> tools = info.ToolInstances;
			bool hasQatInstance = false;
			bool hasQatRibbonGroupInstance = false;
			int otherLocationCount = 0;

			for (int i = 0, count = tools.Count; i < count; i++)
			{
				ToolLocation location = XamRibbon.GetLocation(tools[i]);

				if (location == ToolLocation.QuickAccessToolbar)
					hasQatInstance = true;
				else if (location == ToolLocation.Ribbon)
				{
					// the tool is on a ribbon group but is that ribbon group on the qat
					RibbonGroup group = tools[i].Parent as RibbonGroup;

					if (null != group && XamRibbon.GetLocation(group) == ToolLocation.QuickAccessToolbar)
						hasQatRibbonGroupInstance = true;
					else
						otherLocationCount++;
				}
				else
					otherLocationCount++;
			}

			ToolLocation toolLocation = XamRibbon.GetLocation(tool);

			// if this tool is on the qat then its only valid if its not already on the qat
			if (toolLocation == ToolLocation.QuickAccessToolbar)
				return hasQatInstance == false;

			// if the new tool is on a ribbon group then make sure its 
			if (toolLocation == ToolLocation.Ribbon)
			{
				// the tool is on a ribbon group but is that ribbon group on the qat
				RibbonGroup group = (RibbonGroup)Utilities.GetAncestorFromType(tool, typeof(RibbonGroup), true);

				if (null != group && XamRibbon.GetLocation(group) == ToolLocation.QuickAccessToolbar)
					return hasQatRibbonGroupInstance == false;
			}

			return otherLocationCount == 0;
		}

				#endregion //CanRegisterTool	
    
			#endregion //Private Methods

		#endregion Methods

		#region Private Class IdInfo

		private class IdInfo
		{
			#region Member Variables

			private string										_id = string.Empty;
			private List<FrameworkElement>						_toolInstances = new List<FrameworkElement>(5);

			#endregion //Member Variables

			#region Constructor

			internal IdInfo(string id)
			{
				if (id == null || id == string.Empty)
					throw new ArgumentNullException("id");

				this._id = id;
			}

			#endregion Constructor

			#region Properties

				#region ToolInstances
    
    		internal List<FrameworkElement> ToolInstances
			{
				get { return this._toolInstances; }
			}

   				#endregion //ToolInstances	
    
			#endregion //Properties

			#region Methods

				#region AddTool

			internal bool AddTool(FrameworkElement tool)
			{
				if (tool == null)
					throw new ArgumentNullException("tool");

				string toolId = RibbonToolHelper.GetId(tool);
				if (this._id != toolId)
					throw new InvalidOperationException("Wrong ID!");


				if (this._toolInstances.Count > 0 && tool.GetType() != this._toolInstances[0].GetType())
					throw new InvalidOperationException(XamRibbon.GetString("LE_MismatchedToolTypeForId"));


				if (this.ContainsTool(tool) == false)
				{
					int totalTools = this._toolInstances.Count;
					if (totalTools > 0)
					{
						RibbonToolProxy.BindTool(this._toolInstances[totalTools - 1] as FrameworkElement, tool);
					}

					this._toolInstances.Add(tool);
					tool.SetValue(XamRibbon.IsRegisteredProperty, true);

					return true;
				}
				else
				{
					tool.SetValue(XamRibbon.IsRegisteredProperty, true);
					return false;
				}
			} 

				#endregion //AddTool

				#region ContainsTool

			internal bool ContainsTool(FrameworkElement tool)
			{
				return this._toolInstances.Contains(tool);
			}

				#endregion //ContainsTool	

				#region RemoveTool

			internal bool RemoveTool(FrameworkElement tool)
			{
				if (tool == null)
					throw new ArgumentNullException("tool");

				// AS 8/21/07
				// When we are changing the id then this does need to change.
				//
				//string toolId = RibbonToolHelper.GetId(tool);
				//if (this._id != toolId)
				//	throw new ArgumentException("Wrong ID!");

				
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

				int index = this._toolInstances.IndexOf(tool);

				if (index < 0)
					return false;

				this._toolInstances.RemoveAt(index);
				tool.SetValue(XamRibbon.IsRegisteredProperty, false);

				#region Only QAT instance remaining
				// AS 10/11/07 BR27304
				// If there is only 1 tool left and it is the qat instance then remove it.
				//
				if (this._toolInstances.Count == 1 && XamRibbon.GetLocation(this._toolInstances[0]) == ToolLocation.QuickAccessToolbar)
				{
					XamRibbon ribbon = XamRibbon.GetRibbon(this._toolInstances[0]);
					Debug.Assert(null != ribbon);

					if (null != ribbon)
						ribbon.QuickAccessToolbar.RemoveTool(this._toolInstances[0]);
				} 
				#endregion //Only QAT instance remaining

				if (this._toolInstances.Count > 0)
				{
					// we need to fix up the bindings but also update the ClonedFromTool references

					// first find a tool to be the new "cloned" from value and update any tools cloned 
					// from the tool that was removed. if the tool being removed had a source tool 
					// then use that - i.e. promote all the tools currently marked as cloned by the 
					// tool being removed
					FrameworkElement newClonedFromTool = XamRibbon.GetClonedFromTool(tool);

					// since all tools are cloned from a single "root" source tool, we only need to 
					// fix up bindings if the tool being removed is the source tool. if we feel
					// this is not the case then we can move the loop at the end of this if block
					// to outside the if block.
					//
					if (newClonedFromTool == null && this._toolInstances.Count > 1)
					{
						// While we have some code in here to deal with this for future expansion, there is a potential 
						// problem since we will be promoting a tool that was bound to the 'source' tool to be a source 
						// tool but it will still be bound. This could cause a problem for say a menu tool since only 
						// the "source" menu tool actually has the tool instances in its items collection and when its
						// removed, they will not be linked to the ribbon anymore so we might have to handle them specially
						// and keep them in the logical children of the ribbon until all the instances are removed
						//
						Debug.Assert(newClonedFromTool == null, "Check out the comments and make sure we're handling this properly. If so, remove the assert.");

						// otherwise, find a tool that used the removed tool as the source and
						// use that as the new source tool
						#region Extra Processing to find source if this was the root source
						for (int i = 0, count = this._toolInstances.Count; i < count; i++)
						{
							FrameworkElement remainingTool = this._toolInstances[i];

							// if this tool is using the other as its source then we can use this as the new source
							if (XamRibbon.GetClonedFromTool(remainingTool) == tool)
							{
								newClonedFromTool = remainingTool;
								break;
							}
						}
						#endregion //Extra Processing to find source if this was the root source

						// this tool is now the source tool for all others so clear its cloned from tool
						if (newClonedFromTool != null)
						{
							
							XamRibbon.SetClonedFromTool(newClonedFromTool, null);
						}

						
						
						
						// update the ClonedFromTool of the tools that referenced the removed tool
						for (int i = 0, count = this._toolInstances.Count; i < count; i++)
						{
							FrameworkElement remainingTool = this._toolInstances[i];

							// if a tool used the removed tool as its source then update it to 
							// use the new "source" tool
							if (XamRibbon.GetClonedFromTool(remainingTool) == tool)
							{
								RibbonToolProxy.BindTool(remainingTool, newClonedFromTool);
								XamRibbon.SetClonedFromTool(remainingTool, newClonedFromTool);
							}
						}
					}
				}

				return true;
			}

				#endregion //RemoveTool

			#endregion //Methods
		}

		#endregion //Private Class IdInfo
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