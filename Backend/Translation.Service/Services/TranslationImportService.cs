
using Translation.DAO.Data;
using Translation.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Translation.Contracts.DTO.Import;
using Translation.Service.Interfaces;

namespace Translation.Service.Services
{
    public class TranslationImportService : ITranslationImportService
    {
        private readonly AppDbContext _context;

        public TranslationImportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportKeysResponse> ImportKeysAsync(IFormFile file, string empId)
        {
            var response = new ImportKeysResponse();

            if (file == null || file.Length == 0)
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "File is required."
                });
                return response;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".json")
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "Only JSON files are supported for now."
                });
                return response;
            }

            List<ImportKeyDto>? items;

            try
            {
                using var stream = file.OpenReadStream();

                items = await JsonSerializer.DeserializeAsync<List<ImportKeyDto>>(
                    stream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "Invalid JSON file format."
                });
                return response;
            }

            if (items == null || items.Count == 0)
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "JSON file does not contain any keys."
                });
                return response;
            }

            response.TotalRows = items.Count;

            var normalizedItems = new List<ImportKeyDto>();

            for (int i = 0; i < items.Count; i++)
            {
                var row = items[i];
                var rowNumber = i + 1;

                if (string.IsNullOrWhiteSpace(row.KeyName))
                {
                    response.Warnings.Add($"Row {rowNumber}: KeyName is empty. Skipped.");
                    response.SkippedCount++;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(row.OriginalText))
                {
                    response.Warnings.Add($"Row {rowNumber}: OriginalText is empty. Skipped.");
                    response.SkippedCount++;
                    continue;
                }

                if (row.ProjectId == null || row.ProjectId <= 0)
                {
                    response.Warnings.Add($"Row {rowNumber}: Valid ProjectId is required. Skipped.");
                    response.SkippedCount++;
                    continue;
                }

                normalizedItems.Add(new ImportKeyDto
                {
                    ExternalKeyId = row.ExternalKeyId,

                    // IMPORTANT:
                    // Do not uppercase XLIFF-style keys.
                    KeyName = row.KeyName.Trim(),

                    OriginalText = row.OriginalText.Trim(),
                    ProjectId = row.ProjectId.Value
                });
            }

            if (!normalizedItems.Any())
            {
                response.Success = false;
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "No valid rows found to import."
                });
                return response;
            }

            var duplicateRows = normalizedItems
                .GroupBy(x => x.KeyName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .ToList();

            if (duplicateRows.Any())
            {
                foreach (var duplicate in duplicateRows)
                {
                    response.Warnings.Add(
                        $"Duplicate key found in uploaded file: {duplicate.Key}. Using first occurrence.");
                }

                normalizedItems = normalizedItems
                    .GroupBy(x => x.KeyName, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();
            }

            var uploadedKeyNames = normalizedItems
                .Select(x => x.KeyName!)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var existingKeyNames = await _context.TranslationKeys
                .AsNoTracking()
                .Where(k => uploadedKeyNames.Contains(k.KeyName))
                .Select(k => k.KeyName)
                .ToListAsync();

            var existingKeyNameSet = existingKeyNames
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var item in normalizedItems)
            {
                if (existingKeyNameSet.Contains(item.KeyName!))
                {
                    response.SkippedCount++;
                    response.Warnings.Add($"Skipped existing key: {item.KeyName}");
                    continue;
                }

                var key = new TranslationKey
                {
                    KeyName = item.KeyName!,
                    OriginalText = item.OriginalText!,
                    ProjectId = item.ProjectId!.Value,
                    CreatedByEmpId = empId,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.TranslationKeys.AddAsync(key);
                existingKeyNameSet.Add(item.KeyName!);
                response.InsertedCount++;
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                response.Success = false;
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = $"Database error while importing keys: {ex.InnerException?.Message ?? ex.Message}"
                });

                return response;
            }

            response.Success = response.InsertedCount > 0 || response.SkippedCount > 0;

            return response;
        }
    }
}