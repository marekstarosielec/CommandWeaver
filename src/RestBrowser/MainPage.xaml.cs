namespace RestBrowser
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            browserWebView.Source = "http://onet.pl";
        }

    }

}
