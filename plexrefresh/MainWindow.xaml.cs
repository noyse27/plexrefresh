using System.Windows;
using System.Windows.Controls;
using plexrefresh.ViewModels;

namespace plexrefresh;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            if (DataContext is MainViewModel vm)
            {
                // Synchronize PasswordBox with the loaded config token
                TokenBox.Password = vm.Token ?? string.Empty;

                // Sync PasswordBox when ViewModel property changes (e.g. from TextBox)
                vm.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(MainViewModel.Token))
                    {
                        if (TokenBox.Password != vm.Token)
                        {
                            TokenBox.Password = vm.Token ?? string.Empty;
                        }
                    }
                };
            }
        };
    }

    private void TokenBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is MainViewModel vm)
        {
            // Avoid recursive updates
            if (vm.Token != TokenBox.Password)
            {
                vm.Token = TokenBox.Password;
            }
        }
    }

    private void OnSaveConfigClick(object sender, RoutedEventArgs e)
    {
        // This method can be removed if using Command binding in XAML, 
        // but keeping it for compatibility if someone still calls it.
        if (DataContext is MainViewModel vm)
        {
            vm.Token = TokenBox.Password;
            if (vm.SaveConfigCommand.CanExecute(null))
            {
                vm.SaveConfigCommand.Execute(null);
            }
        }
    }
}
