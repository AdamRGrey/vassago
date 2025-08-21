namespace vassago.tests;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Newtonsoft.Json;
using static vassago.Models.Enumerations;
using vassago.Controllers.api;
using vassago.Models;
using vassago.ProtocolInterfaces;

public class ExternalProtocolTest
{
    Rememberer r = Rememberer.Instance;
    private static Guid myExternalId= Guid.NewGuid();
    private static ExternalProtocolController contr = new ExternalProtocolController(Substitute.For<ILogger<vassago.Controllers.api.ExternalProtocolController>>());
    private static ProgramConfiguration conf = new ProgramConfiguration();
    [SetUp]
    public void Setup()
    {
        conf = JsonConvert.DeserializeObject<ProgramConfiguration>(File.ReadAllText("appsettings.Development.json"));
        Shared.DBConnectionString = conf.DBConnectionString;
    }
    // [TearDown]
    // public void TearDown()
    // {
    // }

    [Test]
    public async Task Connect()
    {
        ProtocolExternal connectionBody = new ()
        {
            ExternalId = myExternalId.ToString(),
            Style = ExternalProtocolStyle.Restful
        };
        await contr.Connect(connectionBody);
        var ourProtocol = Shared.ProtocolList.FirstOrDefault(p => p is ExternalRestful && (p as ExternalRestful).SelfChannel.ExternalId == myExternalId.ToString());
        Assert.That(ourProtocol, Is.Not.Null);
    }
    // [Test]
    // public void Disconnect()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "Disconnect");
    //     Assert.Fail();
    // }
    // [Test]
    // public void GetCommands()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("GET"), "GetCommands");
    //     Assert.Fail();
    // }
    // [Test]
    // public void GetChannel()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("GET"), "GetChannel");
    //     Assert.Fail();
    // }
    // [Test]
    // public void MessageReceived()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "MessageReceived");
    //     Assert.Fail();
    // }
    // [Test]
    // public void MessageUpdated()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "MessageUpdated");
    //     Assert.Fail();
    // }
    // [Test]
    // public void AccountCreated()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "AccountCreated");
    //     Assert.Fail();
    // }
    // [Test]
    // public void AccountUpdated()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "AccountUpdated");
    //     Assert.Fail();
    // }
    // [Test]
    // public void ChannelCreated()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "ChannelCreated");
    //     Assert.Fail();
    // }
    // [Test]
    // public void ChannelUpdated()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "ChannelUpdated");
    //     Assert.Fail();
    // }
    // [Test]
    // public void ValidateChannel()
    // {
    //     var req = new HttpRequestMessage(new HttpMethod("POST"), "ValidateChannel");
    //     Assert.Fail();
    // }
}
