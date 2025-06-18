using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.RealWorldUsecases
{
    //Document processing interfaces segregated by responsibility
    public interface IDocumentReader
    {
        string ReadDocument(string filePath);
        bool CanRead(string fileExtension);
    }

    public interface IDocumentWriter
    {
        void WriteDocument(string content, string filePath);
        bool CanWrite(string fileExtension);
    }

    public interface IDocumentConverter
    {
        string ConvertDocument(string content, string fromFormat, string toFormat);
        List<string> SupportedConversions { get; }
    }

    public interface IDocumentCompressor
    {
        byte[] CompressDocument(string content);
        string DecompressDocument(byte[] compressedData);
    }

    public interface IDocumentEncryptor
    {
        string EncryptDocument(string content, string key);
        string DecryptDocument(string encryptedContent, string key);
    }

    public interface IDocumentValidator
    {
        bool ValidateDocument(string content);
        List<string> GetValidationErrors(string content);
    }

    //PDF processor implements only needed interfaces
    public class PdfDocumentProcessor : IDocumentReader, IDocumentWriter, IDocumentConverter
    {
        public string ReadDocument(string filePath)
        {
            Console.WriteLine($"Reading PDF document: {filePath}");
            return "PDF content";
        }

        public bool CanRead(string fileExtension)
        {
            return fileExtension.ToLower() == ".pdf";
        }

        public void WriteDocument(string content, string filePath)
        {
            Console.WriteLine($"Writing PDF document: {filePath}");
        }

        public bool CanWrite(string fileExtension)
        {
            return fileExtension.ToLower() == ".pdf";
        }

        public string ConvertDocument(string content, string fromFormat, string toFormat)
        {
            Console.WriteLine($"Converting from {fromFormat} to {toFormat}");
            return $"Converted content to {toFormat}";
        }

        public List<string> SupportedConversions => new List<string> { "pdf-to-txt", "txt-to-pdf" };
    }

    //Text processor with different capabilities
    public class TextDocumentProcessor : IDocumentReader, IDocumentWriter, IDocumentValidator
    {
        public string ReadDocument(string filePath)
        {
            Console.WriteLine($"Reading text document: {filePath}");
            return File.ReadAllText(filePath);
        }

        public bool CanRead(string fileExtension)
        {
            return new[] { ".txt", ".md", ".log" }.Contains(fileExtension.ToLower());
        }

        public void WriteDocument(string content, string filePath)
        {
            Console.WriteLine($"Writing text document: {filePath}");
            File.WriteAllText(filePath, content);
        }

        public bool CanWrite(string fileExtension)
        {
            return new[] { ".txt", ".md", ".log" }.Contains(fileExtension.ToLower());
        }

        public bool ValidateDocument(string content)
        {
            return !string.IsNullOrWhiteSpace(content) && content.Length < 1000000;
        }

        public List<string> GetValidationErrors(string content)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(content))
                errors.Add("Content cannot be empty");
            if (content.Length >= 1000000)
                errors.Add("Content exceeds maximum length");
            return errors;
        }
    }

    //Secure document processor with encryption capabilities
    public class SecureDocumentProcessor : IDocumentReader, IDocumentWriter, IDocumentEncryptor, IDocumentCompressor
    {
        public string ReadDocument(string filePath)
        {
            var encryptedContent = File.ReadAllText(filePath);
            return DecryptDocument(encryptedContent, "default-key");
        }

        public bool CanRead(string fileExtension)
        {
            return fileExtension.ToLower() == ".secure";
        }

        public void WriteDocument(string content, string filePath)
        {
            var encryptedContent = EncryptDocument(content, "default-key");
            File.WriteAllText(filePath, encryptedContent);
        }

        public bool CanWrite(string fileExtension)
        {
            return fileExtension.ToLower() == ".secure";
        }

        public string EncryptDocument(string content, string key)
        {
            Console.WriteLine("Encrypting document content");
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(content));
        }

        public string DecryptDocument(string encryptedContent, string key)
        {
            Console.WriteLine("Decrypting document content");
            return Encoding.UTF8.GetString(Convert.FromBase64String(encryptedContent));
        }

        public byte[] CompressDocument(string content)
        {
            Console.WriteLine("Compressing document");
            return Encoding.UTF8.GetBytes(content);
        }

        public string DecompressDocument(byte[] compressedData)
        {
            Console.WriteLine("Decompressing document");
            return Encoding.UTF8.GetString(compressedData);
        }
    }

    //Document service that works with any processor type
    public class DocumentService
    {
        public void ProcessDocuments(IDocumentReader reader, IDocumentWriter writer, string inputPath, string outputPath)
        {
            if (reader.CanRead(Path.GetExtension(inputPath)) && writer.CanWrite(Path.GetExtension(outputPath)))
            {
                var content = reader.ReadDocument(inputPath);
                writer.WriteDocument(content, outputPath);
            }
        }

        public void ValidateDocument(IDocumentValidator validator, string content)
        {
            if (!validator.ValidateDocument(content))
            {
                var errors = validator.GetValidationErrors(content);
                Console.WriteLine($"Validation failed: {string.Join(", ", errors)}");
            }
        }
    }
}
