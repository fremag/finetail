namespace FineTail;

public interface IFineTailView 
{
    bool Pause { get; set; }
    
    void Init(TextFileModel model);
    void Update();
    void OnKey(ConsoleKeyInfo key);
    void Stop();
}