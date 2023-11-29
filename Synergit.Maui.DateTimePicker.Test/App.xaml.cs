namespace Synergit.Maui.DateTimePicker.Test
{
    public partial class App : Application
    {
        public App()
        {
            Application.Current.UserAppTheme = AppTheme.Light;

            InitializeComponent();

            MainPage = new AppShell();
        
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        // Handler for unhandled XAML exceptions
        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            // Handle the exception here
            Exception exception = (Exception)args.ExceptionObject;
            Console.WriteLine($"Unhandled exception occurred: {exception}");
        }
    }
}
