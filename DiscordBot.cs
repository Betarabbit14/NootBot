using Discord;
using Discord.Audio;
using Discord.Commands;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace ConsoleApp1
{
    public class DiscordBot
    {
        DiscordClient client;
        CommandService commands;

        public DiscordBot()
        {
//========//constructor
            client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

//========//client command prefix creation setup
            client.UsingCommands(input =>
            {
                input.PrefixChar = '>';
                input.HelpMode = HelpMode.Public;
                input.AllowMentionPrefix = true;
            });

//========//client audio service creation setup
            client.UsingAudio(input =>
            {
                input.Mode = AudioMode.Outgoing;
            });

            commands = client.GetService<CommandService>();

            //========//reply commands section
            commands.CreateCommand("kys")
                .Description("Be mean to the bot and make her feel bad.")
                .Do(async e =>
                {
                    await e.Channel.SendMessage("<:dude:297615426672918538> :gun: gotchu");
                });

            client.GetService<CommandService>().CreateCommand("greet") //create command greet
                .Alias(new string[] { "gr", "hi" }) //add 2 aliases, so it can be run with >gr and >hi
                .Description("Greets a person.") //add description, it will be shown when >help is used
                .Parameter("GreetedPerson", ParameterType.Required) //as an argument, we have a person we want to greet
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{e.User.Name} greets {e.GetArg("GreetedPerson")}");
                    //sends a message to channel with the given text
                });

//========//audio commands
            commands.CreateCommand("pls").Do(async (e) =>
            {
                await e.Channel.SendMessage("Playing: Beep Beep Sheep.mp3");
                var vChannel = e.Server.VoiceChannels.FirstOrDefault(); // Finds the first VoiceChannel on the server 'SMIC'
                var vClient = await client.GetService<AudioService>()
                        .Join(vChannel);
                var channelCount = client.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
                var OutFormat = new WaveFormat(48000, 16, channelCount); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
                using (var MP3Reader = new Mp3FileReader("a.mp3")) // Create a new Disposable MP3FileReader, to read audio from the filePath parameter
                using (var resampler = new MediaFoundationResampler(MP3Reader, OutFormat)) // Create a Disposable Resampler, which will convert the read MP3 data to PCM, using our Output Format
                {
                    resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                    int blockSize = OutFormat.AverageBytesPerSecond / 50; // Establish the size of our AudioBuffer
                    byte[] buffer = new byte[blockSize];
                    int byteCount;

                    while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0) // Read audio into our buffer, and keep a loop open while data is present
                    {
                        if (byteCount < blockSize)
                        {
                            // Incomplete Frame
                            for (int i = byteCount; i < blockSize; i++)
                                buffer[i] = 0;
                        }
                        vClient.Send(buffer, 0, blockSize); // Send the buffer to Discord
                    }
                }
            });

//========//event actions or announcements
            client.UserJoined += async (s, e) =>
            {
                var channel = e.Server.FindChannels("general",ChannelType.Text).FirstOrDefault();
                var user = e.User;
                await channel.SendMessage(String.Format("Ayy, {0} has joined the channel!", user.Name));
            };

            client.UserLeft += async (s, e) =>
            {
                var channel = e.Server.FindChannels("general", ChannelType.Text).FirstOrDefault();
                var user = e.User;
                await channel.SendMessage(String.Format("RIP, {0} has left the channel.", user.Name));
            };



            client.ExecuteAndWait(async () =>
            {
                //do NOT LEAK MY TOKEN I WILL KILL U
                await client.Connect("MY_TOKEN", TokenType.Bot);
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        public void SendAudio(string filePath)
        {
            

        }

    }
}
