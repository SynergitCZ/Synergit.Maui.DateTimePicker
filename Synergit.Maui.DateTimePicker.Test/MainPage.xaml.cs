namespace Synergit.Maui.DateTimePicker.Test
{
    public partial class MainPage : ContentPage
    {
        private DateTime? myDateTime = DateTime.Now;

        public MainPage()
        {
            InitializeComponent();
        }

        public DateTime? MyDateTime
        {
            get => myDateTime;
            set
            {
                myDateTime = value;
                OnPropertyChanged(nameof(MyDateTime));
            }
        }

    }

}
