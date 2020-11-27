using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace powershellBlazor.Data
{
	public class PowerShellService
	{
		List<string> errors = new List<string>();

		Dictionary<string, List<string>> PowerShellStreams = new Dictionary<string, List<string>>();

		public async Task<Dictionary<string, List<string>>> Run()
		{
			/**********************************
			***** Fill in these variables *****
			**********************************/
			var computerName = ""; //name of computer to perform remote action on
			var userName = "";			   // username to access remote computer
			var password = "";							   // password for user
			
			using var powershell = ConfigurePowerShell(computerName, userName, password);

			var result = await powershell.InvokeAsync();

			var outputStream = result.Select(obj => obj.ToString()).ToList();

			PowerShellStreams.Add("Output Stream", outputStream);
			PowerShellStreams.Add("Error Stream", errors);

			return PowerShellStreams;
		}

		private PowerShell ConfigurePowerShell(string computerName, string userName, string password)
		{
			var path = GetScriptPath("Get-DeployedRqPieces.ps1");

			var username = userName;
			var securePassword = new NetworkCredential("", password).SecurePassword;
			var credential = new PSCredential(username, securePassword);

			var powerShell = PowerShell.Create()
				.AddCommand("Invoke-Command")
				.AddParameter("ComputerName", computerName)
				.AddParameter("ScriptBlock", ScriptBlock.Create("Get-Process"))
				.AddParameter("Credential", credential);

			powerShell.Streams.Error.DataAdded += ErrorDataAdded;

			return powerShell;
		}

		public void ErrorDataAdded(object sender, DataAddedEventArgs e)
		{
			errors.Add(((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString());
		}

		private string GetScriptPath(string fileName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var location = assembly.Location;

			var path = Path.Join(Directory.GetParent(location).FullName, fileName);

			return path;
		}
	}
}
