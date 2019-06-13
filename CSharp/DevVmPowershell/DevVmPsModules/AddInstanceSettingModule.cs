﻿using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace DevVmPsModules
{
	[Cmdlet(VerbsCommon.Add, "InstanceSetting")]
	public class AddInstanceSettingModule : BaseModule
	{
		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 0,
			HelpMessage = "Name of the Relativity Instance")]
		public string RelativityInstanceName { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 1,
			HelpMessage = "Username of the Relativity Admin")]
		public string RelativityAdminUserName { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 2,
			HelpMessage = "Password of the Relativity Admin")]
		public string RelativityAdminPassword { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 3,
			HelpMessage = "Name of the Instance Setting")]
		public string Name { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 4,
			HelpMessage = "Section of the Instance Setting")]
		public string Section { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 5,
			HelpMessage = "Description of the Instance Setting")]
		public string Description { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 6,
			HelpMessage = "Value of the Instance Setting")]
		public string Value { get; set; }

		protected override void ProcessRecordCode()
		{
			//Validate Input arguments
			ValidateInputArguments();

			IConnectionHelper connectionHelper = new ConnectionHelper(RelativityInstanceName, RelativityAdminUserName, RelativityAdminPassword);
			IInstanceSettingsHelper instanceSettingsHelper = new InstanceSettingsHelper(connectionHelper);

			// Create Instance Setting
			instanceSettingsHelper.CreateInstanceSetting(Name, Section, Description, Value);
		}

		private void ValidateInputArguments()
		{
			if (string.IsNullOrWhiteSpace(RelativityInstanceName))
			{
				throw new ArgumentNullException(nameof(RelativityInstanceName), $"{nameof(RelativityInstanceName)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(RelativityAdminUserName))
			{
				throw new ArgumentNullException(nameof(RelativityAdminUserName), $"{nameof(RelativityAdminUserName)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(RelativityAdminPassword))
			{
				throw new ArgumentNullException(nameof(RelativityAdminPassword), $"{nameof(RelativityAdminPassword)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(Name))
			{
				throw new ArgumentNullException(nameof(Name), $"{nameof(Name)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(Section))
			{
				throw new ArgumentNullException(nameof(Section), $"{nameof(Section)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(Description))
			{
				throw new ArgumentNullException(nameof(Description), $"{nameof(Description)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(Value))
			{
				throw new ArgumentNullException(nameof(Value), $"{nameof(Value)} cannot be NULL or Empty.");
			}
		}
	}
}
