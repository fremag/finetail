
using FineTail;

namespace FineTailTests;

public class ReverseTextReaderTests
{
    private string path;
    private TextFileReader reader;

    [SetUp]
    public void Setup()
    {
    }

    [TearDown]
    public void TearDown()
    {
        if(File.Exists(path))
        {
            File.Delete(path);
        }
    }
    
    [Test]
    public void ReadNextLineTest()
    {
        Init("AA", "BB");
        
        reader.Top();
        var line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("AA");
        
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("BB");
    }

    private void Init(params string[] lines)
    {
        Init(lines, 1024);
    }

    private void Init(string[] lines, int bufferSize = 0)
    {
        path = Path.GetTempFileName();
        reader = new TextFileReader(path, bufferSize);
        File.WriteAllLines(path, lines);
    }

    private void InitText(string text)
    {
        path = Path.GetTempFileName();
        reader = new TextFileReader(path);
        File.WriteAllText(path, text);
    }
        

    [Test]
    public void ReadNextLine_EndFileTest()
    {
        Init("AA", "BB");
        reader.Bottom();
        var line = reader.ReadNextLine();
        Check.That(line).IsNull();
    }
    
    [Test]
    public void ReadPreviousLineTest()
    {
        Init("AA", "BB", "CCC");

        reader.Bottom();
        var line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("CCC");

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("BB");
    }    
    
    [Test]
    public void ReadPreviousLine_TopTest()
    {
        Init("AA", "BB", "CCC");

        reader.Top();
        var line = reader.ReadPreviousLine();
        Check.That(line).IsNull();
    }

    [Test]
    public void ReadPreviousLine_NoFinalCRLF_Test()
    {
        InitText(string.Join(Environment.NewLine, "AA", "BB", "CCC"));

        reader.Bottom();
        var line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("CC");
    }

    [Test]
    public void EmptyFile_Test()
    {
        Init();
        
        reader.Bottom();
        var line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(null);

        reader.Top();
        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(null);

        reader.Bottom();
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(null);
    }
    
    [Test]
    public void ReadPreviousLine_EmptyLines_Test()
    {
        Init("", "A", "", "B", "", "C", "");
        reader.Bottom();
        var line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(string.Empty);

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("C");

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(string.Empty);

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("B");
        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(string.Empty);

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("A");

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(string.Empty);
        
        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(null);
    }
    
    [Test]
    public void ReadNextLine_EmptyLines_Test()
    {
        Init("", "A", "", "B", "", "C", "");
        reader.Top();
        var line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(string.Empty);
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("A");

        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(string.Empty);

        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("B");

        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(string.Empty);
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("C");

        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(string.Empty);
        
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(null);
    }

    [Test]
    public void ReadNextPrev_Test()
    {
        Init("A", "B");
        reader.Top();
        var line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("A");
        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("A");
    }

    [Test]
    public void ReadPrevNext_Test()
    {
        Init("A", "B");
        reader.Bottom();
        var line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("B");
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("B");
    }

    [Test]
    public void ReadNextLine_Previous_Test()
    {
        Init("", "A", "", "B", "", "C", "");
        reader.Top();
        var line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(string.Empty);
        
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("A");

        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo(string.Empty);

        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("B");

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("B");
        
        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo(string.Empty);

        line = reader.ReadPreviousLine();
        Check.That(line).IsEqualTo("A");
        
        line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("A");
    }

    [Test]
    public void PositionTest()
    {
        Init("A", "B");
        reader.Top();
        Check.That(reader.Position).IsEqualTo(-1);

        reader.ReadPreviousLine();
        Check.That(reader.Position).IsEqualTo(-1);
        
        reader.ReadNextLine();
        Check.That(reader.Position).IsEqualTo(2);

        reader.Bottom();
        Check.That(reader.Position).IsEqualTo(5);

        reader.ReadNextLine();
        Check.That(reader.Position).IsEqualTo(5);

        reader.ReadPreviousLine();
        Check.That(reader.Position).IsEqualTo(2);
    }

    [Test]
    public void DeleteFileTest()
    {
        Init(new []{"A", "B", "C", "D"}, 1);
        reader.Top();
        var line = reader.ReadNextLine();
        Check.That(line).IsEqualTo("A");
        File.Delete(path);
        
        for (int i = 0; i < 10; i++)
        {
           line = reader.ReadNextLine();
        }
    }
}