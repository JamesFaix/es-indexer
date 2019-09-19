namespace PowerDms.EsIndexer
{
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
}
