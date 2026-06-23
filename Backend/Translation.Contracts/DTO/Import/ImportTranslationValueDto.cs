using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Contracts.DTO.Import
{
    public class ImportTranslationValueDto
    {
        public string? KeyName { get; set; }
        public string? OriginalText { get; set; }
        public string? Translation { get; set; }
        public string? FileLanguageCode { get; set; }
    }
}
