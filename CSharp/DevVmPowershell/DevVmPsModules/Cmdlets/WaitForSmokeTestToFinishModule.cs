﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Helpers.Implementations;
using Helpers.Interfaces;

namespace DevVmPsModules.Cmdlets
{
	[Cmdlet(VerbsCommon.Watch, "SmokeTestForTestToFinish")]
	public class WaitForSmokeTestToFinishModule : BaseModule
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
			HelpMessage = "Username of the Relativity Sql Account")]
		public string SqlAdminUserName { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 4,
			HelpMessage = "Password of the Relativity Sql Account")]
		public string SqlAdminPassword { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 5,
			HelpMessage = "Name of the Workspace that the Smoke Tests are being run in")]
		public string WorkspaceName { get; set; }

		[Parameter(
			Mandatory = true,
			ValueFromPipelineByPropertyName = true,
			ValueFromPipeline = true,
			Position = 6,
			HelpMessage = "Timeout Value in minutes for the Wait")]
		public string TimeoutValueInMinutes { get; set; }

		protected override void ProcessRecordCode()
		{
			//Validate Input arguments
			ValidateInputArguments();

			IConnectionHelper connectionHelper = new ConnectionHelper(
				relativityInstanceName: RelativityInstanceName,
				relativityAdminUserName: RelativityAdminUserName,
				relativityAdminPassword: RelativityAdminPassword,
				sqlAdminUserName: SqlAdminUserName,
				sqlAdminPassword: SqlAdminPassword);
			ISmokeTestHelper smokeTestHelper = new SmokeTestHelper(connectionHelper);

			int timeoutValueInMinutes = int.Parse(TimeoutValueInMinutes);
			bool testsCompleted = smokeTestHelper.WaitForSmokeTestToComplete(WorkspaceName, timeoutValueInMinutes);
			if (!testsCompleted)
			{
				throw new Exception("Tests did not all complete successfully");
			}
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

			if (string.IsNullOrWhiteSpace(SqlAdminUserName))
			{
				throw new ArgumentNullException(nameof(SqlAdminUserName), $"{nameof(SqlAdminUserName)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(SqlAdminPassword))
			{
				throw new ArgumentNullException(nameof(SqlAdminPassword), $"{nameof(SqlAdminPassword)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(WorkspaceName))
			{
				throw new ArgumentNullException(nameof(WorkspaceName), $"{nameof(WorkspaceName)} cannot be NULL or Empty.");
			}

			if (string.IsNullOrWhiteSpace(TimeoutValueInMinutes))
			{
				throw new ArgumentNullException(nameof(TimeoutValueInMinutes), $"{nameof(TimeoutValueInMinutes)} cannot be NULL, Empty.");
			}
			int timeoutValue;
			if (!int.TryParse(TimeoutValueInMinutes, out timeoutValue))
			{
				throw new ArgumentNullException(nameof(TimeoutValueInMinutes), $"{nameof(TimeoutValueInMinutes)} cannot be an invalid INT.");
			}
		}
	}
}
