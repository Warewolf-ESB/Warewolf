using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Windows;
using System.Globalization;
using System.ComponentModel;
using System.Windows.Controls;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.DockManager.Events;
using Infragistics.Shared;
using System.Collections;
using System.Windows.Media;
using Infragistics.Collections;
using Infragistics.Windows.Controls;

namespace Infragistics.Windows.DockManager
{
	/// <summary>
	/// Helper class for managing loading and saving a layout file.
	/// </summary>
	internal static class LayoutManager
	{
		#region Constants

		// tags
		private const string RootTag = "xamDockManager";
		private const string ContentPanesTag = "contentPanes";
		private const string ContentPaneTag = "contentPane";
		private const string PanesTag = "panes";
		private const string DocumentsTag = "documents";
		private const string SplitPaneTag = "splitPane";
		private const string TabGroupPaneTag = "tabGroup";

		// attribs
		private const string VersionAttrib = "version";
		private const string UnpinnedOrderAttrib = "unpinnedOrder";
		private const string IsClosedAttrib = "isClosed";
		private const string NameAttrib = "name";
		private const string LocationAttrib = "location";
		private const string LastDockableStateAttrib = "lastDockableState";
		private const string SerializationIdAttrib = "serializationId";
		private const string SplitterOrientationAttrib = "splitterOrientation";
		private const string ExtentAttrib = "extent";
		private const string FloatingLocationAttrib = "floatingLocation";
		private const string FloatingSizeAttrib = "floatingSize";
		private const string RelativeSizeAttrib = "relativeSize";
		private const string SelectedIndexAttrib = "selectedIndex";
		private const string LastActivatedTimeAttrib = "lastActivatedTime";
		private const string LastFloatingSizeAttrib = "lastFloatingSize";
		private const string LastFloatingWindowRectAttrib = "lastFloatingWindowRect";
		private const string LastFloatingLocationAttrib = "lastFloatingLocation";
		private const string FlyoutExtentAttrib = "flyoutExtent";

		// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
		private const string FloatingWindowStateAttrib = "windowState";
		private const string FloatingRestoreBoundsAttrib = "restoreBounds";
		private const string FloatingRestoreToMaximizedAttrib = "restoreToMaximized";

		// AS 5/13/08
		// Save out the visibility state itself instead of just whether it was closed/collapsed.
		// While we don't specifically set the visibility to hidden, it is possible for the 
		// developer to do that in which case we will step on that state when we load the state.
		//
		private const string VisibilityAttrib = "visibility";

		#endregion //Constants

		#region Member Variables

		// AS 5/17/08 Reuse Group/Split
		// Optimization - just get this value once and store it rather then get it every time
		// we load a split pane in a loadlayout.
		//
		private static readonly Orientation DefaultSplitterOrientation = (Orientation)SplitPane.SplitterOrientationProperty.DefaultMetadata.DefaultValue;

		#endregion // Member Variables

		#region Constructor
		static LayoutManager()
		{
		} 
		#endregion //Constructor

		#region Public Methods

		#region LoadLayout

		public static void LoadLayout(XamDockManager dockManager, String layout)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				StreamWriter sw = new StreamWriter(ms);
				sw.Write(layout);
				sw.Flush();
				ms.Position = 0;

				LoadLayout(dockManager, ms);
			}
		}

		public static void LoadLayout(XamDockManager dockManager, Stream stream)
		{
			if (dockManager.IsLoadingLayout)
				throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutInProgress"));

			// AS 10/14/10 TFS57352
			dockManager.OnBeforeLoadLayoutProcess();

			dockManager.IsLoadingLayout = true;

			// AS 5/17/08 Reuse Group/Split
			// Because we may end up reusing panes now, we cannot suspend the layout. If we do
			// then we encounter a problem when we try to change the floating location, etc.
			// because the pane may be out of the tree because we haven't allowed updates
			// to occur. This wasn't really an issue before because the panes we were processing
			// were not in the tree yet.
			//
			//bool suspendedRootPaneUpdates = false;

			// AS 5/14/08 BR32587
			bool hadActivePane = dockManager.ActivePane != null;

            // AS 10/15/08 TFS8068
            GroupTempValueReplacement replacements = new GroupTempValueReplacement();

            try
			{
				// note, there seems to be a bug in the xmltextreader where
				// an exception regarding not finding the root element
				// is thrown because the data is not loaded. i came across
				// this when loading in info from a filestream. loading from 
				// a manifest resource stream worked fine though - presumably 
				// because the bits were already loaded. to get around this, 
				// we need to use a bufferedstream but since we don't know the 
				// source of the stream, we will always use it
				using (BufferedStream bufferedStream = new BufferedStream(stream))
				{
					XmlDocument document = new XmlDocument();
					document.Load(bufferedStream);

					#region Prepare

					XmlNode rootNode = document.SelectSingleNode(RootTag);

					if (rootNode == null)
						throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutInvalidRootElement", RootTag));

					// load the version
					Version version = new Version(rootNode.Attributes[VersionAttrib].Value);

					#region Currently Loaded Panes
					// all named panes we currently have
					Dictionary<string, ContentPane> currentPanes = new Dictionary<string, ContentPane>();

					// all current panes regardless of whether they have a name
					List<ContentPane> allCurrentPanes = new List<ContentPane>();

					foreach (ContentPane pane in DockManagerUtilities.GetAllPanes(dockManager, PaneFilterFlags.All))
					{
						if (false == string.IsNullOrEmpty(pane.Name))
						{
							Debug.Assert(!currentPanes.ContainsKey(pane.Name));
							currentPanes.Add(pane.Name, pane);
						}

						allCurrentPanes.Add(pane);
					}
					#endregion //Currently Loaded Panes

					// AS 5/17/08 Reuse Group/Split
					dockManager.OnBeforeLoadLayout();

					ParseInfo parseInfo = new ParseInfo(dockManager);

					#endregion //Prepare

					#region ContentPanes

					// content panes
					XmlNode contentPanes = rootNode.SelectSingleNode(ContentPanesTag);

                    // AS 10/17/08 TFS8130
                    Dictionary<ContentPane, ContentPaneInfo> paneInfoByPane = new Dictionary<ContentPane, ContentPaneInfo>();

					foreach (XmlNode paneNode in contentPanes.SelectNodes(ContentPaneTag))
					{
						// prepare the content panes
						string name = paneNode.Attributes[NameAttrib].Value;
						string serializationId = ReadAttribute(paneNode, SerializationIdAttrib, null);

						ContentPane pane;

						if (false == currentPanes.TryGetValue(name, out pane))
						{
							// raise an event (InitializePaneContent) to allow the developer to provide a content pane to use
							ContentPane newPane = new ContentPane();
							newPane.Name = name;
							newPane.SerializationId = serializationId;

							InitializePaneContentEventArgs args = new InitializePaneContentEventArgs(newPane);

							dockManager.RaiseInitializePaneContent(args);

                            // AS 10/17/08 TFS8130
                            // We're going to allow the pane to be set so it could have been 
                            // null'd out.
                            //
                            //if (newPane.Content != null && newPane.Name == name)
                            //    pane = newPane;
                            if (null != args.NewPane)
                            {
                                if (LogicalTreeHelper.GetParent(args.NewPane) != null || VisualTreeHelper.GetParent(args.NewPane) != null)
									throw new InvalidOperationException(XamDockManager.GetString("LE_NewPaneCannotHaveParent"));

                                // if they cleared out the content or they provided a custom pane
                                // then use that pane. otherwise assume they didn't want it
                                if (newPane.Content != null || newPane != args.NewPane)
                                    pane = args.NewPane;
                            }
						}

						if (null != pane)
						{
                            // AS 10/17/08 TFS8130
                            // Since they can provide a pane, let's make sure they don't try 
                            // to put one in two different slots.
                            //
                            if (paneInfoByPane.ContainsKey(pane))
								throw new InvalidOperationException(XamDockManager.GetString("LE_PaneAlreadyReferencedInLayout"));
                            
							ContentPaneInfo paneInfo = new ContentPaneInfo();

							paneInfo.Pane = pane;
							paneInfo.SerializationId = serializationId;
                            // AS 1/28/09 TFS11028
                            // Moved this down so we can try to intelligently default based on the location.
                            //
							//paneInfo.LastDockableState = ReadEnumAttribute(paneNode, LastDockableStateAttrib, DockableState.Floating);

							// AS 5/13/08
							// We'll support the IsClosed attribute but we won't write it out anymore.
							//
							//paneInfo.IsClosed = ReadAttribute(paneNode, IsClosedAttrib, false);
							Visibility VisibilityNotSet = unchecked((Visibility)(-1));
							Visibility visibility = ReadEnumAttribute(paneNode, VisibilityAttrib, VisibilityNotSet);

							if (visibility == VisibilityNotSet)
								visibility = ReadAttribute(paneNode, IsClosedAttrib, false) ? Visibility.Collapsed : Visibility.Visible;

							paneInfo.Visibility = visibility;

							paneInfo.Location = ReadEnumAttribute(paneNode, LocationAttrib, PaneLocation.Unknown);
							paneInfo.UnpinnedOrder = ReadAttribute(paneNode, UnpinnedOrderAttrib, 0);
							paneInfo.LastActivatedTime = ReadAttribute(paneNode, LastActivatedTimeAttrib, DateTime.MinValue);
							paneInfo.LastFloatingSize = ReadAttributeWithConverter(paneNode, LastFloatingSizeAttrib, Size.Empty);
							paneInfo.LastFloatingWindowRect = ReadAttributeWithConverter(paneNode, LastFloatingWindowRectAttrib, Rect.Empty);
							paneInfo.FlyoutExtent = ReadAttribute(paneNode, FlyoutExtentAttrib, double.NaN);
							paneInfo.LastFloatingPoint = ReadAttributeWithConverter<Point?>(paneNode, LastFloatingLocationAttrib, null);

							if (paneInfo.Location == PaneLocation.Unknown)
								throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutInvalidPaneLocation", name));

							if (paneInfo.Location == PaneLocation.Document && dockManager.HasDocumentContentHost == false)
								throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutNoDocumentContentHost", name));

                            // AS 1/28/09 TFS11028
                            // We were parsing this above but the enumeration that we saved was an internal enum so layouts
                            // that were saved while we were obfuscating would not be able to be parsed and would result in 
                            // an exception. If this should happen we will instead try to guess the last dockable state based 
                            // on the saved location which is why I had to move this down.
                            //
                            try
                            {
                                paneInfo.LastDockableState = ReadEnumAttribute(paneNode, LastDockableStateAttrib, DockableState.Floating);
                            }
                            catch
                            {
                                switch (paneInfo.Location)
                                {
                                    case PaneLocation.Floating:
                                    case PaneLocation.FloatingOnly:
                                        paneInfo.LastDockableState = DockableState.Floating;
                                        break;
                                    default:
                                        paneInfo.LastDockableState = DockableState.Docked;
                                        break;
                                }
                            }

                            parseInfo.LoadedPanes.Add(name, paneInfo);

                            // AS 10/17/08 TFS8130
                            paneInfoByPane.Add(pane, paneInfo);
						}
						else
						{
							// store the panes that don't exist so we can remove/ignore them
							// when processing the documents/panes
							parseInfo.LoadedPanes.Add(name, null);
						}
					}
					#endregion //ContentPanes

					#region Remove All Panes From Tree

					// remove all the content panes from the tree
					foreach (KeyValuePair<string, ContentPaneInfo> item in parseInfo.LoadedPanes)
					{
						ContentPane cp = item.Value != null ? item.Value.Pane : null;

						if (null != cp)
						{
							// AS 9/11/09 TFS21330
							//// AS 10/15/08 TFS8068
							//replacements.Add(DockManagerUtilities.CreateMoveReplacement(cp));
							DockManagerUtilities.AddMoveReplacement(replacements, cp);

							dockManager.ClosePaneHelper(cp, PaneCloseAction.RemovePane, false);

							Debug.Assert(cp.PaneLocation == PaneLocation.Unknown &&
								cp.PlacementInfo.CurrentContainer == null &&
								cp.PlacementInfo.DockableContainer == null &&
								cp.PlacementInfo.DockedContainer == null &&
								cp.PlacementInfo.DockedEdgePlaceholder == null &&
								cp.PlacementInfo.FloatingDockablePlaceholder == null &&
								cp.PlacementInfo.FloatingOnlyPlaceholder == null);

							cp.IsPinned = true; // start off as pinned
							cp.ClearValue(SplitPane.RelativeSizeProperty);
						}
					}
					#endregion //Remove All Panes From Tree

					#region Panes
					// panes
					XmlNode panesNode = rootNode.SelectSingleNode(PanesTag);
					// AS 5/2/08 BR32485
					// There may not have been any saved split panes so don't try
					// to get the list unless we had a <Panes> tag.
					//
					//XmlNodeList panesList = panesNode.SelectNodes(SplitPaneTag);
					//
					//if (null != panesList)
					if (null != panesNode)
					{
						XmlNodeList panesList = panesNode.SelectNodes(SplitPaneTag);

						// AS 5/17/08 Reuse Group/Split
						// See above. Basically we cannot do this anymore.
						//
						//dockManager.Panes.BeginUpdate();
						//
						//suspendedRootPaneUpdates = true;

						foreach (XmlNode pane in panesList)
						{
							PaneLocation location = ReadEnumAttribute<PaneLocation>(pane, LocationAttrib, PaneLocation.Unknown);

							switch (location)
							{
								// AS 5/17/08
								// Added explicit value checks in case we ever add other locations.
								//
								case PaneLocation.DockedBottom:
								case PaneLocation.DockedLeft:
								case PaneLocation.DockedRight:
								case PaneLocation.DockedTop:
								case PaneLocation.Floating:
								case PaneLocation.FloatingOnly:
									break;

								default:
								case PaneLocation.Unknown:
								case PaneLocation.Unpinned:
								case PaneLocation.Document:
									Debug.Fail("Unexpected root pane location!");
									continue;
							}

							// store the current location so we know where the pane would be. this is 
							// necessary since the pane location won't be set until the pane is loaded
							parseInfo.CurrentLocation = location;

							SplitPane split = LoadSplitPane(pane, parseInfo);

							if (null != split)
							{
								InitialPaneLocation initialLocation = DockManagerUtilities.GetInitialLocation(location);

								XamDockManager.SetInitialLocation(split, initialLocation);

								#region Root Split Props
								switch (location)
								{
									case PaneLocation.DockedBottom:
									case PaneLocation.DockedTop:
									case PaneLocation.DockedLeft:
									case PaneLocation.DockedRight:
										double extent = ReadAttribute(pane, ExtentAttrib, double.NaN);

										if (double.IsNaN(extent) == false)
										{
											if (location == PaneLocation.DockedRight || location == PaneLocation.DockedLeft)
												split.Width = extent;
											else
												split.Height = extent;
										}
										else
										{
											// AS 5/17/08 Reuse Group/Split
											split.ClearValue(FrameworkElement.WidthProperty);

											// AS 3/26/10 TFS28510
											split.ClearValue(FrameworkElement.HeightProperty);
										}
										break;
									case PaneLocation.Floating:
									case PaneLocation.FloatingOnly:
										Size floatSize = ReadAttributeWithConverter(pane, FloatingSizeAttrib, Size.Empty);
										Point floatLocation = ReadAttributeWithConverter(pane, FloatingLocationAttrib, new Point());

										XamDockManager.SetFloatingLocation(split, floatLocation);
										if (false == floatSize.IsEmpty)
											XamDockManager.SetFloatingSize(split, floatSize);
										else
										{
											// AS 5/17/08 Reuse Group/Split
											split.ClearValue(XamDockManager.FloatingSizeProperty);
										}
										break;
								}
								#endregion //Root Split Props

								dockManager.Panes.Add(split);

								#region Root Split Props
								switch (location)
								{
									case PaneLocation.Floating:
									case PaneLocation.FloatingOnly:
										// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
										// We need the toolwindow so now that its in the panes collection we should have 
										// that and we can set the window state & restore bounds on it.
										//
										WindowState floatState = ReadAttributeWithConverter(pane, FloatingWindowStateAttrib, WindowState.Normal);
										Rect floatRestoreBounds = ReadAttributeWithConverter(pane, FloatingRestoreBoundsAttrib, Rect.Empty);
										bool floatRestoreToMax = ReadAttribute(pane, FloatingRestoreToMaximizedAttrib, false);

										ToolWindow tw = ToolWindow.GetToolWindow(split);
										Debug.Assert(null != tw, "Unable to get to the toolwindow");

										if (null != tw)
										{
											tw.WindowState = floatState;

											if (floatState != WindowState.Normal)
												tw.SetRestoreBounds(floatRestoreBounds);

											if (floatState == WindowState.Minimized)
												tw.RestoreToMaximized = floatRestoreToMax;
										}
										break;
								}
								#endregion //Root Split Props

								// i don't really want to initialize the location here since 
								// i would prefer to have it centralized when the panes collection
								// is processed. that being said we need to know the state when we're
								// processing unpinned panes and since the state should be known,
								// we will initialize it here
								split.SetValue(XamDockManager.PaneLocationPropertyKey, DockManagerKnownBoxes.FromValue(location));
							}
						}
					}
					#endregion //Panes

					#region Documents

					// documents
					XmlNode documentsNode = rootNode.SelectSingleNode(DocumentsTag);

					if (null != documentsNode)
					{
						if (dockManager.HasDocumentContentHost)
						{
							// set the root splitter orientation
							dockManager.DocumentContentHost.RootSplitterOrientation = ParseEnum<Orientation>(documentsNode.Attributes[SplitterOrientationAttrib].Value);

							XmlNodeList documentsList = documentsNode.SelectNodes(SplitPaneTag);

							#region Root Document Splits
							if (null != documentsList)
							{
								ObservableCollectionExtended<SplitPane> documents = dockManager.DocumentContentHost.Panes;

								parseInfo.CurrentLocation = PaneLocation.Document;

                                // AS 11/24/08 TFS10802
                                // We cannot suspend the updates or else a reused root split pane
                                // would still have a PaneLocation because we're not actually 
                                // removing it from the logical tree. We were getting around that 
                                // in the ParseInfo.GetOrCreateSplitPane by clearing the 
                                // DP key for the PaneLocation but WPF has a bug whereby they 
                                // will no longer inherit values from an ancestor once a 
                                // readonly inherited property is set on an element.
                                //
								//documents.BeginUpdate();

								try
								{
									foreach (XmlNode pane in documentsList)
									{
										SplitPane split = LoadSplitPane(pane, parseInfo);

										if (null != split)
											documents.Add(split);
									}
								}
								finally
								{
                                    // AS 11/24/08 TFS10802
                                    //documents.EndUpdate();
								}
							} 
							#endregion //Root Document Splits
						}
					}
					#endregion //Documents

					#region ContentPane fixups

					List<UnpinnedTabAreaInfo.SortOrderInfo>[] unpinnedOrder = new List<UnpinnedTabAreaInfo.SortOrderInfo>[4];

					for (int i = 0; i < unpinnedOrder.Length; i++)
						unpinnedOrder[i] = new List<UnpinnedTabAreaInfo.SortOrderInfo>();

					// lastly fix up so that the cp's are where the last dockable location indicated
					foreach (KeyValuePair<string, ContentPaneInfo> item in parseInfo.LoadedPanes)
					{
						ContentPaneInfo cpi = item.Value;
						ContentPane cp = cpi != null ? cpi.Pane : null;

						if (null != cp)
						{
							#region Replace appropriate placeholder
							switch (cpi.Location)
							{
								case PaneLocation.Unpinned:
									{
										if (cp.PlacementInfo.DockedEdgePlaceholder == null)
											throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutPanePositionNotFound", cp.Name, cpi.Location));

										// put the pane in the docked container
										cp.PlacementInfo.DockedContainer.InsertContentPane(null, cp);

										Dock side = DockManagerUtilities.GetDockedSide(cp.PaneLocation);
										unpinnedOrder[(int)side].Add(new UnpinnedTabAreaInfo.SortOrderInfo(cp, cpi.UnpinnedOrder));

										// initialize its unpinned state
										cp.IsPinned = false;

										if (false == double.IsNaN(cpi.FlyoutExtent))
											UnpinnedTabFlyout.SetFlyoutExtent(cp, cpi.FlyoutExtent);
										else
											cp.ClearValue(UnpinnedTabFlyout.FlyoutExtentProperty);

										// process the move right now to avoid a flicker
										dockManager.ProcessPinnedState(cp, cp.PaneLocation, false);
										break;
									}
								case PaneLocation.DockedLeft:
								case PaneLocation.DockedBottom:
								case PaneLocation.DockedRight:
								case PaneLocation.DockedTop:
									{
										if (cp.PlacementInfo.DockedEdgePlaceholder == null)
											throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutPanePositionNotFound", cp.Name, cpi.Location));

										cp.PlacementInfo.DockedContainer.InsertContentPane(null, cp);
										break;
									}
								case PaneLocation.Document:
									{
										Debug.Assert(cp.PaneLocation == PaneLocation.Document);

										if (cp.PaneLocation != PaneLocation.Document)
											throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutPanePositionNotFound", cp.Name, cpi.Location));

										break;
									}
								case PaneLocation.Floating:
									{
										if (cp.PlacementInfo.FloatingDockablePlaceholder == null)
											throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutPanePositionNotFound", cp.Name, cpi.Location));

										cp.PlacementInfo.FloatingDockablePlaceholder.Container.InsertContentPane(null, cp);
										break;
									}
								case PaneLocation.FloatingOnly:
									{
										if (cp.PlacementInfo.FloatingOnlyPlaceholder == null)
											throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutPanePositionNotFound", cp.Name, cpi.Location));

										cp.PlacementInfo.FloatingOnlyPlaceholder.Container.InsertContentPane(null, cp);
										break;
									}
							} 
							#endregion //Replace appropriate placeholder

							// if the pane hasn't been activated then update the activation
							// order based on the saved activation time
							if (cp.LastActivatedTime < cpi.LastActivatedTime)
								cp.LastActivatedTime = cpi.LastActivatedTime;

							cp.LastFloatingSize = cpi.LastFloatingSize;
							cp.LastFloatingWindowRect = cpi.LastFloatingWindowRect;
							cp.LastFloatingLocation = cpi.LastFloatingPoint;

							// restore last dockable state. this is needed when the pane is displayed
							// as floating only or document and the end user reselects dockable
							cp.PlacementInfo.VerifyLastDockableContainer(cpi.LastDockableState);

							// restore closed state
							// AS 5/13/08
							//cp.SetValue(FrameworkElement.VisibilityProperty, cpi.IsClosed ? KnownBoxes.VisibilityCollapsedBox : KnownBoxes.VisibilityVisibleBox);
							cp.SetValue(FrameworkElement.VisibilityProperty, KnownBoxes.FromValue(cpi.Visibility));
						}
					}

					// make the pane manager rebuild the active pane list using the
					// updated sort order datetime.
					dockManager.ActivePaneManager.RefreshActivePaneSortOrder();

					#endregion //ContentPane fixups

					#region Sort Unpinned Panes
					// fix up unpinned tab order
					for (int i = 0; i < unpinnedOrder.Length; i++)
					{
						if (unpinnedOrder[i].Count > 1)
						{
							UnpinnedTabAreaInfo tabArea = dockManager.GetUnpinnedTabAreaInfo((Dock)i);

							tabArea.SortPanes(unpinnedOrder[i]);
						}
					} 
					#endregion //Sort Unpinned Panes

				}
			}
			finally
			{
				// AS 5/17/08 Reuse Group/Split
				// See above. Basically we cannot do this anymore.
				//
				//if (suspendedRootPaneUpdates)
				//	dockManager.Panes.EndUpdate();

				dockManager.IsLoadingLayout = false;

                // AS 10/15/08 TFS8068
                replacements.Dispose();

				
#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

				dockManager.ActivePaneManager.ResetActivePanesAfterLoad(hadActivePane);

				// AS 5/17/08 Reuse Group/Split
				// Since we're reusing groups, we could be reusing tool windows so
				// make sure they get updated after the layout is complete. Instead
				// of making this specific to that though, we'll just let the dm
				// know we're done with the load layout.
				//
				dockManager.OnAfterLoadLayout();
			}
		}
		#endregion //LoadLayout

		#region SaveLayout

		public static string SaveLayout(XamDockManager dockManager)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				SaveLayout(dockManager, ms);

				ms.Position = 0;
				StreamReader sr = new StreamReader(ms);
				return sr.ReadToEnd();
			}
		}

		public static void SaveLayout(XamDockManager dockManager, Stream stream)
		{
			// AS 2/3/10 TFS27137
			// Let the dockmanager know that we are about to save a layout so it 
			// can perform any pending operations that it may need to before the save.
			//
			dockManager.OnBeforeSaveLayout();

			XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
			writer.Formatting = Formatting.Indented;

			writer.WriteStartDocument();
			writer.WriteStartElement(RootTag); // <xamDockManager>
			writer.WriteAttributeString(VersionAttrib, AssemblyVersion.Version);

			#region ContentPanes

			// first build a list of the panes
			writer.WriteStartElement(ContentPanesTag); // <contentPanes>

			SerializationInfo info = new SerializationInfo();

			foreach (ContentPane pane in dockManager.GetPanes(PaneNavigationOrder.ActivationOrder))
			{
				// skip panes that shouldn't be serialized
				if (pane.SaveInLayout == false)
					continue;

				writer.WriteStartElement(ContentPaneTag); // <contentPane>

				if (string.IsNullOrEmpty(pane.Name) || info.NamedPanes.ContainsKey(pane.Name))
					throw new InvalidOperationException(XamDockManager.GetString("LE_SaveLayoutPaneNameMissing"));

				// store the panes by name so we can validate the uniqueness
				info.NamedPanes.Add(pane.Name, pane);

				writer.WriteAttributeString(NameAttrib, pane.Name);
				WriteAttribute(writer, LocationAttrib, pane.PaneLocation);

				// if the pane is not docked/floating but it was at one point, store where that was
				if (pane.PlacementInfo.DockableContainer != pane.PlacementInfo.CurrentContainer && null != pane.PlacementInfo.DockableContainer)
				{
					DockableState lastDockable = DockManagerUtilities.IsDocked(pane.PlacementInfo.DockableContainer.PaneLocation)
						? DockableState.Docked
						: DockableState.Floating;
					WriteAttribute(writer, LastDockableStateAttrib, lastDockable);
				}

				if (pane.LastFloatingSize.IsEmpty == false)
					WriteAttribute(writer, LastFloatingSizeAttrib, pane.LastFloatingSize);

				if (pane.LastFloatingWindowRect.IsEmpty == false)
					WriteAttribute(writer, LastFloatingWindowRectAttrib, pane.LastFloatingWindowRect);

				WriteAttribute(writer, LastFloatingLocationAttrib, pane.LastFloatingLocation);

				if (pane.LastActivatedTime != DateTime.MinValue)
					WriteAttribute(writer, LastActivatedTimeAttrib, pane.LastActivatedTime);

				if (false == string.IsNullOrEmpty(pane.SerializationId))
					writer.WriteAttributeString(SerializationIdAttrib, pane.SerializationId);

				// AS 5/13/08
				//if (pane.Visibility == Visibility.Collapsed)
				//	WriteAttribute(writer, IsClosedAttrib, true);
				if (pane.Visibility != Visibility.Visible)
					WriteAttribute(writer, VisibilityAttrib, pane.Visibility);

				#region Unpinned
				if (pane.IsPinned == false)
				{
					IContentPaneContainer container = pane.PlacementInfo.CurrentContainer;
					Debug.Assert(null != container && container is UnpinnedTabAreaInfo);
					UnpinnedTabArea tabArea = container != null ? container.ContainerElement as UnpinnedTabArea : null;

					if (null != tabArea)
						WriteAttribute(writer, UnpinnedOrderAttrib, tabArea.Items.IndexOf(pane));

					if (false == double.IsNaN(UnpinnedTabFlyout.GetFlyoutExtent(pane)))
						WriteAttribute(writer, FlyoutExtentAttrib, UnpinnedTabFlyout.GetFlyoutExtent(pane));
				}
				#endregion //Unpinned

				writer.WriteEndElement(); // </contentPane>
			}
			writer.WriteEndElement(); // <contentPanes> 

			#endregion //ContentPanes

			if (info.NamedPanes.Count > 0)
			{
				#region Panes

				if (dockManager.Panes.Count > 0)
				{
					writer.WriteStartElement(PanesTag); // <panes>

					foreach (SplitPane split in dockManager.Panes)
					{
						if (info.ShouldSerialize(split))
							WriteSplitPane(writer, split, true, info);
					}

					writer.WriteEndElement(); // </panes>
				}
				#endregion //Panes

				#region Documents
				if (dockManager.DocumentContentHost != null)
				{
					writer.WriteStartElement(DocumentsTag); // <documents>

					// the orientation of the root split pane
					writer.WriteAttributeString(SplitterOrientationAttrib, dockManager.DocumentContentHost.RootSplitterOrientation.ToString());

					foreach (SplitPane split in dockManager.DocumentContentHost.Panes)
					{
						if (info.ShouldSerialize(split))
							WriteSplitPane(writer, split, false, info);
					}

					writer.WriteEndElement(); // </documents>
				}
				#endregion //Documents
			}

			writer.WriteEndElement(); // </xamDockManager>
			writer.WriteEndDocument();
			writer.Flush();
		}

		#endregion //SaveLayout

		#endregion //Public Methods

		#region Private Methods

		#region GetItems
		private static object[] GetItems(System.Collections.IList list)
		{
			object[] items = null;
			items = new object[list.Count];
			list.CopyTo(items, 0);
			return items;
		}
		#endregion //GetItems

		#region LoadContentPane
		private static FrameworkElement LoadContentPane(XmlNode paneNode, ParseInfo parseInfo)
		{
			// read in the name so we know what contentpane this is associated with
			string name = ReadAttribute(paneNode, NameAttrib, null);

			if (name == null)
				throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutMissingName"));

			ContentPaneInfo cpi;

			if (false == parseInfo.LoadedPanes.TryGetValue(name, out cpi))
			{
				throw new InvalidOperationException(XamDockManager.GetString("LE_LoadLayoutUndefinedContentPane", name));
			}

			// if the pane didn't exist then don't deserialize it
			if (cpi == null || cpi.Pane == null)
				return null;

			FrameworkElement element = null;

			// we don't use placeholders for documents so return the pane itself
			if (parseInfo.CurrentLocation == PaneLocation.Document)
				element = cpi.Pane;
			else
			{
				ContentPanePlaceholder placeholder = new ContentPanePlaceholder();
				placeholder.Initialize(cpi.Pane);
				cpi.Pane.PlacementInfo.StorePlaceholder(placeholder, parseInfo.CurrentLocation);

				element = placeholder;
			}

			Size relativeSize = ReadAttributeWithConverter(paneNode, RelativeSizeAttrib, Size.Empty);

			if (false == relativeSize.IsEmpty)
				SplitPane.SetRelativeSize(element, relativeSize);

			return element;
		} 
		#endregion //LoadContentPane

		#region LoadSplitPane
		private static SplitPane LoadSplitPane(XmlNode paneNode, ParseInfo parseInfo)
		{
			
#region Infragistics Source Cleanup (Region)










#endregion // Infragistics Source Cleanup (Region)

			String name = ReadAttribute(paneNode, NameAttrib, String.Empty);
			Size relativeSize = ReadAttributeWithConverter(paneNode, RelativeSizeAttrib, Size.Empty);
			Orientation orientation = ReadEnumAttribute(paneNode, SplitterOrientationAttrib, LayoutManager.DefaultSplitterOrientation);
			SplitPane group = parseInfo.GetOrCreateSplitPane(name, relativeSize, orientation);
			object[] originalItems = GetItems(group.Panes);

			foreach (XmlNode child in paneNode.ChildNodes)
			{
				FrameworkElement childElement = null;

				if (child.Name == ContentPaneTag)
				{
					if (parseInfo.CurrentLocation == PaneLocation.Document)
						throw new InvalidOperationException(XamDockManager.GetString("LE_InvalidDocumentSplitPaneChild"));

					childElement = LoadContentPane(child, parseInfo);
				}
				else if (child.Name == TabGroupPaneTag)
				{
					childElement = LoadTabGroupPane(child, parseInfo);
				}
				else if (child.Name == SplitPaneTag)
				{
					childElement = LoadSplitPane(child, parseInfo);
				}

				if (null != childElement)
					group.Panes.Add(childElement);
			}

			// if the pane didn't have any panes deserialized then don't include it
			// AS 5/17/08 Reuse Group/Split
			// We do want to keep it if it has a non-autogenerated name.
			//
			//if (group.Panes.Count == 0)
			if (group.Panes.Count == 0 && false == DockManagerUtilities.ShouldPreventPaneRemoval(group))
				return null;

			// AS 5/17/08 Reuse Group/Split
			parseInfo.ReorderOriginalItems(originalItems, group, group.Panes);

			return group;
		}
		#endregion //LoadSplitPane

		#region LoadTabGroupPane
		private static TabGroupPane LoadTabGroupPane(XmlNode paneNode, ParseInfo parseInfo)
		{
			
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

			String name = ReadAttribute(paneNode, NameAttrib, string.Empty);
			Size relativeSize = ReadAttributeWithConverter(paneNode, RelativeSizeAttrib, Size.Empty);
			TabGroupPane group = parseInfo.GetOrCreateTabGroup(name, relativeSize);
			int initialCount = group.Items.Count;
			object[] originalItems = GetItems(group.Items);

			foreach (XmlNode child in paneNode.ChildNodes)
			{
				FrameworkElement childElement = null;

				if (child.Name == ContentPaneTag)
				{
					childElement = LoadContentPane(child, parseInfo);
				}
				else
				{
					Debug.Fail("Unexpected child pane!:" + child.Name);
				}

				if (null != childElement)
					group.Items.Add(childElement);
			}

			// restore the selected index
			int selectedIndex = ReadAttribute(paneNode, SelectedIndexAttrib, -1);

			// AS 5/17/08 Reuse Group/Split
			// Offset by the number of items we had in their to start.
			//
			selectedIndex += initialCount;

			group.SelectedIndex = Math.Max(-1, Math.Min(group.Items.Count - 1, selectedIndex));

			// if the pane didn't have any panes deserialized then don't include it
			// AS 5/17/08 Reuse Group/Split
			// We do want to keep it if it has a non-autogenerated name.
			//
			//if (group.Items.Count == 0)
			if (group.Items.Count == 0 && false == DockManagerUtilities.ShouldPreventPaneRemoval(group))
				return null;

			// AS 5/17/08 Reuse Group/Split
			object selectedValue = group.SelectedIndex >= 0 ? group.Items[group.SelectedIndex] : DependencyProperty.UnsetValue;

			if (parseInfo.ReorderOriginalItems(originalItems, group, group.Items))
				group.SelectedIndex = group.Items.IndexOf(selectedValue);

			return group;
		} 
		#endregion //LoadTabGroupPane

		#region ParseEnum (string)


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static T ParseEnum<T>(string value)
		{
			Debug.Assert(typeof(T).IsEnum);

			// AS 3/17/06 Case Insensitive
			// I decided not to use IgnoreCase here since I think we always
			// want this to be case sensitive.
			//
			return (T)Enum.Parse(typeof(T), value, false);
		}
		#endregion //ParseEnum (string)

		#region ReadAttribute
		private static string ReadAttribute(XmlNode node, string name, string defaultValue)
		{
			XmlAttribute attrib = node.Attributes[name];

			if (attrib != null)
				return attrib.Value;

			return defaultValue;
		}

		private static bool ReadAttribute(XmlNode node, string name, bool defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToBoolean(attrib) : defaultValue;
		}

		private static int ReadAttribute(XmlNode node, string name, int defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToInt32(attrib) : defaultValue;
		}

		private static DateTime ReadAttribute(XmlNode node, string name, DateTime defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToDateTime(attrib, XmlDateTimeSerializationMode.Local) : defaultValue;
		}

		private static double ReadAttribute(XmlNode node, string name, double defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToDouble(attrib) : defaultValue;
		}

		private static T ReadAttributeWithConverter<T>(XmlNode node, string name, T defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			if (attrib == null)
				return defaultValue;

			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

			return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, attrib);
		}

		private static T ReadEnumAttribute<T>(XmlNode node, string name, T defaultValue)
		{
			Debug.Assert(typeof(T).IsEnum);
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? (T)Enum.Parse(defaultValue.GetType(), attrib) : defaultValue;
		}
		#endregion //ReadAttribute

		#region WriteAttribute
		private static void WriteAttribute(XmlWriter writer, string name, double value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Enum value)
		{
			writer.WriteAttributeString(name, value.ToString());
		}

		private static void WriteAttribute(XmlWriter writer, string name, int value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		private static void WriteAttribute(XmlWriter writer, string name, bool value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Size value)
		{
			if (value.IsEmpty == false)
				WriteAttribute(writer, name, value, TypeDescriptor.GetConverter(typeof(Size)));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Rect value)
		{
			if (value.IsEmpty == false)
				WriteAttribute(writer, name, value, TypeDescriptor.GetConverter(typeof(Rect)));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Point? value)
		{
			if (null != value)
				WriteAttribute(writer, name, value.Value, TypeDescriptor.GetConverter(typeof(Point)));
		}

		private static void WriteAttribute(XmlWriter writer, string name, object value, TypeConverter converter)
		{
			writer.WriteAttributeString(name, converter.ConvertToString(null, CultureInfo.InvariantCulture, value));
		}

		private static void WriteAttribute(XmlTextWriter writer, string name, DateTime dateTime)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc));
		}
		#endregion //WriteAttribute

		#region WriteContentPane
		private static void WriteContentPane(XmlWriter writer, FrameworkElement child, SerializationInfo info)
		{
			Debug.Assert(child is ContentPane || child is ContentPanePlaceholder);
			Debug.Assert(info.ShouldSerialize(child));

			writer.WriteStartElement(ContentPaneTag); // <contentpane>

			string name = child is ContentPanePlaceholder ? ((ContentPanePlaceholder)child).Pane.Name : child.Name;
			writer.WriteAttributeString(NameAttrib, name);
			WriteRelativeSize(writer, child);

			writer.WriteEndElement(); // </contentPane>
		}
		#endregion //WriteContentPane

		#region WriteSplitPane
		private static void WriteSplitPane(XmlWriter writer, SplitPane pane, bool isRootDmPane, SerializationInfo info)
		{
			Debug.Assert(info.ShouldSerialize(pane));

			writer.WriteStartElement(SplitPaneTag);

			// AS 5/17/08 Reuse Group/Split
			if (false == string.IsNullOrEmpty(pane.Name))
			{
				if (info.NamedSplits.ContainsKey(pane.Name))
					throw new InvalidOperationException(XamDockManager.GetString("LE_MultipleContainersWithSameName", typeof(SplitPane), pane.Name));

				info.NamedSplits.Add(pane.Name, pane);
				writer.WriteAttributeString(NameAttrib, pane.Name);
			}

			writer.WriteAttributeString(SplitterOrientationAttrib, pane.SplitterOrientation.ToString());

			bool storeRelativeSize = isRootDmPane == false;

			#region IsRootPane
			if (isRootDmPane)
			{
				PaneLocation location = XamDockManager.GetPaneLocation(pane);

				WriteAttribute(writer, LocationAttrib, location);

				switch (location)
				{
					case PaneLocation.DockedLeft:
					case PaneLocation.DockedRight:
						// AS 10/5/09 NA 2010.1 - LayoutMode
						//if (false == double.IsNaN(pane.Width))
						//	WriteAttribute(writer, ExtentAttrib, pane.Width);
						double width = pane.WidthToSerialize;
						if (false == double.IsNaN(width))
							WriteAttribute(writer, ExtentAttrib, width);
						break;
					case PaneLocation.DockedTop:
					case PaneLocation.DockedBottom:
						// AS 10/5/09 NA 2010.1 - LayoutMode
						//if (false == double.IsNaN(pane.Height))
						//	WriteAttribute(writer, ExtentAttrib, pane.Height);
						double height = pane.HeightToSerialize;
						if (false == double.IsNaN(height))
							WriteAttribute(writer, ExtentAttrib, height);
						break;
					case PaneLocation.Floating:
					case PaneLocation.FloatingOnly:
					{
						WriteAttribute(writer, FloatingLocationAttrib, XamDockManager.GetFloatingLocation(pane));
						WriteAttribute(writer, FloatingSizeAttrib, XamDockManager.GetFloatingSize(pane));

						// AS 1/26/11 NA 2011 Vol 1 - Min/Max/Taskbar
						ToolWindow tw = ToolWindow.GetToolWindow(pane);
						Debug.Assert(null != tw, "Unable to get to the toolwindow.");

						if (tw.WindowState != WindowState.Normal)
						{
							WriteAttribute(writer, FloatingWindowStateAttrib, tw.WindowState);
							WriteAttribute(writer, FloatingRestoreBoundsAttrib, tw.GetRestoreBounds());

							if (tw.WindowState == WindowState.Minimized && tw.RestoreToMaximized)
								WriteAttribute(writer, FloatingRestoreToMaximizedAttrib, true);
						}
						break;
					}
					default:
						Debug.Fail("Unexpected location - " + location.ToString());
						break;
				}
			}
			else
			{
				WriteRelativeSize(writer, pane);
			}
			#endregion //IsRootPane

			foreach (FrameworkElement child in pane.Panes)
			{
				if (false == info.ShouldSerialize(child))
					continue;

				if (child is ContentPane || child is ContentPanePlaceholder)
					WriteContentPane(writer, child, info);
				else if (child is SplitPane)
					WriteSplitPane(writer, (SplitPane)child, false, info);
				else if (child is TabGroupPane)
					WriteTabGroup(writer, (TabGroupPane)child, info);
			}

			writer.WriteEndElement();
		}
		#endregion //WriteSplitPane

		#region WriteRelativeSize
		private static void WriteRelativeSize(XmlWriter writer, FrameworkElement element)
		{
			PropertyDescriptor pd = DependencyPropertyDescriptor.FromProperty(SplitPane.RelativeSizeProperty, element.GetType());

			if (pd == null || pd.ShouldSerializeValue(element))
				WriteAttribute(writer, RelativeSizeAttrib, SplitPane.GetRelativeSize(element));
		} 
		#endregion //WriteRelativeSize

		#region WriteTabGroup
		private static void WriteTabGroup(XmlWriter writer, TabGroupPane group, SerializationInfo info)
		{
			Debug.Assert(info.ShouldSerialize(group));

			writer.WriteStartElement(TabGroupPaneTag); // <tabGroup>

			// AS 5/17/08 Reuse Group/Split
			if (false == string.IsNullOrEmpty(group.Name))
			{
				if (info.NamedTabGroups.ContainsKey(group.Name))
					throw new InvalidOperationException(XamDockManager.GetString("LE_MultipleContainersWithSameName", typeof(TabGroupPane), group.Name));

				info.NamedTabGroups.Add(group.Name, group);
				writer.WriteAttributeString(NameAttrib, group.Name);
			}

			int selectedIndex = group.SelectedIndex;

			for(int i = 0; i <= selectedIndex; i++)
			{
				if (false == info.ShouldSerialize(group.Items[i]))
					selectedIndex--;
			}

			WriteAttribute(writer, SelectedIndexAttrib, selectedIndex);

			WriteRelativeSize(writer, group);

			foreach (object item in group.Items)
			{
				Debug.Assert(item is ContentPane || item is ContentPanePlaceholder);

				if (false == info.ShouldSerialize(item))
					continue;

				if (item is ContentPane || item is ContentPanePlaceholder)
					WriteContentPane(writer, item as FrameworkElement, info);
			}

			writer.WriteEndElement(); // </contentPane>
		}
		#endregion //WriteTabGroup

		#endregion //Private Methods

		#region ContentPaneInfo class
		private class ContentPaneInfo
		{
			internal ContentPane Pane;
			internal string SerializationId;
			internal PaneLocation Location;
			internal int UnpinnedOrder;
			//internal bool IsClosed;
			internal Visibility Visibility = Visibility.Visible;
			internal DockableState LastDockableState;
			internal DateTime LastActivatedTime = DateTime.MinValue;
			internal Size LastFloatingSize = Size.Empty;
			internal Point? LastFloatingPoint = null;
			internal Rect LastFloatingWindowRect = Rect.Empty;
			internal double FlyoutExtent = double.NaN;
		} 
		#endregion //ContentPaneInfo class

		#region ParseInfo class
		private class ParseInfo
		{
			#region Member Variables

			private Dictionary<string, ContentPaneInfo> _loadedPanes;
			private PaneLocation _currentLocation;
			private XamDockManager _dockManager;

			// AS 5/17/08 Reuse Group/Split
			private Dictionary<string, TabGroupPane> _originalTabGroups;
			private Dictionary<string, SplitPane> _originalSplitPanes;
			private Dictionary<FrameworkElement, List<object>> _siblingPanes;

			#endregion //Member Variables

			#region Constructor
			internal ParseInfo(XamDockManager dockManager)
			{
				this._dockManager = dockManager;
				this._loadedPanes = new Dictionary<string, ContentPaneInfo>();

				// AS 5/17/08 Reuse Group/Split
				this.InitializeOriginalPanes();
			}
			#endregion //Constructor

			#region Properties

			internal PaneLocation CurrentLocation
			{
				get { return this._currentLocation; }
				set { this._currentLocation = value; }
			}

			internal Dictionary<string, ContentPaneInfo> LoadedPanes
			{
				get { return this._loadedPanes; }
			}
			#endregion //Properties

			// AS 5/17/08 Reuse Group/Split
			#region Methods

			#region CompareItems
			private static bool CompareItems(object item1, object item2)
			{
				if (object.ReferenceEquals(item1, item2))
					return true;

				if ((item1 is ContentPane || item1 is ContentPanePlaceholder)
					&&
					(item2 is ContentPane || item2 is ContentPanePlaceholder))
				{
					ContentPane pane1 = item1 as ContentPane ?? ((ContentPanePlaceholder)item1).Pane;
					ContentPane pane2 = item2 as ContentPane ?? ((ContentPanePlaceholder)item2).Pane;

					return pane1 == pane2;
				}

				return false;
			} 
			#endregion //CompareItems

			#region FindSiblingItem
			private static int FindSiblingItem(object originalItem, IList currentList)
			{
				for (int i = 0, count = currentList.Count; i < count; i++)
				{
					if (CompareItems(originalItem, currentList[i]))
						return i;
				}

				return -1;
			} 
			#endregion //FindSiblingItem

			#region GetOrCreateTabGroup
			internal TabGroupPane GetOrCreateTabGroup(string name, Size relativeSize)
			{
				TabGroupPane tabGroup = null;
				bool hasName = false == string.IsNullOrEmpty(name);

				if (hasName && this._originalTabGroups.TryGetValue(name, out tabGroup))
				{
					Debug.Assert(null != tabGroup, "We've already moved this pane during the load!");

					if (null != tabGroup)
					{
						IPaneContainer container = DockManagerUtilities.GetParentPane(tabGroup);

						// the container could be null if it was in the dm before we pulled the panes
						// out that we will be repositioning because they are referenced by the layout
						if (null != container)
						{
							container.RemovePane(tabGroup);
							DockManagerUtilities.RemoveContainerIfNeeded(container);
						}
					}

					// clear the tabgroup but keep it in the list for debugging 
					// so we know we had it at one point
					this._originalTabGroups[name] = null;
				}

				if (null == tabGroup)
				{
					tabGroup = DockManagerUtilities.CreateTabGroup(_dockManager);

					// if the saved pane had a name and we didn't reuse a pane
					// then assign the saved name. note i'm not sure if picking
					// up a name we didn't generate won't cause a problem down
					// the line
					if (hasName && name != tabGroup.Name)
						tabGroup.Name = name;
				}

				if (relativeSize != SplitPane.GetRelativeSize(tabGroup))
				{
					if (relativeSize.IsEmpty)
						tabGroup.ClearValue(SplitPane.RelativeSizeProperty);
					else
						SplitPane.SetRelativeSize(tabGroup, relativeSize);
				}

				return tabGroup;
			}
			#endregion // GetOrCreateTabGroup

			#region GetOrCreateSplitPane
			internal SplitPane GetOrCreateSplitPane(string name, Size relativeSize, Orientation orientation)
			{
				SplitPane split = null;
				bool hasName = false == string.IsNullOrEmpty(name);

				if (hasName && this._originalSplitPanes.TryGetValue(name, out split))
				{
					Debug.Assert(null != split, "We've already moved this pane during the load!");

					if (null != split)
					{
						IPaneContainer container = DockManagerUtilities.GetParentPane(split);

						// the container could be null if it was in the dm before we pulled the panes
						// out that we will be repositioning because they are referenced by the layout
						if (null != container)
						{
							container.RemovePane(split);

                            // AS 11/24/08 TFS10802
                            // There is a bug in the WPF framework that if you set a value
                            // on a read only inheritance property or clear it, then it will
                            // no longer pick up the value of the ancestor. We only did this 
                            // because we had suspended the update notifications. We're not
                            // going to do that anymore.
                            //
							//// if somehow this split was a root pane, make sure it doesn't have a pane location
							//// since we've suspended updates to the panes collection, this could likely be set still
							//split.ClearValue(XamDockManager.PaneLocationPropertyKey);

							DockManagerUtilities.RemoveContainerIfNeeded(container);
						}
					}

					// clear the tabgroup but keep it in the list for debugging 
					// so we know we had it at one point
					this._originalSplitPanes[name] = null;
				}

				if (null == split)
				{
					split = DockManagerUtilities.CreateSplitPane(_dockManager);

					// if the saved pane had a name and we didn't reuse a pane
					// then assign the saved name. note i'm not sure if picking
					// up a name we didn't generate won't cause a problem down
					// the line
					if (hasName && name != split.Name)
						split.Name = name;
				}

				if (relativeSize != SplitPane.GetRelativeSize(split))
				{
					if (relativeSize.IsEmpty)
						split.ClearValue(SplitPane.RelativeSizeProperty);
					else
						SplitPane.SetRelativeSize(split, relativeSize);
				}

				if (orientation != split.SplitterOrientation)
					split.SetValue(SplitPane.SplitterOrientationProperty, KnownBoxes.FromValue(orientation));

				return split;
			}
			#endregion // GetOrCreateSplitPane

			#region InitializeOriginalPanes
			private void InitializeOriginalPanes()
			{
				this._originalSplitPanes = new Dictionary<string, SplitPane>();
				this._originalTabGroups = new Dictionary<string, TabGroupPane>();
				this._siblingPanes = new Dictionary<FrameworkElement, List<object>>();

				foreach (SplitPane split in this._dockManager.Panes)
					this.InitializeOriginalPanes(split);

				if (this._dockManager.HasDocumentContentHost)
				{
					foreach (SplitPane documentSplit in this._dockManager.DocumentContentHost.Panes)
						InitializeOriginalPanes(documentSplit);
				}
			}

			private void InitializeOriginalPanes(SplitPane pane)
			{
				if (false == string.IsNullOrEmpty(pane.Name))
					this._originalSplitPanes.Add(pane.Name, pane);

				List<object> panes = new List<object>();

				foreach (FrameworkElement element in pane.Panes)
				{
					// track the order of the panes so we can try
					// to reposition panes not saved in the layout
					panes.Add(element);

					SplitPane childSplit = element as SplitPane;

					if (null != childSplit)
						this.InitializeOriginalPanes(childSplit);
					else
					{
						TabGroupPane tabGroup = element as TabGroupPane;

						if (null != tabGroup)
						{
							if (false == string.IsNullOrEmpty(tabGroup.Name))
								this._originalTabGroups.Add(tabGroup.Name, tabGroup);

							List<object> tabItems = new List<object>();

							foreach (object item in tabGroup.Items)
							{
								tabItems.Add(item);
							}

							this._siblingPanes.Add(tabGroup, tabItems);
						}
					}
				}

				this._siblingPanes.Add(pane, panes);
			}
			#endregion // InitializeOriginalPanes

			#region ReorderOriginalItems
			internal bool ReorderOriginalItems(object[] originalItems, FrameworkElement parent, IList currentItems)
			{
				List<object> siblingList;

				if (null != originalItems
					&& originalItems.Length > 0
					&& this._siblingPanes.TryGetValue(parent, out siblingList))
				{
					return ReorderOriginalItems(originalItems, siblingList, currentItems);
				}

				return false;
			}

			private static bool ReorderOriginalItems(object[] originalItems, List<object> originalSiblingList, IList currentItems)
			{
				bool movedItem = false;

				// iterate the original list and reposition the things based on where they
				// were in the list we saved for the group
				for (int i = 0; i < originalItems.Length; i++)
				{
					object originalItem = originalItems[i];
					int originalIndex = originalSiblingList.IndexOf(originalItem);
					int currentIndex = currentItems.IndexOf(originalItem);

					// AS 3/17/11 TFS67321
					// Skip items no longer in the collection
					//
					if (currentIndex < 0)
						continue;

					Debug.Assert(originalIndex >= 0);

					if (originalIndex >= 0)
					{
						bool processedItem = false;

						// look for and item that it followed so we can position
						// it immediately after that item
						for (int beforeIndex = originalIndex - 1; beforeIndex >= 0; beforeIndex--)
						{
							// get the item that used to be before the original
							object siblingItem = originalSiblingList[beforeIndex];

							// where is that item in the list now?
							int siblingIndex = FindSiblingItem(siblingItem, currentItems);

							// if its still in the list...
							if (siblingIndex >= 0)
							{
								// we want to position after that item so bump the index
								if (currentIndex > siblingIndex)
									siblingIndex++;

								// if its there then this is not being moved
								if (siblingIndex == currentIndex)
								{
									// set a flag so we don't bother processing the after list
									processedItem = true;
									break;
								}

								// reposition the item after that item in the sibling list
								currentItems.RemoveAt(currentIndex);
								currentItems.Insert(siblingIndex, originalItem);
								movedItem = true;

								// AS 7/16/09 TFS19529
								// We also want to skip the after list because we have moved it
								// and we want to break out of the for loop we're in.
								//
								processedItem = true;
								break;
							}
						}

						if (processedItem)
							continue;

						// then look for an item that it was before
						for (int afterIndex = originalIndex + 1; afterIndex < originalSiblingList.Count; afterIndex++)
						{
							object siblingItem = originalSiblingList[afterIndex];

							// find that item in the specified list and if found, move it after that
							int siblingIndex = FindSiblingItem(siblingItem, currentItems);

							if (siblingIndex >= 0)
							{
								// if the item is already before that then decrement the index
								// since it would be one lower if it weren't in the list
								if (currentIndex < siblingIndex)
									siblingIndex--;

								// if its already there then we're done
								if (siblingIndex == currentIndex)
									break;

								// reposition the item before that item in the sibling list
								currentItems.RemoveAt(currentIndex);
								currentItems.Insert(siblingIndex, originalItem);
								movedItem = true;

								// AS 7/16/09 TFS19529
								// We want to break out of the for loop we're in.
								//
								break;
							}
						}
					}
				}

				return movedItem;
			} 
			#endregion //ReorderOriginalItems

			#endregion // Methods
		}
		#endregion //ParseInfo class

		#region SerializationInfo class
		private class SerializationInfo
		{
			#region Member Variables

			private Dictionary<string, ContentPane> _namedPanes;

			// AS 5/17/08 Reuse Group/Split
			private Dictionary<string, SplitPane> _namedSplits;
			private Dictionary<string, TabGroupPane> _namedTabGroups;

			// AS 5/17/08 Reuse Group/Split
			// Optimization to prevent walking down the tree multiple times.
			//
			private Dictionary<object, bool> _shouldSerializeStates;

			#endregion //Member Variables

			#region Constructor
			internal SerializationInfo()
			{
				this._namedPanes = new Dictionary<string, ContentPane>();

				// AS 5/17/08 Reuse Group/Split
				this._namedSplits = new Dictionary<string, SplitPane>();
				this._namedTabGroups = new Dictionary<string, TabGroupPane>();
				this._shouldSerializeStates = new Dictionary<object, bool>();
			} 
			#endregion //Constructor

			#region Properties

			#region NamedPanes
			internal Dictionary<string, ContentPane> NamedPanes
			{
				get { return this._namedPanes; }
			}
			#endregion //NamedPanes

			// AS 5/17/08 Reuse Group/Split
			#region NamedSplits
			internal Dictionary<string, SplitPane> NamedSplits
			{
				get { return this._namedSplits; }
			}
			#endregion //NamedSplits

			// AS 5/17/08 Reuse Group/Split
			#region NamedTabGroups
			internal Dictionary<string, TabGroupPane> NamedTabGroups
			{
				get { return this._namedTabGroups; }
			}
			#endregion //NamedTabGroups

			#endregion //Properties

			#region Methods

			#region ShouldSerialize
			private bool ShouldSerialize(SplitPane pane)
			{
				// AS 5/17/08 Reuse Group/Split
				// if this is a pane we can't remove then we should
				// save the pane in the layout so we know where it
				// should exist in the loaded layout
				if (DockManagerUtilities.ShouldPreventPaneRemoval(pane))
					return true;

				foreach (FrameworkElement element in pane.Panes)
				{
					if (ShouldSerialize(element))
						return true;
				}

				return false;
			}

			internal bool ShouldSerialize(object item)
			{
				
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

				bool shouldSerialize;

				if (false == _shouldSerializeStates.TryGetValue(item, out shouldSerialize))
				{
					if (item is TabGroupPane)
						shouldSerialize = ShouldSerialize((TabGroupPane)item);
					else if (item is ContentPane)
						shouldSerialize = ShouldSerialize((ContentPane)item);
					else if (item is ContentPanePlaceholder)
						shouldSerialize = ShouldSerialize((ContentPanePlaceholder)item);
					else if (item is SplitPane)
						shouldSerialize = ShouldSerialize((SplitPane)item);
					else
					{
						Debug.Fail("Unexpected item type!");
						shouldSerialize = false;
					}

					// cache the should serialize state
					_shouldSerializeStates.Add(item, shouldSerialize);
				}

				return shouldSerialize;
			}

			private bool ShouldSerialize(TabGroupPane tabGroup)
			{
				// AS 5/17/08 Reuse Group/Split
				// if this is a pane we can't remove then we should
				// save the pane in the layout so we know where it
				// should exist in the loaded layout
				if (DockManagerUtilities.ShouldPreventPaneRemoval(tabGroup))
					return true;

				foreach (object item in tabGroup.Items)
				{
					if (ShouldSerialize(item))
						return true;
				}

				return false;
			}

			private bool ShouldSerialize(ContentPane pane)
			{
				// AS 7/15/09 TFS18453
				// This is what lead to the exception. Essentially we were only checking 
				// if we had a pane with the specified name in our list. However, the customer 
				// was removing the old pane and adding a new one before the load so we had a 
				// pane with that name but it wasn't the one associated with the placeholder that 
				// for which we were checking the ShouldSerialize.
				//
				//return this._namedPanes.ContainsKey(pane.Name);
				ContentPane actualPane;

				if (_namedPanes.TryGetValue(pane.Name, out actualPane))
					return pane == actualPane;

				return false;
			}

			private bool ShouldSerialize(ContentPanePlaceholder placeholder)
			{
				return this.ShouldSerialize(placeholder.Pane);
			}
			#endregion //ShouldSerialize

			#endregion //Methods
		} 
		#endregion //SerializationInfo class
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