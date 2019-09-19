using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
                var ext = Path.GetExtension(path);
                var name = Path.GetFileName(path).Replace(ext, "");
                yield return new SampleFile(name, encoded);
            }
        }

        public void WriteFile(string path, IEnumerable<ExtractedFile> files)
        {
            var rows = files.Select(f => 
            {
                var text = f.Attachment.Content ?? "";
                text = RemoveExcessWhitespace(text);

                return new CsvRowModel
                {
                    Name = f.Name,
                    Content = text
                };
            });

            using (var stream = File.OpenWrite(path))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer))
            {
                csv.WriteRecords(rows);
            }
        }

        private class CsvRowModel
        {
            public string Name { get; set; }
            public string Content { get; set; }
        }

        private static string RemoveExcessWhitespace(string text)
        {
            /*
             * LoRT does this whitespace condensing today after getting text
             * out of Tika. If we don't do this to the results of AWS's Tika,
             * there will be a lot of whitespace diffs in our comparison.
             */
            text = text
                .Replace("\r", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            text = Regex.Replace(text, @"\s+", " ");

            return text;
        }
    }
}
