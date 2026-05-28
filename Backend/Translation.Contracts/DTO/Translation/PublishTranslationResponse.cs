using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Contracts.DTO.Translation
{
    public class PublishTranslationResponse
    {
        public bool Success { get; set; }

        public string Version { get; set; }

        public int FileCount { get; set; }

        public string Message { get; set; }
    }
}
