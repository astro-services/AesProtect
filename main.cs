using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        // Set the path of the executable file to protect
        string exePath = "MyProgram.exe";

        // Set the password to use for the ZIP archive
        string password = "myPassword";

        // Set the output path for the protected executable file
        string outputPath = "MyProtectedProgram.exe";

        // Compress the executable file using a password-protected ZIP archive
        using (var zipStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
            {
                // Add the executable file to the ZIP archive
                var entry = archive.CreateEntry(Path.GetFileName(exePath));
                using (var stream = entry.Open())
                {
                    using (var fileStream = File.OpenRead(exePath))
                    {
                        fileStream.CopyTo(stream);
                    }
                }
            }

            // Encrypt the ZIP archive with the password
            byte[] zipData = zipStream.ToArray();
            byte[] salt = new byte[32];
            new RNGCryptoServiceProvider().GetBytes(salt);
            byte[] key = new Rfc2898DeriveBytes(Encoding.UTF8.GetBytes(password), salt, 10000).GetBytes(32);
            byte[] iv = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(iv);
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (var cipherStream = new MemoryStream())
                {
                    using (var cryptoStream = new CryptoStream(cipherStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(zipData, 0, zipData.Length);
                        cryptoStream.FlushFinalBlock();

                        // Write the protected executable file
                        byte[] protectedData = cipherStream.ToArray();
                        using (var fileStream = File.Create(outputPath))
                        {
                            fileStream.Write(salt, 0, salt.Length);
                            fileStream.Write(iv, 0, iv.Length);
                            fileStream.Write(protectedData, 0, protectedData.Length);
                        }
                    }
                }
            }
        }

        Console.WriteLine("Executable file protected and saved to: " + outputPath);
    }
}
