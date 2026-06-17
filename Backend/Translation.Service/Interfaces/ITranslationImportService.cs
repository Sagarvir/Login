using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;
using Translation.Contracts.DTO.Import;

namespace Translation.Service.Interfaces
{
    public interface ITranslationImportService
    {
        Task<ImportKeysResponse> ImportKeysAsync(IFormFile file, string empId, int projectId);
    }
}
