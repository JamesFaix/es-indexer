namespace PowerDms.EsIndexer
{
    /// <summary>
    /// Represents a source file on disk after have content base64 encoded
    /// </summary>
    public class SampleFile
    {
        /// <summary>
        /// Document name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Base64 encoded contents
        /// </summary>
        public string Data { get; }

        public SampleFile(string name, string data)
        {
            Name = name;
            Data = data;
        }
    }

    /// <summary>
    /// Represents a file queried from ES after extraction
    /// </summary>
    public class ExtractedFile
    {
        public string Name { get; set; }

        public ExtractedAttachment Attachment { get; set; }
    }

    /// <summary>
    /// Represents the text extraction output attached to the document in ES
    /// </summary>
    public class ExtractedAttachment
    {
        /// <summary>
        /// Extracted text content of document
        /// </summary>
        public string Content { get; set; }
    }
}
