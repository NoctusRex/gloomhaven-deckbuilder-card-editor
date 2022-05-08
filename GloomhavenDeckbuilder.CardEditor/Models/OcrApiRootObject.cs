using System.Collections.Generic;

namespace GloomhavenDeckbuilder.CardEditor.Models
{
    public class RootObject
    {
        public List<ParsedResult> ParsedResults { get; set; } = new();
        public int OCRExitCode { get; set; }
        public bool IsErroredOnProcessing { get; set; }
        public List<string> ErrorMessage { get; set; } = new();
        public string ErrorDetails { get; set; } = string.Empty;
    }

    public class ParsedResult
    {
        public object FileParseExitCode { get; set; } = string.Empty;
        public string ParsedText { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ErrorDetails { get; set; } = string.Empty;
    }

}
