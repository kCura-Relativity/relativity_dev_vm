﻿using Helpers.Interfaces;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using Relativity.Services.ServiceProxy;
using System;
using System.Linq;
using System.Net.Http;
using Helpers.RequestModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Helpers.Implementations
{
	public class InstanceSettingsHelper : IInstanceSettingsHelper
	{
		private ServiceFactory ServiceFactory { get; }
		private string InstanceAddress { get; set; }
		private string AdminUsername { get; set; }
		private string AdminPassword { get; set; }

		public InstanceSettingsHelper(IConnectionHelper connectionHelper, string instanceAddress, string adminUsername, string adminPassword)
		{
			ServiceFactory = connectionHelper.GetServiceFactory();
			InstanceAddress = instanceAddress;
			AdminUsername = adminUsername;
			AdminPassword = adminPassword;
		}

		public int CreateInstanceSetting(string name, string section, string description, string value)
		{
			try
			{
				HttpClient httpClient = RestHelper.GetHttpClient(InstanceAddress, AdminUsername, AdminPassword);
				InstanceSettingManagerCreateRequest instanceSettingManagerUpdateRequest = new InstanceSettingManagerCreateRequest
				{
					instanceSetting = new RequestModels.instanceSetting
					{
						Name = name,
						Section = section,
						Machine = "",
						ValueType = "Text",
						Value = value,
						InitialValue = value,
						Encrypted = false,
						Description = description,
						Keywords = "",
						Notes = ""
					}
				};
				string createRequest = JsonConvert.SerializeObject(instanceSettingManagerUpdateRequest);
				HttpResponseMessage createResponse = RestHelper.MakePostAsync(httpClient, Constants.Connection.RestUrlEndpoints.InstanceSettings.endpointUrl, createRequest).Result;
				if (!createResponse.IsSuccessStatusCode)
				{
					throw new Exception("Failed to create Instance Setting");
				}

				Console.WriteLine("Successfully Created Instance Setting");
				string result = createResponse.Content.ReadAsStringAsync().Result;
				int instanceSettingArtifactId = int.Parse(result);
				return instanceSettingArtifactId;
			}
			catch (Exception ex)
			{
				throw new Exception("Error Creating Instance Setting", ex);
			}
		}

		public bool UpdateInstanceSettingValue(string name, string section, string newValue)
		{
			try
			{
				if (newValue == null)
				{
					newValue = string.Empty;
				}

				HttpClient httpClient = RestHelper.GetHttpClient(InstanceAddress, AdminUsername, AdminPassword);
				ObjectManagerQueryRequestModel objectManagerQueryRequestModel = new ObjectManagerQueryRequestModel
				{
					request = new Request
					{
						objectType = new Helpers.RequestModels.ObjectType
						{
							Name = Constants.InstanceSetting.INSTANCE_SETTING_OBJECT_TYPE
						},
						fields = new object[]
						{
							new
							{
								Name = "Name"
							},
							new
							{
								Name = "Value"
							}
						},
						condition = $"'Section' == '{section}' AND 'Name' == '{name}'"
					},
					start = 1,
					length = 10
				};

				string queryRequest = JsonConvert.SerializeObject(objectManagerQueryRequestModel);
				HttpResponseMessage queryResponse = RestHelper.MakePostAsync(httpClient,
					Constants.Connection.RestUrlEndpoints.ObjectManager.queryUrl, queryRequest).Result;
				if (!queryResponse.IsSuccessStatusCode)
				{
					throw new Exception("Failed to Query for Agent Artifact Ids");
				}
				string result = queryResponse.Content.ReadAsStringAsync().Result;
				JObject jObject = JObject.Parse(result);
				int totalCount = jObject["TotalCount"].Value<int>();
				if (totalCount > 0)
				{
					int instanceSettingArtifactId = jObject["Objects"][0]["ArtifactID"].Value<int>();
					string instanceSettingName = jObject["Objects"][0]["FieldValues"][0]["Value"].Value<string>();
					if (instanceSettingName == "ESIndexCreationSettings")
					{
						string instanceSettingValue = jObject["Objects"][0]["FieldValues"][1]["Value"].Value<string>();
						newValue = instanceSettingValue.Replace("\"number_of_shards\": 12", "\"number_of_shards\": 2");
						newValue = newValue.Replace("\"number_of_replicas\": 2", "\"number_of_replicas\": 0");
					}
					InstanceSettingManagerUpdateRequest instanceSettingManagerUpdateRequest = new InstanceSettingManagerUpdateRequest
					{
						instanceSetting = new RequestModels.InstanceSetting
						{
							ArtifactId = instanceSettingArtifactId,
							Value = newValue
						}
					};
					string updateRequest = JsonConvert.SerializeObject(instanceSettingManagerUpdateRequest);
					HttpResponseMessage updateResponse = RestHelper.MakePutAsync(httpClient, Constants.Connection.RestUrlEndpoints.InstanceSettings.endpointUrl, updateRequest).Result;
					if (updateResponse.IsSuccessStatusCode)
					{
						Console.WriteLine("Successfully updated the Instance Setting");
					}
					else
					{
						throw new Exception("Failed to Update Instance Setting.");
					}
					return updateResponse.IsSuccessStatusCode;
				}

				Console.WriteLine("Failed to find the existing Instance Setting");
				return false;
			}
			catch (Exception ex)
			{
				throw new Exception("Error Updating Instance Setting", ex);
			}
		}

		public void DeleteInstanceSetting(int instanceSettingArtifactId)
		{
			try
			{
				HttpClient httpClient = RestHelper.GetHttpClient(InstanceAddress, AdminUsername, AdminPassword);
				string deleteUrl = $"Relativity.REST/api/Relativity.InstanceSettings/workspace/{Constants.EDDS_WORKSPACE_ARTIFACT_ID}/instancesettings/{instanceSettingArtifactId}";
				HttpResponseMessage httpResponseMessage = RestHelper.MakeDeleteAsync(httpClient, deleteUrl).Result;
				if (!httpResponseMessage.IsSuccessStatusCode)
				{
					throw new Exception("Failed to Delete Instance Setting");
				}
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to Delete Instance Setting", ex);
			}
		}

		public string GetInstanceSettingValue(string name, string section)
		{
			try
			{
				string retVal = null;
				using (IInstanceSettingManager instanceSettingManager = ServiceFactory.CreateProxy<IInstanceSettingManager>())
				{
					Query query = new Query();
					query.Condition = $"'Section' == '{section}' AND 'Name' == '{name}'";
					InstanceSettingQueryResultSet instanceSettingQueryResultSet = instanceSettingManager.QueryAsync(query).Result;
					if (instanceSettingQueryResultSet.Success)
					{
						if (instanceSettingQueryResultSet.TotalCount == 0)
						{
							return null;
						}
						Relativity.Services.Result<Relativity.Services.InstanceSetting.InstanceSetting> result = instanceSettingQueryResultSet.Results.First();
						if (result.Artifact != null)
						{
							retVal = result.Artifact.Value;
						}
					}
				}

				return retVal;
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to Get Instance Setting Value", ex);
			}
		}

		public int GetInstanceSettingArtifactIdByName(string name, string section)
		{
			try
			{
				int artifactId = 0;
				using (IInstanceSettingManager instanceSettingManager = ServiceFactory.CreateProxy<IInstanceSettingManager>())
				{
					Query query = new Query();
					query.Condition = $"'Section' == '{section}' AND 'Name' == '{name}'";
					InstanceSettingQueryResultSet instanceSettingQueryResultSet = instanceSettingManager.QueryAsync(query).Result;
					if (instanceSettingQueryResultSet.Success)
					{
						if (instanceSettingQueryResultSet.TotalCount == 0)
						{
							return 0;
						}
						Relativity.Services.Result<Relativity.Services.InstanceSetting.InstanceSetting> result = instanceSettingQueryResultSet.Results.First();
						if (result.Artifact != null)
						{
							artifactId = result.Artifact.ArtifactID;
						}
					}
				}

				return artifactId;
			}
			catch (Exception ex)
			{
				throw new Exception("Failed to Get Instance Setting Value", ex);
			}
		}
	}
}
