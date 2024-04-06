# discord-bot

copy appsettings.json to appsettings.ENV.json and fill it in. dotnet seems to understand files called appsettings.json (and appsettings.xml?) and knows how to overwrite *specific values found within* the .[ENV].[extension] version

# auth link

https://discord.com/oauth2/authorize?client_id=913003037348491264&permissions=274877942784&scope=bot
that's read messages/view channels, send messages, send messages in threads, and attach files. but not add reactions?

# concepts

## Data Types

### Accounts

a `User` can have multiple `Account`s. e.g., @adam:greyn.club? that's an "account". I, however, am a `User`. An `Account` has references to the `Channels` its seen in.

### Attachment

debating whether to save a copy of every single attachment. Discord allows 25MB attachments, and shtikbot lives in several art channels.

### Channel

a place where communication can happen. any level of these can have any number of children. In matrix, everything is a "room" - even spaces and threads. Seems like a fine idea. So for vassago, a discord "channel" is a channel. a "thread" is a child of that channel. a "category" is a parent of that channel. A "server" (formerly "guild") is a parent of that channel. and fuck it, Discord itself is a "channel". Includes permissions vassago has for a channel; MaxAttachmentBytes, etc. go down the hierarchy until you find an override.

### FeaturePermission

the permissions of a feature. It can be restricted to accounts, to users, to channels. It has an internal name... and tag? and it can be (or not be) inheritable?

### Message

a message (duh). features bools for "mentions me", the external ID, the reference to the account, the channel.

### User

a person or program who operates an account. recognizing that 2 `Account`s belong to 1 `User` can be done by that user (using LinkMe). I should be able to collapse myself automatically.

## Behavior

both a "feature" and an "anti-feature". a channel might dictate something isn't allowed (lewdness in a g-rated channel). A person might not be allowed to do something - lots of me-only things like directing other bots (and the now rendered-moot Torrent feature). A behavior might need a command alias in a particular channel (freedomunits in jubel's)

so "behavior" might need to tag other data types? do I have it do a full select every time we get a message? ...no, only if the (other) triggering conditions are met. Then you can take your time.