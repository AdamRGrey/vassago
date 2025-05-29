namespace vassago.ProtocolInterfaces;

using vassago.Models;

public abstract class ProtocolInterface
{
    public static string Protocol { get; }
    public abstract Channel SelfChannel { get; }
    public abstract Task<int> SendMessage(Channel channel, string text);
    public abstract Task<int> SendFile(Channel channel, string path, string accompanyingText);
    public abstract Task<int> React(Message message, string reaction);
    public abstract Task<int> Reply(Message message, string text);
}
