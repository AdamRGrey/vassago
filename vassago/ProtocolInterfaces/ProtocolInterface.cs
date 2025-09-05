namespace vassago.ProtocolInterfaces;

using Newtonsoft.Json;
using static vassago.Models.Enumerations;

using vassago.Models;

/*
 * these are both in and out - so it's been unclear in my own dang head how to think of them. this file makes it look like it's vassago -> world, one way.
 * but these are both.
 * and essentially, what these do is intake messages, translate them to our format, send them to the Rememberer, then in like 2 places call Behaver.
 * because all that behaver does is get triggered beased on message received.
 *
 * also, protocols don't know what a "user" is. they know what accounts are.
 */
public abstract class ProtocolInterface
{
    protected static Rememberer r = Rememberer.Instance;
    public static string Protocol { get; }
    public abstract Channel SelfChannel { get; }
    public abstract ProtocolConfiguration ConfigurationEntity { get; }
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
    public abstract Task<int> SendMessage(Channel channel, string text);
    public abstract Task<int> React(Message message, string reaction);
    public abstract Task<int> Reply(Message message, string text);
    public abstract Task<int> UpdateConfiguration(ProtocolConfiguration newCfg);
    public abstract Task<int> Die();

    public delegate Task<bool> MessageEvent(Message m);
    public event MessageEvent MessageReceived;
    ///<summary>
    ///children aren't allowed to raise events.
    ///"why, adam?"
    ///yeah, *microsoft*, why?
    ///</summary>
    protected void basedot_MessageReceived(Message m)
    {
        if (MessageReceived != null)
            MessageReceived(m);
    }
    public event MessageEvent MessageUpdated;
    protected void basedot_MessageUpdated(Message m)
    {
        if (MessageUpdated != null)
            MessageUpdated(m);
    }
    public delegate void ChannelEvent(Channel c);
    public event ChannelEvent ChannelJoined;
    protected void basedot_ChannelJoined(Channel c)
    {
        if (ChannelJoined != null)
            ChannelJoined(c);
    }
    public event ChannelEvent ChannelUpdated;
    protected void basedot_ChannelUpdated(Channel c)
    {
        if (ChannelUpdated != null)
            ChannelUpdated(c);
    }
    public delegate void AccountEvent(Account a);
    public event AccountEvent AccountMet;
    protected void basedot_AccountMet(Account a)
    {
        if (AccountMet != null)
            AccountMet(a);
    }
    public event AccountEvent AccountUpdated;
    protected void basedot_AccountUpdated(Account a)
    {
        if (AccountUpdated != null)
            AccountUpdated(a);
    }

}
