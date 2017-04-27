using GitHubClient.Classes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using uwpLibb;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GitHubClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        List<Button> repositories = new List<Button>();
        List<RadioButton> pagesRadio = new List<RadioButton>();
        User user = new User();

        private SocketClient _socketCient;

        StreamSocket socket;

        public MainPage()
        {
            this.InitializeComponent();
            _socketCient = new SocketClient();
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            _socketCient = new SocketClient();
            string host = "api.github.com";
            string port = "443";
            string message = "GET /repos/";
            Task answer = _socketCient.Connect(host, port, message);
            //var dialog = new MessageDialog(answer.ToString());
            //await dialog.ShowAsync();

            HostName hostName;

            //string message = "GET /user/1";
            string getAnswer = "";

            using (socket = new StreamSocket())
            {
                hostName = new HostName(host);

                // Set NoDelay to false so that the Nagle algorithm is not disabled
                socket.Control.NoDelay = false;
                //getAnswer += "sadsd";

                // Connect to the server
                await socket.ConnectAsync(hostName, port, SocketProtectionLevel.Tls12);
                // Send the message
                await this.send(message);
                // Read response
                getAnswer += await this.read();
                //textBox.Text = answer;
            }
            textBox.Text = getAnswer;
            //
            //string name = textBox.Text;
            //string url = "https://github.com/" + name + "?tab=repositories";
            //HttpClient http = new System.Net.Http.HttpClient();
            //HttpResponseMessage response = await http.GetAsync(url);
            //var html = await response.Content.ReadAsStringAsync();
            //if (html.Contains("Page not found"))
            //{
            //    textBox1.Text = "wrong user name";
            //    return;
            //}
            //else this.Frame.Navigate(typeof(RepoDetailsPage), html);
        }


        public async Task send(string message)
        {
            DataWriter writer;

            // Create the data writer object backed by the in-memory stream. 
            using (writer = new DataWriter(socket.OutputStream))
            {
                // Set the Unicode character encoding for the output stream
                writer.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                // Specify the byte order of a stream.
                writer.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                // Gets the size of UTF-8 string.
                writer.MeasureString(message);
                // Write a string value to the output stream.
                writer.WriteString(message);

                // Send the contents of the writer to the backing stream.
                try
                {
                    await writer.StoreAsync();
                }
                catch (Exception exception)
                {
                    switch (SocketError.GetStatus(exception.HResult))
                    {
                        case SocketErrorStatus.HostNotFound:
                            // Handle HostNotFound Error
                            throw;
                        default:
                            // If this is an unknown status it means that the error is fatal and retry will likely fail.
                            throw;
                    }
                }

                await writer.FlushAsync();
                // In order to prolong the lifetime of the stream, detach it from the DataWriter
                writer.DetachStream();
            }
        }

        public async Task<String> read()
        {
            DataReader reader;
            StringBuilder strBuilder;
            using (reader = new DataReader(socket.InputStream))
            {
                strBuilder = new StringBuilder();

                // Set the DataReader to only wait for available data (so that we don't have to know the data size)
                reader.InputStreamOptions = Windows.Storage.Streams.InputStreamOptions.Partial;
                // The encoding and byte order need to match the settings of the writer we previously used.
                reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
                reader.ByteOrder = Windows.Storage.Streams.ByteOrder.LittleEndian;

                // Send the contents of the writer to the backing stream. 
                // Get the size of the buffer that has not been read.
                await reader.LoadAsync(256);

                // Keep reading until we consume the complete stream.
                while (reader.UnconsumedBufferLength > 0)
                {
                    strBuilder.Append(reader.ReadString(reader.UnconsumedBufferLength));
                    await reader.LoadAsync(256);
                }

                reader.DetachStream();
                return strBuilder.ToString();
            }
        }

        private void repoBtnClicked(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            int index = int.Parse(btn.Name);
            this.Frame.Navigate(typeof(RepoDetailsPage), user.Repos[index]);
        }



        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            string name = textBox.Text;
            parsePageOfUserRepos(name);
        }

        private void fillRichTextBlockWithNewStr(string str)
        {
            Run run = new Run();
            run.Text = str;

            // Create paragraph
            Paragraph paragraph = new Paragraph();

            // Add run to the paragraph
            paragraph.Inlines.Add(run);

            // Add paragraph to the rich text block
            //richTextBlock.Blocks.Add(paragraph);
        }

        private void BGRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //richTextBlock.Blocks.Clear();
            RadioButton r = (RadioButton)sender;
            fillRichTextBlockWithNewStr(r.Name + " repositories.count = " + repositories.Count);
            int index = int.Parse(r.Name);
            repolistView.Items.Clear();
            //int maxIndex = repositories.Count > 7*index + 7 ? (7*index + 7) : repositories.Count;
            int maxIndex;
            if (repositories.Count > 7 * index + 7)
            {
                maxIndex = 7 * index + 7;
            }
            else maxIndex = repositories.Count;
            fillRichTextBlockWithNewStr("Max index " + maxIndex);
            for (int i = 7 * index; i < maxIndex; i++)
            {
                repolistView.Items.Add(repositories[i]);
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            User userCame = e.Parameter as User;
            try
            {
                parsePageOfUserRepos(userCame.UserName);
                textBox.Text = userCame.UserName;
            }
            catch (Exception ex)
            {
                return;
            }
        }

        private async void mainPage_SizeChanged(object sender, System.EventArgs e)
        {
            var Width = Window.Current.Bounds.Width;
            var Height = Window.Current.Bounds.Height;
            await new MessageDialog(Width + " " + Height).ShowAsync();
        }

        private async void parsePageOfUserRepos(string name)
        {
            repositories.Clear();
            pagesRadio.Clear();
            repolistView.Items.Clear();
            //pagesPanel.Children.Clear();

            /*Find image of user*/
            /**********************************************************/

            //String urlUserData = "https://api.github.com/" + name;
            //HttpClient http = new System.Net.Http.HttpClient();
            //HttpResponseMessage response = await http.GetAsync(urlUserData);

            string urlUserData = $"GET https://github.com/{name} HTTP/1.1\r\n" +
                                  "Host: github.com\r\n" +
                                  "Connection: keep-alive\r\n" +
                                  "User-Agent: Kiba-kun\r\n\r\n" +
                                  "";
            await _socketCient.Connect("github.com", "443", "haha");
            await _socketCient.Send(urlUserData);
            string response = await _socketCient.Read();

            user = new User();
            user.UserName = name;
            var html = response.Substring(response.IndexOf("\r\n\r\n"));

            string[] chooseImageSrc = html.Split('>');
            bool was = false;
            foreach (string findImage in chooseImageSrc)
            {
                if (findImage.ToUpper().Contains("@" + name.ToUpper()))
                {
                    string[] words = findImage.Split(' ');
                    string urlImage = "";
                    foreach (string word in words)
                    {
                        if (word.Contains("src="))
                        {
                            urlImage = word.Substring(5);
                            urlImage.Remove(urlImage.Length - 1);
                            user.UrImage = urlImage;
                            was = true;
                            break;
                        }
                    }
                    //break;
                }
                if (was) break;
            }
            /**********************************************************/
            /*Found image of user*/


            /*Find repositories of user*/
            /**********************************************************/

            string repoRequest = $"GET https://github.com/{name}/?tab=repositories HTTP/1.1\r\n" +
                                 "Host: github.com\r\n" +
                                 "Connection: keep-alive\r\n" +
                                 "User-Agent: Vearol\r\n\r\n";

            await _socketCient.Connect("github.com", "443", "haha");
            await _socketCient.Send(repoRequest);
            response = await _socketCient.Read();
            html = response.Substring(response.IndexOf("\r\n\r\n"));
            if (html.Contains("Page not found"))
            {
                return;
            }

            string[] sentences = html.Split('<');
            foreach (string choose in sentences)
                if (choose.Contains("codeRepository"))
                {
                    Repository repo = new Repository()
                    {
                        User = user,
                        Title = choose.Split('>')[1]
                    };
                    user.Repos.Add(repo);
                }
            /**********************************************************/
            /*Found repositories of user*/



            //Random random = new Random();

            /*Creating radiobuttons and filling our listView with our repos*/
            /**********************************************************/
            int countRepos = user.Repos.Count;
            fillRichTextBlockWithNewStr("" + countRepos);
            int firstPageCountElements = countRepos > 7 ? 7 : countRepos;

            for (int i = 0; i < countRepos; i++)
            {
                Button button = new Button();
                button.Content = user.Repos[i].Title;
                button.Name = "" + i;
                button.Width = 480;
                button.HorizontalContentAlignment = HorizontalAlignment.Left;
                button.Click += repoBtnClicked;
                button.Margin = new Thickness(0, 10, 0, 0);
                repositories.Add(button);
                if (i < firstPageCountElements)
                    repolistView.Items.Add(button);
            }



        }

        private void repolistView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
