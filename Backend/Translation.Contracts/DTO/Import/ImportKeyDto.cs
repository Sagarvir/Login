using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Contracts.DTO.Import
{
    public class ImportKeyDto
    {
        public int? ExternalKeyId { get; set; }
        public string? KeyName { get; set; }
        public string? OriginalText { get; set; }
        public int? ProjectId { get; set; }
        public string? Translation { get; set; }
        public string? LanguageCode { get; set; }
    }
}
