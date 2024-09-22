using System.Text;
using CrypticWizard.RandomWordGenerator;

namespace FileGenerator;

public static class FileGenerator
{
    private static readonly WordGenerator WordGenerator = new();
    private const string OutputFile = "output.txt";

    private static int RandomNumber(Random rand) => rand.Next(1, 100000);

    private static string RandomString(Random rand)
    {
        var numberOfWords = rand.Next(1, 500);
        return string.Join(" ", WordGenerator.GetWords(WordGenerator.PartOfSpeech.noun, numberOfWords));
    }
    
    private static void GenerateFile(long sizeInBytes)
    {
        var random = new Random();
        long writtenBytes = 0;

        using var writer = new StreamWriter(OutputFile);
        while (writtenBytes < sizeInBytes)
        {
            var number = RandomNumber(random);
            var text = RandomString(random);
            var line = $"{number}. {text}";

            writer.WriteLine(line);
            writtenBytes = CalculateWrittenBytes(writtenBytes, line);
            writtenBytes = AddRepeatedLines(random, line, text, writer, writtenBytes);
        }
    }

    private static long AddRepeatedLines(Random random, string line, string text, StreamWriter writer, long writtenBytes)
    {
        if (random.Next(100) < 10)  // 10% probability
        {
            line = $"{RandomNumber(random)}. {text}";
            writer.WriteLine(line);
            writtenBytes = CalculateWrittenBytes(writtenBytes, line);
        }

        if (random.Next(100) < 5)  // 5% probability
        {
            writer.WriteLine(line);
            writtenBytes = CalculateWrittenBytes(writtenBytes, line);
        }

        return writtenBytes;
    }

    private static long CalculateWrittenBytes(long writtenBytes, string line)
    {
        return writtenBytes + Encoding.UTF8.GetByteCount(line + Environment.NewLine);
    }

    private static void Main(string[] args)
    {
        if (args.Length != 1)
        {
            Console.WriteLine("Usage: <size_in_bytes>");
            return;
        }

        if (!long.TryParse(args[0], out var sizeInBytes))
        {
            Console.WriteLine($"Couldn't parse number - '{args[0]}'.");
            return;
        }
        
        GenerateFile(sizeInBytes);
    }
}