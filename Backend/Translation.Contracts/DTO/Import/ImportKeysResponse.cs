using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Contracts.DTO.Import
{
    public class ImportKeysResponse
    {
        public bool Success { get; set; }
        public int TotalRows { get; set; }
        public int InsertedCount { get; set; }
        public int SkippedCount { get; set; }

        public List<string> Warnings { get; set; } = new();
        public List<ImportKeyErrorDto> Errors { get; set; } = new();
    }
}
