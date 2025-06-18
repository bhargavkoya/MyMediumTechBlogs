using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoftwarePrinciplesInDotNET.SOLID
{
    //CORRECT: Segregated Interfaces
    //The solution is to break the fat interface into smaller, focused interfaces
    public interface IPrinter
    {
        void Print(string document);
    }

    public interface IScanner
    {
        void Scan(string document);
    }

    public interface IFaxMachine
    {
        void Fax(string document);
    }

    public interface ICopier
    {
        void Copy(string document);
    }

    public interface IEmailSender
    {
        void Email(string document);
    }

    public interface ICloudUploader
    {
        void CloudUpload(string document);
    }

    // Now classes implement only what they need
    public class BasicPrinter : IPrinter
    {
        public void Print(string document)
        {
            Console.WriteLine($"Printing: {document}");
        }
    }

    public class AdvancedPrinter : IPrinter, IScanner, ICopier
    {
        public void Print(string document)
        {
            Console.WriteLine($"Advanced printing: {document}");
        }

        public void Scan(string document)
        {
            Console.WriteLine($"Scanning: {document}");
        }

        public void Copy(string document)
        {
            Console.WriteLine($"Copying: {document}");
        }
    }

    public class MultiFunctionDevice : IPrinter, IScanner, IFaxMachine, ICopier, IEmailSender, ICloudUploader
    {
        public void Print(string document)
        {
            Console.WriteLine($"MFD Printing: {document}");
        }

        public void Scan(string document)
        {
            Console.WriteLine($"MFD Scanning: {document}");
        }

        public void Fax(string document)
        {
            Console.WriteLine($"MFD Faxing: {document}");
        }

        public void Copy(string document)
        {
            Console.WriteLine($"MFD Copying: {document}");
        }

        public void Email(string document)
        {
            Console.WriteLine($"MFD Emailing: {document}");
        }

        public void CloudUpload(string document)
        {
            Console.WriteLine($"MFD Uploading to cloud: {document}");
        }
    }
    public class PrinterISP
    {
    }
}
