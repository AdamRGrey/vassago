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
    private Channel channelState;
    [SetUp]
    public void Setup()
    {
        //jump through hoops to get nunit to cooperate (a.k.a. "arrange") - appsettings is specified via the .csproj to get copied from the target project to this test project
        conf = JsonConvert.DeserializeObject<ProgramConfiguration>(File.ReadAllText("appsettings.Development.json"));
        Shared.DBConnectionString = conf.DBConnectionString;

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
    // [Test]
    // public void GetCommands()
    // {
    //     Assert.Fail();
    // }
    [Test]
    public void GetChannel()
    {
        Channel usableState = (contr.GetChannel(myExternalId) as OkObjectResult).Value as Channel;
        Assert.That(usableState, Is.Not.Null);
    }
    // [Test]
    // public void MessageReceived()
    // {
    //     Assert.Fail();
    // }
    // [Test]
    // public void MessageUpdated()
    // {
    //     Assert.Fail();
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
}
