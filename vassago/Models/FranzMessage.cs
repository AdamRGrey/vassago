using System;
using System.Collections.Generic;
using franz;
using gray_messages;

//psst, future adam: that means we're gray_messages.chat.chat_message.
namespace gray_messages.chat
{
    public class chat_message : gray_messages.message
    {
        public Guid MessageId { get; set; }
        public string Content { get; set; }
        public string RawContent { get; set; }
        public bool MentionsMe { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public uint AttachmentCount { get; set; }

        public Guid AccountId { get; set; }
        public string AccountName { get; set; }

        public Guid UserId { get; set; }
        public string UserName { get; set; }

        public Guid ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string ChannelProtoocl { get; set; }

        public List<Guid> UAC_Matches { get; set; }
        public List<string> BehavedOnBy { get; set; }
    }
}
