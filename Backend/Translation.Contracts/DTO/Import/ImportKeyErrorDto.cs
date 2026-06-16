using System;
using System.Collections.Generic;
using System.Text;

namespace Translation.Contracts.DTO.Import
{
    public class ImportKeyErrorDto
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
