
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
using Tamir.SharpSsh.jsch;

/* KeyGen.cs
 * ====================================================================
 * The following example was posted with the original JSch java library,
 * and is translated to C# to show the usage of SharpSSH JSch API
 * ====================================================================
 * */
namespace Tamir.SharpSsh.jsch.examples
{
	/// <summary>
	/// This progam will demonstrate the RSA/DSA keypair generation. 
	/// You will be asked a passphrase for output_keyfile.
	/// If everything works fine, you will get the DSA or RSA keypair, 
	/// output_keyfile and output_keyfile+".pub".
	/// The private key and public key are in the OpenSSH format.
	/// </summary>
	public class KeyGen
	{
		public static void Main(params string[] arg)
		{
			if(arg.Length<3)
			{
				Console.Error.WriteLine(
					"usage: java KeyGen rsa output_keyfile comment\n"+
					"       java KeyGen dsa  output_keyfile comment");
				Environment.Exit(-1);
			}

			//Get sig type ('rsa' or 'dsa')
			String _type=arg[0];
			int type=0;
			if(_type.Equals("rsa")){type=KeyPair.RSA;}
			else if(_type.Equals("dsa")){type=KeyPair.DSA;}
			else 
			{
				Console.Error.WriteLine(
					"usage: java KeyGen rsa output_keyfile comment\n"+
					"       java KeyGen dsa  output_keyfile comment");
				Environment.Exit(-1);
			}
			//Output file name
			String filename=arg[1];
			//Signature comment
			String comment=arg[2];

			//Create a new JSch instance
			JSch jsch=new JSch();

			//Prompt the user for a passphrase for the private key file
			String passphrase=InputForm.GetUserInput("Enter passphrase (empty for no passphrase)", true);

			try
			{
				//Generate the new key pair
				KeyPair kpair=KeyPair.genKeyPair(jsch, type);
				//Set a passphrase
				kpair.setPassphrase(passphrase);
				//Write the private key to "filename"
				kpair.writePrivateKey(filename);
				//Write the public key to "filename.pub"
				kpair.writePublicKey(filename+".pub", comment);
				//Print the key fingerprint
				Console.WriteLine("Finger print: "+kpair.getFingerPrint());
				//Free resources
				kpair.dispose();
			}
			catch(Exception e)
			{
				Console.WriteLine(e);
			}
			Environment.Exit(0);
		}
	}
}
