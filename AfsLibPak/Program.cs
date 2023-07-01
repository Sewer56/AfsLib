using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AFSLib;

namespace AfsLibPak;

class Program
{
    static void Main(string[] args)
    {
        var fullPath = Path.GetFullPath(args[0]);
        if (!Directory.Exists(fullPath))
            UnpackAfs(fullPath);
        else
            PackAfs(fullPath);
    }

    private static void UnpackAfs(string fullPath)
    {
        var data = File.ReadAllBytes(fullPath);
        if (AfsArchive.TryFromFile(data, out var afsViewer))
        {
            var outPath = $"{Path.GetDirectoryName(fullPath)}/{Path.GetFileNameWithoutExtension(fullPath)}";
            Directory.CreateDirectory(outPath);

            // Do stuff.
            for (int x = 0; x < afsViewer.Files.Count; x++)
            {
                var filePath = $"{outPath}/{x}_";
                var file = afsViewer.Files[x];
                if (!string.IsNullOrEmpty(file.Name))
                    filePath += file.Name;

                File.WriteAllBytes(filePath, afsViewer.Files[x].Data);
            }
        }
    }

    private static void PackAfs(string fullPath)
    {
        var files = Directory.GetFiles(fullPath);
        var archive = new AfsArchive();
        var getFileIndexRegex = new Regex(@"\d+");
        var removeFileStartRegex = new Regex(@"\d+_");
        
        foreach (var file in files.OrderBy(x => int.Parse(getFileIndexRegex.Match(Path.GetFileNameWithoutExtension(x)).Value)))
            archive.Files.Add(new AFSLib.Structs.File(removeFileStartRegex.Replace(Path.GetFileName(file), ""), File.ReadAllBytes(file)));

        File.WriteAllBytes($"{fullPath}.afs", archive.ToBytes());
    }
}