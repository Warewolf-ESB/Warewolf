
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using CubicOrange.Windows.Forms.ActiveDirectory;

namespace ADPickerTester
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lblFeedback;
		private System.Windows.Forms.Button btnInvoke;
        private CheckedListBox chklistDefaultTypes;
        private Label label1;
        private CheckBox chkMultiSelect;
        private Label label2;
        private CheckedListBox chklistAllowedTypes;
        private Label label3;
        private CheckedListBox chklistAllowedLocations;
        private Label label4;
        private CheckedListBox chklistDefaultLocations;
        private CheckBox chkShowAdvanced;
        private TextBox txtTargetComputer;
        private Label label5;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.lblFeedback = new System.Windows.Forms.Label();
            this.btnInvoke = new System.Windows.Forms.Button();
            this.chklistDefaultTypes = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.chkMultiSelect = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chklistAllowedTypes = new System.Windows.Forms.CheckedListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chklistAllowedLocations = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chklistDefaultLocations = new System.Windows.Forms.CheckedListBox();
            this.chkShowAdvanced = new System.Windows.Forms.CheckBox();
            this.txtTargetComputer = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblFeedback
            // 
            this.lblFeedback.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblFeedback.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblFeedback.Location = new System.Drawing.Point(0, 213);
            this.lblFeedback.Name = "lblFeedback";
            this.lblFeedback.Size = new System.Drawing.Size(538, 246);
            this.lblFeedback.TabIndex = 0;
            // 
            // btnInvoke
            // 
            this.btnInvoke.Location = new System.Drawing.Point(405, 166);
            this.btnInvoke.Name = "btnInvoke";
            this.btnInvoke.Size = new System.Drawing.Size(121, 39);
            this.btnInvoke.TabIndex = 1;
            this.btnInvoke.Text = "Directory Object Picker";
            this.btnInvoke.Click += new System.EventHandler(this.btnInvoke_Click);
            // 
            // chklistDefaultTypes
            // 
            this.chklistDefaultTypes.FormattingEnabled = true;
            this.chklistDefaultTypes.Location = new System.Drawing.Point(279, 25);
            this.chklistDefaultTypes.Name = "chklistDefaultTypes";
            this.chklistDefaultTypes.Size = new System.Drawing.Size(120, 124);
            this.chklistDefaultTypes.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(279, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(73, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Default Types";
            // 
            // chkMultiSelect
            // 
            this.chkMultiSelect.AutoSize = true;
            this.chkMultiSelect.Location = new System.Drawing.Point(279, 184);
            this.chkMultiSelect.Name = "chkMultiSelect";
            this.chkMultiSelect.Size = new System.Drawing.Size(81, 17);
            this.chkMultiSelect.TabIndex = 4;
            this.chkMultiSelect.Text = "Multi-Select";
            this.chkMultiSelect.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(76, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Allowed Types";
            // 
            // chklistAllowedTypes
            // 
            this.chklistAllowedTypes.FormattingEnabled = true;
            this.chklistAllowedTypes.Location = new System.Drawing.Point(12, 25);
            this.chklistAllowedTypes.Name = "chklistAllowedTypes";
            this.chklistAllowedTypes.Size = new System.Drawing.Size(120, 124);
            this.chklistAllowedTypes.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(138, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(93, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Allowed Locations";
            // 
            // chklistAllowedLocations
            // 
            this.chklistAllowedLocations.FormattingEnabled = true;
            this.chklistAllowedLocations.Location = new System.Drawing.Point(138, 25);
            this.chklistAllowedLocations.Name = "chklistAllowedLocations";
            this.chklistAllowedLocations.Size = new System.Drawing.Size(120, 124);
            this.chklistAllowedLocations.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(405, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(90, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Default Locations";
            // 
            // chklistDefaultLocations
            // 
            this.chklistDefaultLocations.FormattingEnabled = true;
            this.chklistDefaultLocations.Location = new System.Drawing.Point(405, 25);
            this.chklistDefaultLocations.Name = "chklistDefaultLocations";
            this.chklistDefaultLocations.Size = new System.Drawing.Size(120, 124);
            this.chklistDefaultLocations.TabIndex = 9;
            // 
            // chkShowAdvanced
            // 
            this.chkShowAdvanced.AutoSize = true;
            this.chkShowAdvanced.Location = new System.Drawing.Point(279, 166);
            this.chkShowAdvanced.Name = "chkShowAdvanced";
            this.chkShowAdvanced.Size = new System.Drawing.Size(105, 17);
            this.chkShowAdvanced.TabIndex = 11;
            this.chkShowAdvanced.Text = "Show Advanced";
            this.chkShowAdvanced.UseVisualStyleBackColor = true;
            // 
            // txtTargetComputer
            // 
            this.txtTargetComputer.Location = new System.Drawing.Point(15, 182);
            this.txtTargetComputer.Name = "txtTargetComputer";
            this.txtTargetComputer.Size = new System.Drawing.Size(243, 20);
            this.txtTargetComputer.TabIndex = 12;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 166);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(86, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Target Computer";
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(538, 459);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtTargetComputer);
            this.Controls.Add(this.chkShowAdvanced);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chklistDefaultLocations);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chklistAllowedLocations);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chklistAllowedTypes);
            this.Controls.Add(this.chkMultiSelect);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chklistDefaultTypes);
            this.Controls.Add(this.btnInvoke);
            this.Controls.Add(this.lblFeedback);
            this.Name = "MainForm";
            this.Text = "Directory Object Picker Tester";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		private void btnInvoke_Click(object sender, System.EventArgs e)
		{
			try
			{
                ObjectTypes allowedTypes = ObjectTypes.None;
                foreach (ObjectTypes value in chklistAllowedTypes.CheckedItems)
                {
                    allowedTypes |= value;
                }
                ObjectTypes defaultTypes = ObjectTypes.None;
                foreach (ObjectTypes value in chklistDefaultTypes.CheckedItems)
                {
                    defaultTypes |= value;
                }
                Locations allowedLocations = Locations.None;
                foreach (Locations value in chklistAllowedLocations.CheckedItems)
                {
                    allowedLocations |= value;
                }
                Locations defaultLocations = Locations.None;
                foreach (Locations value in chklistDefaultLocations.CheckedItems)
                {
                    defaultLocations |= value;
                }
                // Show dialog
                DirectoryObjectPickerDialog picker = new DirectoryObjectPickerDialog();
                picker.AllowedObjectTypes = allowedTypes;
                picker.DefaultObjectTypes = defaultTypes;
                picker.AllowedLocations = allowedLocations;
                picker.DefaultLocations = defaultLocations;
                picker.MultiSelect = chkMultiSelect.Checked;
                picker.TargetComputer = txtTargetComputer.Text;
                DialogResult dialogResult = picker.ShowDialog(this);
                if (dialogResult == DialogResult.OK)
                {
                    DirectoryObject[] results;
                    results = picker.SelectedObjects;
                    if (results == null)
                    {
                        lblFeedback.Text = "Results null.";
                        return;
                    }

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();

                    for (int i = 0; i <= results.Length - 1; i++)
                    {
                        sb.Append(string.Format("Name: \t\t {0}", results[i].Name));
                        sb.Append(Environment.NewLine);
                        sb.Append(string.Format("UPN: \t\t {0}", results[i].Upn));
                        sb.Append(Environment.NewLine);
                        sb.Append(string.Format("Path: \t\t {0}", results[i].Path));
                        sb.Append(Environment.NewLine);
                        sb.Append(string.Format("Schema Class: \t\t {0} ", results[i].SchemaClassName));
                        sb.Append(Environment.NewLine);
                        string downLevelName;
                        try
                        {
                            downLevelName = NameTranslator.TranslateUpnToDownLevel(results[i].Upn);
                        }
                        catch (Exception ex)
                        {
                            downLevelName = string.Format("{0}: {1}", ex.GetType().Name, ex.Message);
                        }
                        sb.Append(string.Format("Down-level Name: \t\t {0} ", downLevelName));
                        sb.Append(Environment.NewLine);
                        sb.Append(Environment.NewLine);
                    }
                    lblFeedback.Text = sb.ToString();
                }
                else
                {
                    lblFeedback.Text = "Dialog result: " + dialogResult.ToString();
                }
			}
			catch(Exception e1)
			{				
				MessageBox.Show(e1.ToString());
			}
		}

        private void Form1_Load(object sender, EventArgs e)
        {
            chklistAllowedTypes.Items.Clear();
            chklistDefaultTypes.Items.Clear();
            foreach(ObjectTypes objectType in Enum.GetValues(typeof(ObjectTypes)))
            {
                if (objectType != ObjectTypes.None && objectType != ObjectTypes.All)
                {
                    chklistAllowedTypes.Items.Add(objectType, CheckState.Checked);
                    if (objectType == ObjectTypes.Users || objectType == ObjectTypes.Groups 
                        || objectType == ObjectTypes.Computers || objectType == ObjectTypes.Contacts)
                    {
                        chklistDefaultTypes.Items.Add(objectType, CheckState.Checked);
                    }
                    else
                    {
                        chklistDefaultTypes.Items.Add(objectType, CheckState.Unchecked);
                    }
                }
            }

            chklistAllowedLocations.Items.Clear();
            chklistDefaultLocations.Items.Clear();
            foreach (Locations location in Enum.GetValues(typeof(Locations)))
            {
                if (location != Locations.None && location != Locations.All)
                {
                    chklistAllowedLocations.Items.Add(location, CheckState.Checked);
                    if (location == Locations.JoinedDomain)
                    {
                        chklistDefaultLocations.Items.Add(location, CheckState.Checked);
                    }
                    else
                    {
                        chklistDefaultLocations.Items.Add(location, CheckState.Unchecked);
                    }
                }
            }
        }

	}
}
