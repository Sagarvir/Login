
using Translation.DAO.Data;
using Translation.Models.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Translation.Contracts.DTO.Import;
using Translation.Service.Interfaces;
using System.Xml.Linq;
using Humanizer.Localisation.DateToOrdinalWords;

namespace Translation.Service.Services
{
    public class TranslationImportService : ITranslationImportService
    {
        private readonly AppDbContext _context;

        public TranslationImportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportKeysResponse> ImportKeysAsync(IFormFile file, string empId, int projectId)
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

            if (projectId <= 0)
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "Valid ProjectId is required."
                });
                return response;
            }

            var extension = Path.GetExtension(file.FileName).ToLower();

            if (extension != ".json" && extension != ".xlf" && extension != ".xliff")
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "Only JSON, XLF, and XLIFF files are supported."
                });
                return response;
            }

            List<ImportKeyDto>? items;

            try
            {
                if (extension == ".json")
                {
                    using var stream = file.OpenReadStream();

                    items = await JsonSerializer.DeserializeAsync<List<ImportKeyDto>>(
                        stream,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });
                }
                else
                {
                    items = await ParseXliffFileAsync(file, projectId);
                }
            }
            catch
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = extension == ".json"
                        ? "Invalid JSON file format."
                        : "Invalid XLIFF file format."
                });
                return response;
            }

            if (items == null || items.Count == 0)
            {
                response.Errors.Add(new ImportKeyErrorDto
                {
                    RowNumber = 0,
                    Message = "File does not contain any importable keys."
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

                normalizedItems.Add(new ImportKeyDto
                {
                    ExternalKeyId = row.ExternalKeyId,

                    // IMPORTANT:
                    // Do not uppercase XLIFF-style keys.
                    KeyName = row.KeyName.Trim(),

                    OriginalText = row.OriginalText.Trim(),
                    ProjectId = projectId
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

        private async Task<List<ImportKeyDto>> ParseXliffFileAsync(IFormFile file, int projectId)
        {
            using var stream = file.OpenReadStream();

            var document = await XDocument.LoadAsync(
                stream,
                LoadOptions.None,
                CancellationToken.None);

            var transUnits = document
                .Descendants()
                .Where(x => x.Name.LocalName == "trans-unit")
                .ToList();

            var items = new List<ImportKeyDto>();

            foreach (var unit in transUnits)
            {
                var keyName = unit.Attribute("id")?.Value;

                var source = unit.Elements()
                    .FirstOrDefault(x => x.Name.LocalName == "source")
                    ?.Value;

                items.Add(new ImportKeyDto
                {
                    KeyName = keyName,
                    OriginalText = source,
                    ProjectId = projectId
                });
            }

            return items;
        }

    }
}