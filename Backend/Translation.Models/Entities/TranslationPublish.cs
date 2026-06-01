using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Models.Entities
{
    // Record of a translation publish event.
    public class TranslationPublish
    {
        public int Id { get; set; }

        public string Version { get; set; }

        public DateTime PublishedAt { get; set; }

        public string PublishedBy { get; set; }

        public int FileCount { get; set; }
    }
}
