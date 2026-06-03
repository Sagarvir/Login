using System;
using System.Collections.Generic;
using System.Text;

namespace TranslationService.DTO.Translation
{
    public class PublishDownloadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public byte[]? FileBytes { get; set; }
    }
}
