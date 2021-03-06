﻿using System.Threading.Tasks;

namespace Helpers.Interfaces
{
	public interface IImportApiHelper
	{
		Task<int> AddDocumentsToWorkspace(int workspaceId, string fileType, int count, string resourceFolderPath);
	}
}
