﻿using Helpers.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using Relativity.Services.ServiceProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Helpers.Implementations
{
	public class WorkspaceHelper : IWorkspaceHelper
	{
		private ServiceFactory ServiceFactory { get; }
		public ISqlHelper SqlHelper { get; set; }

		public WorkspaceHelper(IConnectionHelper connectionHelper, ISqlHelper sqlHelper)
		{
			ServiceFactory = connectionHelper.GetServiceFactory();
			SqlHelper = sqlHelper;
		}

		public async Task<int> CreateSingleWorkspaceAsync(string workspaceTemplateName, string workspaceName, bool enableDataGrid)
		{
			// Query for the RelativityOne Quick Start Template
			List<int> workspaceArtifactIds = await WorkspaceQueryAsync(workspaceTemplateName);
			if (workspaceArtifactIds.Count == 0)
			{
				throw new Exception($"Template workspace doesn't exist [Name: {workspaceTemplateName}]");
			}
			if (workspaceArtifactIds.Count > 1)
			{
				throw new Exception($"Multiple Template workspaces exist with the same name [Name: {workspaceTemplateName}]");
			}

			int templateWorkspaceArtifactId = workspaceArtifactIds.First();

			// Create the workspace 
			int workspaceArtifactId = await CreateWorkspaceAsync(templateWorkspaceArtifactId, workspaceName, enableDataGrid);
			return workspaceArtifactId;
		}

		public async Task DeleteAllWorkspacesAsync(string workspaceName)
		{
			List<int> workspaceArtifactIds = await WorkspaceQueryAsync(workspaceName);
			if (workspaceArtifactIds.Count > 0)
			{
				Console.WriteLine("Deleting all Workspaces");
				foreach (int workspaceArtifactId in workspaceArtifactIds)
				{
					await DeleteSingleWorkspaceAsync(workspaceArtifactId);
				}
				Console.WriteLine("Deleted all Workspaces!");
			}
		}

		public async Task DeleteSingleWorkspaceAsync(int workspaceArtifactId)
		{
			Console.WriteLine("Deleting Workspace");

			try
			{
				using (IRSAPIClient rsapiClient = ServiceFactory.CreateProxy<IRSAPIClient>())
				{
					await Task.Run(() => rsapiClient.Repositories.Workspace.DeleteSingle(workspaceArtifactId));

					Console.WriteLine("Deleted Workspace!");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when deleting Workspace", ex);
			}
		}

		private async Task<int> CreateWorkspaceAsync(int templateWorkspaceArtifactId, string workspaceName, bool enableDataGrid)
		{
			Console.WriteLine("Creating new Workspace");

			try
			{
				const string workspaceCreationFailErrorMessage = "Failed to create new workspace";

				using (IRSAPIClient rsapiClient = ServiceFactory.CreateProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = Constants.EDDS_WORKSPACE_ARTIFACT_ID;

					//Create the workspace object and apply any desired properties.		
					Workspace newWorkspace = CreateWorkspaceDto(workspaceName);

					ProcessOperationResult processOperationResult = await Task.Run(() => rsapiClient.Repositories.Workspace.CreateAsync(templateWorkspaceArtifactId, newWorkspace));

					if (!processOperationResult.Success)
					{
						throw new Exception(workspaceCreationFailErrorMessage);
					}

					ProcessInformation processInformation = await Task.Run(() => rsapiClient.GetProcessState(rsapiClient.APIOptions, processOperationResult.ProcessID));

					const int maxTimeInMilliseconds = Constants.Waiting.MAX_WAIT_TIME_IN_MINUTES * 60 * 1000;
					const int sleepTimeInMilliSeconds = Constants.Waiting.SLEEP_TIME_IN_SECONDS * 1000;
					int currentWaitTimeInMilliseconds = 0;

					while ((currentWaitTimeInMilliseconds < maxTimeInMilliseconds) && (processInformation.State != ProcessStateValue.Completed))
					{
						Thread.Sleep(sleepTimeInMilliSeconds);

						processInformation = await Task.Run(() => rsapiClient.GetProcessState(rsapiClient.APIOptions, processOperationResult.ProcessID));

						currentWaitTimeInMilliseconds += sleepTimeInMilliSeconds;
					}

					int? workspaceArtifactId = processInformation.OperationArtifactIDs.FirstOrDefault();
					if (workspaceArtifactId == null)
					{
						throw new Exception(workspaceCreationFailErrorMessage);
					}

					Console.WriteLine($"Workspace ArtifactId: {workspaceArtifactId.Value}");
					Console.WriteLine("Created new Workspace!");

					if (enableDataGrid)
					{
						Console.WriteLine("Updating workspace to be Data Grid Enabled");

						//Update Workspace to be Data Grid Enabled
						Workspace workspace = rsapiClient.Repositories.Workspace.ReadSingle(workspaceArtifactId.Value);
						workspace.EnableDataGrid = true;
						rsapiClient.Repositories.Workspace.UpdateSingle(workspace);

						//Enable Data Grid on Extracted Text field
						SqlHelper.EnableDataGridOnExtractedText(Constants.Connection.Sql.EDDS_DATABASE, workspaceName);

						Console.WriteLine("Workspace updated to be Data Grid Enabled");
					}

					return workspaceArtifactId.Value;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when creating Workspace", ex);
			}
		}

		private static Workspace CreateWorkspaceDto(string workspaceName)
		{
			Workspace newWorkspace = new Workspace
			{
				Name = workspaceName,
				Accessible = true
			};
			return newWorkspace;
		}

		private async Task<List<int>> WorkspaceQueryAsync(string workspaceName)
		{
			Console.WriteLine($"Querying for Workspaces [Name: {workspaceName}]");

			try
			{
				using (IRSAPIClient rsapiClient = ServiceFactory.CreateProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = Constants.EDDS_WORKSPACE_ARTIFACT_ID;

					TextCondition textCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, workspaceName);
					Query<Workspace> workspaceQuery = new Query<Workspace>
					{
						Fields = FieldValue.AllFields,
						Condition = textCondition
					};

					QueryResultSet<Workspace> workspaceQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Workspace.Query(workspaceQuery));

					if (!workspaceQueryResultSet.Success || workspaceQueryResultSet.Results == null)
					{
						throw new Exception("Failed to query Workspaces");
					}

					List<int> workspaceArtifactIds = workspaceQueryResultSet.Results.Select(x => x.Artifact.ArtifactID).ToList();

					Console.WriteLine($"Queried for Workspaces! [Count: {workspaceArtifactIds.Count}]");

					return workspaceArtifactIds;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when querying Workspaces", ex);
			}
		}

		public async Task<int> GetWorkspaceCountQueryAsync(string workspaceName)
		{
			Console.WriteLine($"Querying for Workspace Name: {workspaceName}");

			try
			{
				using (IRSAPIClient rsapiClient = ServiceFactory.CreateProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = Constants.EDDS_WORKSPACE_ARTIFACT_ID;

					TextCondition textCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, workspaceName);
					Query<Workspace> workspaceQuery = new Query<Workspace>
					{
						Fields = FieldValue.NoFields,
						Condition = textCondition
					};

					QueryResultSet<Workspace> workspaceQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Workspace.Query(workspaceQuery));

					if (!workspaceQueryResultSet.Success || workspaceQueryResultSet.Results == null)
					{
						throw new Exception("Failed to query Workspaces");
					}

					List<int> workspaceArtifactIds = workspaceQueryResultSet.Results.Select(x => x.Artifact.ArtifactID).ToList();

					int workspaceCount = workspaceArtifactIds.Count;
					Console.WriteLine($"Queried for Workspaces! [Count: {workspaceCount}]");

					return workspaceCount;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when querying for Workspace count", ex);
			}
		}

		public async Task<int> GetFirstWorkspaceArtifactIdQueryAsync(string workspaceName)
		{
			Console.WriteLine($"Querying for Workspace Name: {workspaceName}");

			try
			{
				using (IRSAPIClient rsapiClient = ServiceFactory.CreateProxy<IRSAPIClient>())
				{
					rsapiClient.APIOptions.WorkspaceID = Constants.EDDS_WORKSPACE_ARTIFACT_ID;

					TextCondition textCondition = new TextCondition(WorkspaceFieldNames.Name, TextConditionEnum.EqualTo, workspaceName);
					Query<Workspace> workspaceQuery = new Query<Workspace>
					{
						Fields = FieldValue.NoFields,
						Condition = textCondition
					};

					QueryResultSet<Workspace> workspaceQueryResultSet = await Task.Run(() => rsapiClient.Repositories.Workspace.Query(workspaceQuery));

					if (!workspaceQueryResultSet.Success || workspaceQueryResultSet.Results == null)
					{
						throw new Exception("Failed to query Workspaces");
					}

					List<int> workspaceArtifactIds = workspaceQueryResultSet.Results.Select(x => x.Artifact.ArtifactID).ToList();

					Console.WriteLine($"Queried for Workspaces! [Count: {workspaceArtifactIds.Count}]");

					if (workspaceArtifactIds.Count == 0)
					{
						throw new Exception($"No workspace exists! [{nameof(workspaceName)}: {workspaceName}]");
					}

					int firstWorkspaceArtifactId = workspaceArtifactIds.FirstOrDefault();
					return firstWorkspaceArtifactId;
				}
			}
			catch (Exception ex)
			{
				throw new Exception("An error occured when querying Workspaces", ex);
			}
		}
	}
}