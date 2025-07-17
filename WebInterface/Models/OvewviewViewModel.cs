namespace vassago.WebInterface.Models;

using vassago.Models;
public class OverviewViewModel
{
    public List<ProtocolConfiguration> Protocols { get; set; }
    public List<Account> Accounts { get; set; }
    public List<Channel> Channels { get; set; }
    public List<Webhook> Webhooks { get; set; }
    public List<UAC> UACs { get; set; }
    public List<User> Users { get; set; }
}
