using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace silverworker_discord
{
    //replace me with a proper twitch interface
    public class twitchery
    {
        public static async Task twitcherize(string type, string subData)
        {
            string purchaser, action;
            switch (type)
            {
                case "reward-request":
                    await CustomReward(subData);
                    break;
                case "subscription":
                    purchaser = subData.Split("•")[0].Trim();
                    action = subData.Split("•")[1].Trim();
                    var subObj = JsonConvert.SerializeObject(new{
                        purchaserUsername = purchaser,
                        actionData = action
                    }, Formatting.None);
                    if (action == "Subscribed with Prime")
                    {
                        await post("http://192.168.1.151:3001/shortcuts/primeFreshSub", subObj);
                    }
                    else if (action.StartsWith("Resubscribed with Prime."))
                    {
                        await post("http://192.168.1.151:3001/shortcuts/primeResub", subObj);
                    }
                    else if (action.StartsWith("Gifted "))
                    {
                        await post("http://192.168.1.151:3001/shortcuts/giftSub", subObj);
                    }
                    else if (action.StartsWith("Subscribed for"))
                    {
                        await post("http://192.168.1.151:3001/shortcuts/rawSub", subObj);
                    }
                    else if (action.StartsWith("Resubscribed for"))
                    {
                        await post("http://192.168.1.151:3001/shortcuts/rawResub", subObj);
                    }
                    break;
                case "follow":
                    await post("http://192.168.1.151:3001/shortcuts/follow", subData.Split("•")[0].Trim());
                    break;
                case "monetary":
                    await post("http://192.168.1.151:3001/shortcuts/cheer", JsonConvert.SerializeObject(new{
                        purchaserUsername = subData.Split("•")[0].Trim(),
                        actionData = subData.Split("•")[1].Trim()
                    }, Formatting.None));
                    break;
                case "raiding":
                    string partySizeStr = subData.Split("•")[1].Trim().Split(' ').Last();
                    int partySize = -1;
                    int.TryParse(partySizeStr, out partySize);
                    await post("http://192.168.1.151:3001/shortcuts/raid", JsonConvert.SerializeObject(new{
                        raidLeader = subData.Split("•")[0].Trim(),
                        partySize = partySize
                    }, Formatting.None));
                    break;
                default:
                    await UnhandledRedemption(type, subData);
                    break;
            }
        }
        private static async Task CustomReward(string redemptionData)
        {
            var components = redemptionData.Split("•");
            Console.WriteLine($"{components.Length} components:");
            var rewardName = components[0].Trim();
            var redeemer = components[1].Trim();
            var textData = "";
            if (components[1].Contains(":"))
            {
                redeemer = components[1].Substring(0, components[1].IndexOf(":")).Trim();
                textData = components[1].Substring(components[1].IndexOf(":")).Trim();
            }
            Console.WriteLine($"user: {redeemer} redeems {rewardName}, text data? {textData}");

            var redemptionSerialized = JsonConvert.SerializeObject(new
                {
                    redeemer = redeemer,
                    rewardName = rewardName,
                    textData = textData
                }, Formatting.None);
            await post("http://192.168.1.151:3001/shortcuts/redeemReward", redemptionSerialized);
        }
        private static async Task UnhandledRedemption(params string[] data)
        {
            await post("http://192.168.1.151:3001/shortcuts/unhandledRedemption", JsonConvert.SerializeObject(data, Formatting.None)); 
        }
        private static async Task post(string endpoint, string body)
        {
            byte[] sendable = Encoding.ASCII.GetBytes(body);
            var wr = WebRequest.Create(endpoint);
            wr.Method = "POST";
            wr.ContentType = "application/json";
            wr.ContentLength = sendable.Length;
            using (var postStream = wr.GetRequestStream())
            {
                postStream.Write(sendable);
            }
            await wr.GetResponseAsync();
        }
    }
}