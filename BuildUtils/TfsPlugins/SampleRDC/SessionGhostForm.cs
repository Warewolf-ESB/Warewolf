using System;
using System.IO;
using System.Windows.Forms;
using MSTSCLib;

namespace SampleRDC
{
    public partial class SessionGhost : Form
    {
        private string _svr;
        private string _user;
        private string _pass;

        public SessionGhost()
        {
            const int cnt = 4;

            string[] args = Environment.CommandLine.Split(' ');

            File.WriteAllText("data.log", Environment.CommandLine + " " + args.Length);

            if (args.Length == cnt)
            {
                _svr = args[1];
                _user = args[2];
                _pass = args[3];
            }


            InitializeComponent();

            // connect if args ;)
            if (args.Length == cnt)
            {
                //txtServer.Text = _svr;
                //txtUserName.Text = _user;
                //txtPassword.Text = _pass;

                Connect(_svr, _user, _pass);
            }
        }

        private void Connect(string server, string user, string pass)
        {
            try
            {
                rdp.Server = server;
                rdp.UserName = user;

                IMsTscNonScriptable secured = (IMsTscNonScriptable)rdp.GetOcx();
                secured.ClearTextPassword = pass;
                rdp.Connect();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(@"Error Connecting", @"Error connecting to remote desktop " + txtServer.Text + @" Error:  " + Ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                Connect(txtServer.Text,txtUserName.Text, txtPassword.Text);
            }
            catch (Exception Ex)
            {
                MessageBox.Show(@"Error Connecting", @"Error connecting to remote desktop " + txtServer.Text + @" Error:  " + Ex.Message,MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if connected before disconnecting
                if (rdp.Connected.ToString() == "1")
                    rdp.Disconnect();
            }
            catch (Exception Ex)
            {
                MessageBox.Show(@"Error Disconnecting", @"Error disconnecting from remote desktop " + txtServer.Text + @" Error:  " + Ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}