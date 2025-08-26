namespace vassago.tests;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Newtonsoft.Json;
using static vassago.Models.Enumerations;
using vassago.Controllers.api;
using vassago.Models;
using vassago.ProtocolInterfaces;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

public class ExternalProtocolTest
{
    Rememberer rememberer = Rememberer.Instance;
    private static string myExternalId = Guid.NewGuid().ToString();
    private static ExternalProtocolController contr = new ExternalProtocolController(Substitute.For<ILogger<vassago.Controllers.api.ExternalProtocolController>>());
    private static ProgramConfiguration conf = new ProgramConfiguration();
    private static bool configured = false;
    [SetUp]
    public void Setup()
    {
        //jump through hoops to get nunit to cooperate (a.k.a. "arrange") - appsettings is specified via the .csproj to get copied from the target project to this test project
        conf = JsonConvert.DeserializeObject<ProgramConfiguration>(File.ReadAllText("appsettings.Development.json"));
        Shared.DBConnectionString = conf.DBConnectionString;
        if(!configured)
        {
            var testconf = new Configuration()
            {
                KafkaBootstrap = "alloces.lan:9092",
                KafkaName = "testforvassago"
            };
            Task.WaitAll(vassago.Reconfigurator.Kafka(testconf));
            configured = true;
        }
        AppDomain.CurrentDomain.UnhandledException += (sender, e) =>{Assert.Fail("Unhandled exception: " + e.ExceptionObject.ToString());};

        //actual arrange
        ProtocolExternal connectionBody = new()
        {
            ExternalId = myExternalId,
            Style = ExternalProtocolStyle.Restful
        };
        var thisisnotanawait = contr.Connect(connectionBody).Result;
        Console.WriteLine(thisisnotanawait);
        var ourProtocol = Shared.ProtocolList.FirstOrDefault(p => p is ExternalRestful && (p as ExternalRestful).SelfChannel.ExternalId == myExternalId) as ExternalRestful;
        Channel usableState = (contr.GetChannel(myExternalId) as OkObjectResult).Value as Channel;
    }
    [TearDown]
    public void TearDown()
    {
        var thisisnotanawait = contr.Disconnect(myExternalId).Result;
    }

    [Test]
    public void Connect()
    {
        //check Setup() for connection parameters
        var ourProtocol = Shared.ProtocolList.FirstOrDefault(p => p is ExternalRestful && (p as ExternalRestful).SelfChannel.ExternalId == myExternalId) as ExternalRestful;
        Assert.That(ourProtocol, Is.Not.Null);
    }
    [Test]
    public async Task Disconnect()
    {
        var ourProtocol = Shared.ProtocolList.FirstOrDefault(p => p is ExternalRestful && (p as ExternalRestful).SelfChannel.ExternalId == myExternalId) as ExternalRestful;
        Assert.That(ourProtocol, Is.Not.Null);

        await contr.Disconnect(myExternalId);

        var protocolConfs = rememberer.ProtocolsOverview();
        var okbutdidyoufindittho = (from conf in protocolConfs where conf.Protocol == "external" && (conf as ProtocolExternal).ExternalId == myExternalId select conf)
            ?.FirstOrDefault();
        Assert.That(okbutdidyoufindittho, Is.Null);
    }
    [Test]
    public void GetChannel()
    {
        Channel usableState = (contr.GetChannel(myExternalId) as OkObjectResult).Value as Channel;
        Assert.That(usableState, Is.Not.Null);
    }
    [Test]
    public async Task MessageReceived()
    {
        var res = await messagein("hello, cruel world", "receivetest", "messagesender");

        var dbMessage = rememberer.SearchMessage(m => m.ChannelId == res.c.Id && m.ExternalId == res.m.ExternalId);
        Assert.That(dbMessage, Is.Not.Null);
        Assert.That(dbMessage.Content, Is.EquivalentTo("hello, cruel world"));

        var dbAccount = rememberer.SearchAccount(a => a.ExternalId == res.a.ExternalId);
        rememberer.ForgetAccount(dbAccount);
        var dbChannel = rememberer.SearchChannel(c => c.ExternalId == res.c.ExternalId);
        rememberer.ForgetChannel(dbChannel);
    }
    // crashes the test host. I guess i'll have to write a test for this test?
    // oh, maybe I'll debug it some other way like with the debugger OH WAIT doesn't work outside of visual studio.
    // maybe i'll use debug statements? OH WAIT nunit *catches* all of that to dump it all at once afterward. moronic.
    // maybe i'll just catch all exceptions, as NUnit seems to be? nope, doesn't help.
    // alright so debugging a test is much harder than just... figuring out other ways to debug code. cool. coolcoolcool.
    // on par for microsoft 🙄
    // [Test]
    // public async Task MessageUpdated()
    // {
    //     var res = await messagein("hello, cruel world", "updatetest", "messageeditor");
    //     var updatedMessage = res.m;
    //     Assert.That(updatedMessage.Content, Is.EquivalentTo("hello, cruel world"));

    //     updatedMessage.Content += " edit: thank you for the gold kind stranger";
    //     var statusCodeResult =
    //         await contr.MessageUpdated(myExternalId,
    //             updatedMessage,
    //             res.a.ExternalId,
    //             res.c.ExternalId) as IStatusCodeActionResult;


    //     var dbMessage = rememberer.SearchMessage(m => m.ChannelId == res.c.Id && m.ExternalId == updatedMessage.ExternalId);
    //     Assert.That(dbMessage.Content, Is.EquivalentTo("hello, cruel world edit: thank you for the gold kind stranger"));

    //     rememberer.ForgetAccount(res.a);
    //     rememberer.ForgetChannel(res.c);
    // }
    [Test]
    public async Task AccountCreated()
    {
        var freshAccount = new Account();
        freshAccount.ExternalId = Guid.NewGuid().ToString();
        freshAccount.DisplayName = "accountcreated test";

        var requestBody = new Tuple<string, Account, string>(
            myExternalId,
            freshAccount,
            myExternalId
        );
        var statusCodeResult =
            await contr.AccountCreated(myExternalId, freshAccount, myExternalId) as IStatusCodeActionResult;

        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(250).Within(50));

        var dbAccount = rememberer.SearchAccount(a => a.ExternalId == freshAccount.ExternalId);
        Assert.That(dbAccount, Is.Not.Null);
        Assert.That(dbAccount.DisplayName, Is.EquivalentTo("accountcreated test"));

        rememberer.ForgetAccount(dbAccount);
    }
    [Test]
    public async Task AccountUpdated()
    {
        var freshAccount = new Account();
        freshAccount.ExternalId = Guid.NewGuid().ToString();
        freshAccount.DisplayName = "accountcreated test";

        var requestBody = new Tuple<string, Account, string>(
            myExternalId,
            freshAccount,
            myExternalId
        );

        await contr.AccountCreated(myExternalId, freshAccount, myExternalId);
        var dice = new Random();
        var scrambledAccount = new Account()
        {
            ExternalId = freshAccount.ExternalId,
            DisplayName = "updated",
            Username = "updated username",
            IsBot = !freshAccount.IsBot
        };
        var statusCodeResult =
            await contr.AccountUpdated(myExternalId, scrambledAccount, myExternalId) as IStatusCodeActionResult;
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(250).Within(50));

        var dbAccount = rememberer.SearchAccount(a => a.ExternalId == freshAccount.ExternalId);
        Assert.That(dbAccount, Is.Not.Null);
        Assert.That(dbAccount.DisplayName, Is.EquivalentTo(scrambledAccount.DisplayName));
        Assert.That(dbAccount.Username, Is.EquivalentTo(scrambledAccount.Username));

        rememberer.ForgetAccount(dbAccount);
    }
    [Test]
    public async Task ChannelCreated()
    {
        var newChannel = new Channel()
        {
            ExternalId = Guid.NewGuid().ToString(),
            DisplayName = "1 level deep"
        };
        var requestBody = new Tuple<string, Channel, string>(
            myExternalId,
            newChannel,
            myExternalId //parent channel ID; as in, we're 1 level down.
        );
        var statusCodeResult =
            await contr.ChannelCreated(requestBody) as IStatusCodeActionResult;
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(250).Within(50));

        var dbChannel = rememberer.SearchChannel(c => c.ExternalId == newChannel.ExternalId);
        rememberer.ForgetChannel(dbChannel);
    }
    [Test]
    public async Task ChannelUpdated()
    {
        var freshChannel = new Channel()
        {
            ExternalId = Guid.NewGuid().ToString(),
            DisplayName = "1 level deep",
            Protocol = "external"
        };
        var requestBody = new Tuple<string, Channel, string>(
            myExternalId,
            freshChannel,
            myExternalId //parent channel ID; as in, we're 1 level down.
        );
        await contr.ChannelCreated(requestBody);

        var dice = new Random();
        var scrambledChannel = new Channel()
        {
            ExternalId = freshChannel.ExternalId,
            DisplayName = "updated",
            MaxAttachmentBytes = (ulong)((freshChannel.MaxAttachmentBytes ?? 10000) * dice.NextDouble()),
            ChannelType = (ChannelType)(Enum.GetValues(typeof(ChannelType)).Cast<int>().Max() * dice.NextDouble()),
            MaxTextChars = (uint)(dice.NextInt64()),
            ReactionsPossible = !freshChannel.ReactionsPossible,
            LinksAllowed = !freshChannel.LinksAllowed,
            LewdnessFilterLevel = (LewdnessFilterLevel)(Enum.GetValues(typeof(LewdnessFilterLevel)).Cast<int>().Max() * dice.NextDouble()),
            MeannessFilterLevel = (MeannessFilterLevel)(Enum.GetValues(typeof(MeannessFilterLevel)).Cast<int>().Max() * dice.NextDouble()),
        };

        requestBody = new Tuple<string, Channel, string>(
            myExternalId,
            scrambledChannel,
            null
        );
        var statusCodeResult =
            await contr.ChannelUpdated(requestBody) as IStatusCodeActionResult;
        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(250).Within(50));

        var dbChannel = rememberer.SearchChannel(c => c.ExternalId == freshChannel.ExternalId);
        Assert.That(dbChannel.DisplayName, Is.EqualTo(scrambledChannel.DisplayName));
        Assert.That(dbChannel.MaxAttachmentBytes, Is.EqualTo(scrambledChannel.MaxAttachmentBytes));
        Assert.That(dbChannel.ChannelType, Is.EqualTo(scrambledChannel.ChannelType));
        Assert.That(dbChannel.MaxTextChars, Is.EqualTo(scrambledChannel.MaxTextChars));
        Assert.That(dbChannel.ReactionsPossible, Is.EqualTo(scrambledChannel.ReactionsPossible));
        Assert.That(dbChannel.LinksAllowed, Is.EqualTo(scrambledChannel.LinksAllowed));
        Assert.That(dbChannel.LewdnessFilterLevel, Is.EqualTo(scrambledChannel.LewdnessFilterLevel));
        Assert.That(dbChannel.MeannessFilterLevel, Is.EqualTo(scrambledChannel.MeannessFilterLevel));

        rememberer.ForgetChannel(dbChannel);
    }
    [Test]
    public void CanGet0Commands()
    {
        var res = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;
        res = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;

        Assert.That(res, Is.Not.Null);
        Assert.That(res.Count, Is.Zero);
    }
    [Test]
    public async Task CanGet1Command()
    {
        var commandListGotten = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;
        commandListGotten = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;
        Assert.That(commandListGotten.Count, Is.Zero);

        var commandMsgResult = await messagein("!pulsecheck", "receivetest", "messagesender");
        commandListGotten = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;

        Assert.That(commandListGotten, Is.Not.Null);
        foreach(var cl in commandListGotten)
        {
            Console.WriteLine($"[test, CanGet1Command] - {JsonConvert.SerializeObject(cl)}");
        }
        Assert.That(commandListGotten.FirstOrDefault(ec => ec.Type == ExternalCommandType.SendMessage && ec.ChannelId == commandMsgResult.c.ExternalId &&
                                                     ec.Text == "[lub-dub]"),
                    Is.Not.Null);
    }
    [Test]
    public async Task CanGetCommandReact()
    {
        var commandListGotten = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;
        commandListGotten = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;
        Assert.That(commandListGotten.Count, Is.Zero);

        var commandMsgResult = await messagein("wish me luck", "receivetest", "reaction fisher");
        commandListGotten = (contr.GetCommands(myExternalId) as OkObjectResult).Value as List<ExternalCommand>;

        Assert.That(commandListGotten, Is.Not.Null);
        foreach(var cl in commandListGotten)
        {
            Console.WriteLine($"[test, CanGetCommandReact] - {JsonConvert.SerializeObject(cl)}");
        }
        var reactCmd = commandListGotten.FirstOrDefault(ec => ec.Type == ExternalCommandType.React && ec.ChannelId == commandMsgResult.c.ExternalId);
        Assert.That(reactCmd, Is.Not.Null);
    }

    private async Task<(Message m, Channel c, Account a)> messagein(string text, string channelName, string authorName)
    {
        var anChannel = new Channel()
        {
            ExternalId = Guid.NewGuid().ToString(),
            DisplayName = channelName,
            ReactionsPossible = true
        };
        await contr.ChannelCreated(new Tuple<string, Channel, string>(myExternalId, anChannel, myExternalId));

        var anAccount = new Account();
        anAccount.ExternalId = Guid.NewGuid().ToString();
        anAccount.Username = authorName;
        await contr.AccountCreated(myExternalId, anAccount, anChannel.ExternalId);

        var aMessage = new Message()
        {
            ExternalId = Guid.NewGuid().ToString(),
            Content = text
        };
        var statusCodeResult =
            await contr.MessageReceived(myExternalId,
                                  aMessage,
                                  anAccount.ExternalId,
                                  anChannel.ExternalId) as IStatusCodeActionResult;

        Assert.That(statusCodeResult.StatusCode, Is.EqualTo(250).Within(50));
        var dbMessage = rememberer.SearchMessage(m => m.ChannelId == anChannel.Id && m.ExternalId == aMessage.ExternalId);
        var dbAccount = rememberer.SearchAccount(a => a.ExternalId == anAccount.ExternalId);
        var dbChannel = rememberer.SearchChannel(c => c.ExternalId == anChannel.ExternalId);
        return new() { m = dbMessage, c = dbChannel, a = dbAccount };
    }
}
