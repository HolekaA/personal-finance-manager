using System.IO;

namespace osobniSpravceFinanci.Services
{
    public static class DatabaseContext
    {
        public static string DbPath { get; } = Path.Combine(FileSystem.AppDataDirectory, "spravceFinanci.db");
    }
}