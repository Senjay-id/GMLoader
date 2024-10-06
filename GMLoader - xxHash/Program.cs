using Standart.Hash.xxHash;

public class GMLoaderProgram
{
    static void Main()
    {
        try
        {
            string dataWin = "data.win";
            string backupWin = "backup.win";
            string dataWinAlt = "..\\data.win";
            string backupWinAlt = "..\\backup.win";
            ulong fileHash = 0;

            if (File.Exists(dataWin))
            {
                fileHash = ComputeFileHash3(dataWin);
                Console.WriteLine($"Hash Value of {Path.GetFullPath(dataWin)} using xxHash3 algorithm is: {fileHash}");
            }
            if (File.Exists(backupWin))
            {
                fileHash = ComputeFileHash3(backupWin);
                Console.WriteLine($"Hash Value of {Path.GetFullPath(backupWin)} using xxHash3 algorithm is: {fileHash}");
            }

            if (File.Exists(dataWinAlt))
            {
                fileHash = ComputeFileHash3(dataWinAlt);
                Console.WriteLine($"Hash Value of {Path.GetFullPath(dataWinAlt)} using xxHash3 algorithm is: {fileHash}");
            }
            if (File.Exists(backupWinAlt))
            {
                fileHash = ComputeFileHash3(backupWinAlt);
                Console.WriteLine($"Hash Value of {Path.GetFullPath(backupWinAlt)} using xxHash3 algorithm is: {fileHash}");
            }

            if (!File.Exists(dataWin) && !File.Exists(backupWin) && !File.Exists(dataWinAlt) && !File.Exists(dataWinAlt))
            {
                Console.WriteLine("Couldn't find data.win or backup.win");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        // Wait for the user to press a key before closing the application
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    static ulong ComputeFileHash64(string filePath)
    {
        using (FileStream fileStream = File.OpenRead(filePath))
        {
            return xxHash64.ComputeHash(fileStream);
        }
    }

    static ulong ComputeFileHash3(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            byte[] fileBytes = new byte[stream.Length];
            stream.Read(fileBytes, 0, (int)stream.Length);
            return xxHash3.ComputeHash(fileBytes, (int)stream.Length, 0);
        }
    }
}
