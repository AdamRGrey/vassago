namespace vassago.ProtocolInterfaces;

using vassago.Models;

public abstract class ProtocolInterface
{
    public static string Protocol { get; }
    public abstract Channel SelfChannel { get; }
    public abstract ProtocolConfiguration ConfigurationEntity { get; }
    public abstract Task<int> SendMessage(Channel channel, string text);
    public virtual async Task<int> SendFile(Channel channel, string path, string accompanyingText)
    {
        if (!File.Exists(path))
        {
            return 404;
        }
        var fstring = Convert.ToBase64String(File.ReadAllBytes(path));
        return await SendFile(channel, fstring, Path.GetFileName(path), accompanyingText);
    }
    public abstract Task<int> SendFile(Channel channel, string base64dData, string filename, string accompanyingText);
    public abstract Task<int> React(Message message, string reaction);
    public abstract Task<int> Reply(Message message, string text);
    public abstract Task<int> UpdateConfiguration(ProtocolConfiguration newCfg);
    public abstract Task<int> Die();
}
