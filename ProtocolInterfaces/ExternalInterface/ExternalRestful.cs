namespace vassago.ProtocolInterfaces;
using vassago.Models;
using vassago.ProtocolInterfaces;
using static vassago.Models.Enumerations;

/* ok so. internal interfaces are started and stopped.
 * external interfaces *may* not be running. we connect and disconnect. They may also permanently die.
 * what if they perma-die while vassago is down?
 * vassago up, connect
 * vassago up, disconnect
 * external up, connect
 * external up, disconnect
 *
 * one "external" "protcol" is one host. so if I put the whole damn thing in a greasemoney script, that might be 1 protocol.
 * but if I need to switch to Go for a matrix client (grumble grumble), that would be 1 proto that may have many channels.
 */

public class ExternalRestful : ProtocolInterface
{
    private Channel protocolAsChannel;
    public override Channel SelfChannel { get => protocolAsChannel; }
    private ProtocolExternal confEntity;
    public override ProtocolConfiguration ConfigurationEntity { get => confEntity; }
    public static new string Protocol { get => "external"; }
    public List<ExternalCommand> CommandQueue { get; set; }

    public async Task Init(ProtocolExternal cfg)
    {
        confEntity = cfg;
    }
    public override async Task<int> SendMessage(Channel channel, string text)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.SendMessage,
            ChannelId = channel.Id,
            Text = text
        });
        return 200;
    }
    public override async Task<int> SendFile(Channel channel, string base64dData, string filename, string accompanyingText)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.SendFile,
            ChannelId = channel.Id,
            Text = accompanyingText,
            FileData = base64dData,
            FileName = filename
        });
        return 200;
    }
    public override async Task<int> React(Message message, string reaction)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.React,
            MessageId = message.Id,
            Text = reaction
        });
        return 200;
    }
    public override async Task<int> Reply(Message message, string text)
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.Reply,
            MessageId = message.Id,
            Text = text
        });
        return 200;
    }
    public override async Task<int> UpdateConfiguration(ProtocolConfiguration newCfg)
    {
        throw new NotImplementedException();
        // TODO
        // CommandQueue.Add(new ExternalCommand()
        // {
        //     Type = ExternalCommandType.ConfigurationUpdated,
        // });
        // return 200;

        // var newConfEntity = newCfg as ProtocolExternal;
        // if (newConfEntity != null)
        // {
        //     Console.WriteLine("External Interface was able to cast incoming configuration to its type of configuration");
        //     if (newConfEntity.Style != confEntity.Style)
        //         throw new NotImplementedException();
        //     //return 205;
        // }
        // else
        // {
        //     Die();
        //     Console.WriteLine("External Interface was not able to cast incoming configuration to its type of configuration");
        //     return 422;
        // }
    }
    public override async Task<int> Die()
    {
        CommandQueue.Add(new ExternalCommand()
        {
            Type = ExternalCommandType.Die
        });
        return 200;
    }
}
public class ExternalCommand
{
    public ExternalCommandType Type { get; set; }
    public Guid ChannelId { get; set; }
    public Guid MessageId { get; set; }
    ///<summary>
    ///or for reactions, reaction. for files, this will be the accompanying text.
    ///</summary>
    public string Text { get; set; }
    public string FileName { get; set; }
    public string FileData { get; set; }
}
