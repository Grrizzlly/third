using GitHubClient.Classes;
using System;
using System.Collections.Generic;
using uwpLibb;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// Шаблон элемента пустой страницы задокументирован по адресу http://go.microsoft.com/fwlink/?LinkId=234238

namespace GitHubClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class RepoDetailsPage : Page
    {
        private Repository repository;
        List<Commit> commits = new List<Commit>();
        private readonly SocketClient _socketCient = new SocketClient();

        public RepoDetailsPage()
        {
            this.InitializeComponent();
            
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void fillGridWithCommits(int index)
        {
            int max = index + 6 < commits.Count ? index + 6 : commits.Count;
            for (int i = index; i < max; i++)
            {
                Image image = new Image();
                BitmapImage ImgSource = new BitmapImage(new Uri(repository.User.UrImage, UriKind.Absolute));
                image.Source = ImgSource;
                image.Height = 60;
                image.Width = 72;
                image.Margin = new Thickness(0, 0, 0, 0);
                image.HorizontalAlignment = HorizontalAlignment.Left;
                image.VerticalAlignment = VerticalAlignment.Top;
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(70);
                commitsInTheGrid.RowDefinitions.Add(row);
                commitsInTheGrid.Children.Add(image);
                Grid.SetRow(image, commitsInTheGrid.RowDefinitions.Count - 1);
                //TextBox x: Name = "textBox_Copy" Grid.Row = "1" HorizontalAlignment = "Left" TextWrapping = "Wrap" Text = "" VerticalAlignment = "Top"  Width = "232" Height = "63" Margin = "108,10,0,0"
                TextBox newTextBox = new TextBox();
                newTextBox.HorizontalAlignment = HorizontalAlignment.Center;
                newTextBox.VerticalAlignment = VerticalAlignment.Top;
                newTextBox.Text = commits[i].Message + " _________________ " + commits[i].TypeCommit;
                newTextBox.Width = 321;
                newTextBox.Height = 63;
                newTextBox.Margin = new Thickness(99, 0, 10, 0);
                commitsInTheGrid.Children.Add(newTextBox);
                Grid.SetRow(newTextBox, commitsInTheGrid.RowDefinitions.Count - 1);
            }
        }

        private void BGRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //richTextBlock.Blocks.Clear();
            RadioButton r = (RadioButton)sender;
            commitsInTheGrid.Children.Clear();
            commitsInTheGrid.RowDefinitions.Clear();
            int index = int.Parse(r.Name);
            fillGridWithCommits(index*6);
        }


        private void fillListViewWithRadios()
        {
            RadioButton firstRadio = new RadioButton();
            firstRadio.Checked += BGRadioButton_Checked;
            firstRadio.Name = "0";
            firstRadio.IsChecked = true;
            firstRadio.Content = "1 page";
            //pagesRadio.Add(firstRadio);
            listView.Items.Add(firstRadio);
            //textBox.Text += "" + commits.Count;

            int pages = (commits.Count - 1) / 6;
            for (int i = 0; i < pages; i++)
            {
                RadioButton radio = new RadioButton();
                radio.Checked += BGRadioButton_Checked;
                radio.Name = "" + (i + 1);
                radio.IsChecked = false;
                radio.Content = "" + (i + 2) + " page";
                //pagesRadio.Add(firstRadio);
                listView.Items.Add(radio);
            }
        }

        public async void fillCommits(string html){
           // textBox.Text += "\nCount "+commits.Count+"\n";
            string[] sequences = html.Split('<');
            //>Older
            //>Older
            int count = 0;
            Commit commitSave = null;
            foreach (string findCommit in sequences)
            {
                char symbol = (char)34;
                //if (findCommit.Contains("href=" + symbol + "/RosSGaiduk/SocialNetwork/commit/"))
                if (findCommit.ToUpper().Contains(("href=" + symbol + "/" + repository.User.UserName + "/" + repository.Title.ToString() + "/commit/").ToUpper())){
                    count++;
                    //a href = "/RosSGaiduk/SocialNetwork/commit/1159ccd10ced266e12a1f078cea1ab769dbfc7ba" class="message" data-pjax="true" title="added a possibility to search videos">added a possibility to search videos
                    //a href = "/RosSGaiduk/SocialNetwork/commit/1159ccd10ced266e12a1f078cea1ab769dbfc7ba" class="sha btn btn-outline BtnGroup-item">
                    //1159ccd
                    string[] parseFoundStrings = findCommit.Split('>');
                    if (count % 2 == 1){
                        Commit commit = new Commit();
                        commit.Repo = repository;
                        commit.Message = parseFoundStrings[1];
                        commitSave = commit;
                    }else{
                        //c.TypeCommit = parseFoundStrings[1];
                        string buildType = "";
                        string word = parseFoundStrings[1];
                        for (int i = 0; i < word.Length; i++)
                            if ((int)word[i] > 35) buildType += word[i];
                        commitSave.TypeCommit = buildType;
                        commits.Add(commitSave);
                    }
                }
                //href = "/RosSGaiduk/SocialNetwork/commits/master?after=1159ccd10ced266e12a1f078cea1ab769dbfc7ba+34" rel = "nofollow" >

                if (findCommit.Contains(">Older"))
                {
                    char c = (char)34;
                    string[] words = findCommit.Split(c);
                    string buildUrl = "";
                    string found = "https://github.com" + words[1];
                    for (int i = 0; i < found.Length; i++)
                    {
                        if ((int)found[i] > 36)
                            buildUrl += found[i];
                    }
                    string repoRequest = $"GET {buildUrl} HTTP/1.1\r\n" +
                                 "Host: github.com\r\n" +
                                 "Connection: keep-alive\r\n" +
                                 "User-Agent: Kiba-kun\r\n\r\n";

                    await _socketCient.Connect("github.com", "443", "haha");
                    await _socketCient.Send(repoRequest);
                    //string url = "https://github.com/RosSGaiduk/SocialNetwork/commits/master?after=1159ccd10ced266e12a1f078cea1ab769dbfc7ba+34";
                    //HttpClient http = new System.Net.Http.HttpClient();
                    //HttpResponseMessage response = await http.GetAsync(buildUrl);
                    //var continueHtml = await response.Content.ReadAsStringAsync();
                    var continueHtml = await _socketCient.Read();
                   // textBox.Text += "BuildUrl: \n"+buildUrl;
                    fillCommits(continueHtml);
                }
            }

            listView.Items.Clear();
            fillListViewWithRadios();
        }
        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/"+repository.User.UserName+"/"+repository.Title.ToString()+"/commits/master";
            //string url = "https://github.com/RosSGaiduk/SocialNetwork/commits/master";
            string repoRequest = $"GET {url} HTTP/1.1\r\n" +
                                 "Host: github.com\r\n" +
                                 "Connection: keep-alive\r\n" +
                                 "User-Agent: Kiba-kun\r\n\r\n";
            await _socketCient.Connect("github.com", "443", "haha");
            await _socketCient.Send(repoRequest);
            //string url = "https://github.com/RosSGaiduk/SocialNetwork/commits/master?after=1159ccd10ced266e12a1f078cea1ab769dbfc7ba+34";
            //HttpClient http = new System.Net.Http.HttpClient();
            //HttpResponseMessage response = await http.GetAsync(buildUrl);
            //var continueHtml = await response.Content.ReadAsStringAsync();
            var response = await _socketCient.Read();
            //var continueHtml = await _socketCient.Read();
            var html = response.Substring(response.IndexOf("\r\n\r\n"));
            //textBox.Text = html;
            //textBox.Text = "";
            fillCommits(html);
        }

        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            Repository repo = e.Parameter as Repository;
           // textBox.Text = repo.Title+" "+repo.User.UserName;
            string title = "";
            for (int i = 0; i < repo.Title.Length; i++)
                if ((int)repo.Title[i] > 34)
                    title += repo.Title[i];

            repo.Title = title;
            repository = repo;
        }

        private void reposOfUser_click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage), repository.User);
        }

        private void button_Click_1(object sender, RoutedEventArgs e)
        {
            //textBox.Text = ""+commits.Count;
        }
    }
}
