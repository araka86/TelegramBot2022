using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using System.Net.Mail;
using System.Text;
using System.Net;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;
using System.Linq;

public class Program
{
    static ITelegramBotClient bot = new TelegramBotClient("5842668948:AAG8kaYQH4xHEk6tykiOmIFFwMyRxdWpdAA");


    static long chatId = 0;
    static string message;
    static int messageId;
    static string firstName;
    static string lastName;
    static Message? sentMessage;
    //block bad words
    static int blockLevel = 0;
    static bool messDeleted = false;
    static string[] badWords = new string[] { "bad word", "badword" };
    static string[] veryBadWords = new string[] { "very bad word", "verybadword" };
    //poll info
    static int pollId = 0;
    public static readonly string[] PollAnswers = { "Good!", "I could be better..", "Bad" };
    static string destinationFilePath = @"C:\data\";


    public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

        if (update.Type == UpdateType.CallbackQuery)
        {
            var buttonText = update.CallbackQuery.Data;
            await botClient.SendTextMessageAsync(update.CallbackQuery.From.Id, $"You click button {buttonText}");
        }


        // Only process Message updates
        if (update.Type != UpdateType.Message)
            return;


        // Only process text messages
        //if (update.Message!.Type != MessageType.Text)
        //    return;

        var messageData = update.Message;
        #region Send_Photo_And_Save_To_Local_Drive(несжимать)
        if (messageData.Document != null)
        {
            var field = update.Message.Document.FileId;
            var fileinfo = await botClient.GetFileAsync(field);
            var getExt = Path.GetExtension(fileinfo.FilePath);
            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath + Guid.NewGuid() + getExt);
            await botClient.DownloadFileAsync(fileinfo.FilePath, fileStream);
            fileStream.Close();
            return;
        }
        #endregion
        #region Send_Photo_From_another_Channel_And_Save_To_Local_Drive(сжимать)
        if (messageData.Photo != null)
        {
            var field = update.Message.Photo.Last().FileId;
            var fileinfo = await botClient.GetFileAsync(field);
            var getExt = Path.GetExtension(fileinfo.FilePath);
            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath + Guid.NewGuid() + getExt);
            await botClient.DownloadFileAsync(fileinfo.FilePath, fileStream);
            fileStream.Close();
            return;
        }
        #endregion
        #region Send_Video_From_another_Channel_And_Save_To_Local_Drive
        if (messageData.Video != null)
        {
            var field = update.Message.Video.FileId;
            var fileinfo = await botClient.GetFileAsync(field);
            var getExt = Path.GetExtension(fileinfo.FilePath);
            var filePath = fileinfo.FilePath;
            destinationFilePath += fileinfo + getExt;
            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);
            fileStream.Close();
            return;
        }
        #endregion



        if (update.Type == UpdateType.Message) //групировать/негрупировать + зжимать
        {
            #region Scope_Photo_And_Document
            if (update.Message.Photo != null)
            {
                if (update.Message.Photo.Count() > 1)
                {
                    var field = update.Message.Photo.Last().FileId;
                    var fileinfo = await botClient.GetFileAsync(field);
                    var getExt = Path.GetExtension(fileinfo.FilePath);
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath + Guid.NewGuid() + getExt);
                    await botClient.DownloadFileAsync(fileinfo.FilePath, fileStream);
                    fileStream.Close();
                }
                return;
            }
            if (update.Message.Document != null) // групировать/негрупировать + несжимать
            {
                if (update.Message.Document.FileId.Count() > 1)
                {
                    var field = update.Message.Document.FileId;
                    var fileinfo = await botClient.GetFileAsync(field);
                    var getExt = Path.GetExtension(fileinfo.FilePath);
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath + Guid.NewGuid() + getExt);
                    await botClient.DownloadFileAsync(fileinfo.FilePath, fileStream);
                    fileStream.Close();
                }
            }
            #endregion

            messageId = update.Message.MessageId;
            chatId = update.Message.Chat.Id;
            message = update.Message.Text;


            #region Block_Vulgarity_Words
            // First of all  blockLevel = 1 is veryBadWords  blockLevel = 2 badWords 
            if (message == "/vulgarity")
            {
                switch (blockLevel)
                {
                    case 0:
                        blockLevel = 1;
                        await botClient.SendTextMessageAsync
                        (
                        chatId: chatId,
                        text: "Vulgarity: \"Medium block\".",
                         cancellationToken: cancellationToken
                        );
                        return;

                    case 1:
                        blockLevel = 2;
                        await botClient.SendTextMessageAsync
                        (
                        chatId: chatId,
                        text: "Vulgarity: \"Hard block\".",
                         cancellationToken: cancellationToken
                        );
                        return;
                    case 2:
                        blockLevel = 0;
                        await botClient.SendTextMessageAsync
                        (
                        chatId: chatId,
                        text: "Vulgarity: \"Block disabled\".",
                         cancellationToken: cancellationToken
                        );
                        return;
                }
            }

            //Vulgarity block list - bad words
            for (int x = 0; x < badWords.Length; x++)
            {

                if (message.Contains(badWords[x]) && blockLevel == 2 && !messDeleted)
                {
                    messDeleted = true;
                    await botClient.DeleteMessageAsync(chatId, messageId);
                    sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: firstName + " " + lastName + " you can't say that things Level 1",
                //replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
                }
            }
            //Vulgarity block list - very bad words
            for (int x = 0; x < veryBadWords.Length; x++)
            {
                if (message.Contains(veryBadWords[x]) && (blockLevel == 1 || blockLevel == 2) && !messDeleted)
                {
                    messDeleted = true;
                    await botClient.DeleteMessageAsync(chatId, messageId);
                    sentMessage = await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: firstName + " " + lastName + " you can't say that things Level 2",
                //replyMarkup: replyKeyboardMarkup,
                cancellationToken: cancellationToken);
                }
            }
            messDeleted = false;
            #endregion
            if (message != null)
            {
                switch (message)
                {
                    #region Start_Channel
                    case "/start":
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $"Добро пожаловать на борт, добрый путник!");
                        break;
                    #endregion
                    #region Send_Message
                    case "hello":
                        sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Hello " + update.Message.Chat.FirstName + "!!!",
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Message_And_Answer_To_Your_Massage+Link
                    case "hi":
                        sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Trying *all the parameters* of `sendMessage` method",
                        parseMode: ParseMode.MarkdownV2,
                        disableNotification: true,
                        replyToMessageId: update.Message.MessageId,
                        replyMarkup: new InlineKeyboardMarkup(
                        InlineKeyboardButton.WithUrl(
                        text: "Check sendMessage method",
                        url: "https://core.telegram.org/bots/api#sendmessage")),
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Gif
                    case "gif":
                        //Use sendAnimation method to send animation files(GIF or H.264 / MPEG - 4 AVC video without sound).
                        Message message6 = await botClient.SendAnimationAsync(
                        chatId: chatId,
                        animation: "https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/video-waves.mp4",
                        caption: "Waves",
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Photo
                    case "photo":
                        sentMessage = await botClient.SendPhotoAsync(
                        chatId: chatId,
                        photo: "https://i.redd.it/uhkj4abc96r61.jpg",
                        caption: "<b>MEME</b>",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Album_Photo
                    case "album":
                        Message[] messages8 = await botClient.SendMediaGroupAsync(
                        chatId: chatId,
                        media: new IAlbumInputMedia[]
                            {
                            new InputMediaPhoto("https://cdn.pixabay.com/photo/2017/06/20/19/22/fuchs-2424369_640.jpg"),
                            new InputMediaPhoto("https://cdn.pixabay.com/photo/2017/04/11/21/34/giraffe-2222908_640.jpg"),
                            },
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Video
                    case "videoCountdown":
                        sentMessage = await botClient.SendVideoAsync(
                        chatId: chatId,
                        video: "https://raw.githubusercontent.com/TelegramBots/book/master/src/docs/video-countdown.mp4",
                        thumb: "https://raw.githubusercontent.com/TelegramBots/book/master/src/2/docs/thumb-clock.jpg",
                        supportsStreaming: true,
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Link_To_YoutobeVideo
                    case "youtobe":
                        sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "https://www.youtube.com/watch?v=ROcups0YaHE&t=5036s",
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Document
                    case "doc":
                        sentMessage = await botClient.SendDocumentAsync(
                        chatId: chatId,
                        document: "https://github.com/TelegramBots/book/raw/master/src/docs/photo-ara.jpg",
                        caption: "<b>Ara bird</b>. <i>Source</i>: <a href=\"https://pixabay.com\">Pixabay</a>",
                        parseMode: ParseMode.Html,
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Sound
                    case "sound":
                        sentMessage = await botClient.SendAudioAsync(
                        chatId: chatId,
                        audio: "https://github.com/TelegramBots/book/raw/master/src/docs/audio-guitar.mp3",
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_PhoneNumber
                    case "phone":
                        sentMessage = await botClient.SendContactAsync(
                        chatId: chatId,
                        phoneNumber: "+1234567890",
                        firstName: "Anna",
                        lastName: "Rossi",
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Location
                    case "loc":
                        sentMessage = await botClient.SendLocationAsync(
                        chatId: chatId,
                        latitude: 41.9027835f,
                        longitude: 12.4963655,
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Location_With_Address
                    case "locAdress":
                        sentMessage = await botClient.SendVenueAsync(
                        chatId: chatId,
                        latitude: 41.9027835f,
                        longitude: 12.4963655,
                        title: "Rome",
                        address: "Rome, via Daqua 8, 08089",
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Start_Pool
                    case "poolstart":
                        //save the poll id message
                        pollId = messageId + 1;
                        Console.WriteLine($"\nPoll number: {pollId}!");
                        sentMessage = await botClient.SendPollAsync(
                        chatId: chatId,
                        question: " This is test boot, How are you?",
                        options: PollAnswers,
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Stop_pool
                    case "poolstop":
                        Console.WriteLine($"\nPoll number {pollId} is close!");
                        Poll poll = await botClient.StopPollAsync(
                        chatId: chatId,
                        messageId: pollId,
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Sticker
                    case "stick":
                        sentMessage = await botClient.SendStickerAsync(
                        chatId: chatId,
                        sticker: "https://tlgrm.ru/_/stickers/80a/5c9/80a5c9f6-a40e-47c6-acc1-44f43acc0862/192/28.webp",
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Button+Text_somethingActions
                    case "button":
                        sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Button " + firstName + " " + lastName + "",
                        replyMarkup: GetButton(chatId),
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_Buttons+Link_to_Channel
                    case "buttons":
                        sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Button " + firstName + " " + lastName + "",
                        replyMarkup: GetButtons(),
                        cancellationToken: cancellationToken);
                        break;
                    #endregion
                    #region Send_email
                    case "sendMail":
                        sentMessage = await botClient.SendTextMessageAsync(
                        chatId: chatId,
                        text: "Hello " + update.Message.Chat.FirstName + "!!!",
                        cancellationToken: cancellationToken);
                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, $" {sendMail(update.Message.Text)}");
                        break;
                    #endregion
                    default:
                        break;
                }
            }
            return;
        }


    }

    public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        // Некоторые действия
        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
    }



    private static async Task Main(string[] args)
    {
        Console.WriteLine("Запущен бот " + bot.GetMeAsync().Result.FirstName);

        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }, // receive all update types
        };
        bot.StartReceiving(HandleUpdateAsync, HandleErrorAsync, receiverOptions, cancellationToken);


        //var sender = new SmtpSender(() => new SmtpClient("araka86@gmail.com")
        //{
        //    EnableSsl = false,
        //    DeliveryMethod = SmtpDeliveryMethod.Network,
        //    Port = 25,
        //    //DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
        //    //PickupDirectoryLocation = "D:\\Udemy\\ASP\\project\\TelegramBot\\mail"
        //});

        //StringBuilder template = new();
        //template.AppendLine("Test @Model.FirstName");
        //template.AppendLine("<p>Thank You!!!</p>");
        //template.AppendLine("Line 3 @Model.ProductNmae");

        //Email.DefaultSender = sender;
        //Email.DefaultRenderer = new RazorRenderer();

        //var email = await Email
        //    .From("araka86@gmail.com")
        //    .To("tremendous1003@gmail.com", "Sue")
        //    .Subject("Subject2")
        //    .UsingTemplate(template.ToString(), new {FirstName = "Tim", ProductNmae= "Prod1" })
        // //   .Body("Body2")
        //    .SendAsync();



        Console.ReadLine();


    }

    public static string sendMail(string text)
    {
        StringBuilder template = new();
        template.AppendLine("Test");
        template.AppendLine("<p>Thank You!!!</p>");
        template.AppendLine($"You send is: {text}");

        using (MailMessage mail = new MailMessage())
        {
            mail.From = new MailAddress("sender@gmail.com");
            mail.To.Add("tremendous1003@gmail.com");
            mail.Subject = "Sub";
            mail.Body = $"{template}";
            mail.IsBodyHtml = true;
            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential("sender@gmail.com", "xxxxxxxxxxxxxxxx"); // email and application password
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }
        }
        return "maill Send!!!";
    }

    #region Buttons
    private static IReplyMarkup GetButtons()
    {
        InlineKeyboardMarkup inlineKeyboard = new(new[]
               {
                    //First row. You can also add multiple rows.
                    new []
                    {
                        InlineKeyboardButton.WithUrl(text: "Pravda Gerachenko", url: "https://t.me/Pravda_Gerashchenko"),
                        InlineKeyboardButton.WithUrl(text: "Ukraine News", url: "https://t.me/+rALH5Vjf9pQyMDMy"),
                    },
                });
        return inlineKeyboard;
    }


    private static IReplyMarkup GetButton(long chatId)
    {
        var inlineKeyboard = new InlineKeyboardMarkup(new[] { new[] { InlineKeyboardButton.WithCallbackData("test", "SomeTextButton") } });
        return inlineKeyboard;
    }
    #endregion
}