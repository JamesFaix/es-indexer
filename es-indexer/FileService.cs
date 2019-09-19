using System;
using System.Collections.Generic;
using System.IO;

namespace PowerDms.EsIndexer
{
    public class FileService
    {
        public IEnumerable<SampleFile> EnumerableSampleFiles(string directory)
        {
            var filePaths = Directory.EnumerateFiles(directory);

            foreach (var path in filePaths)
            {
                var bytes = File.ReadAllBytes(path);
                var encoded = Convert.ToBase64String(bytes);
                var name = Path.GetFileName(path);
                yield return new SampleFile(name, encoded);
            }
        }

    }
}
