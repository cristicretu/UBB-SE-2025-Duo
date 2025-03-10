using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Duo.Services;
using Duo.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Duo
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private DatabaseService _databaseService = null!;
        private ItemViewModel _itemViewModel = null!;

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeDatabaseAndViewModels();
        }

        private async void InitializeDatabaseAndViewModels()
        {
            try
            {
                // Configure connection string for SQL Server
                // Note: You might want to store this in a config file in a real application
                string connectionString = @"Data Source=localhost;Initial Catalog=DuoApp;Integrated Security=True";

                // Initialize database service
                _databaseService = new DatabaseService(connectionString);
                await _databaseService.InitializeDatabaseAsync();

                // Initialize view model
                _itemViewModel = new ItemViewModel(_databaseService);
                
                // Set the ViewModel on the ItemsView
                ItemsViewControl.ViewModel = _itemViewModel;

                // Load items from database
                await _itemViewModel.LoadItemsAsync();
            }
            catch (Exception ex)
            {
                // Show error dialog
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Database Error",
                    Content = $"Could not initialize the database: {ex.Message}\n\nPlease make sure SQL Server is installed and running.",
                    CloseButtonText = "OK"
                };

                await dialog.ShowAsync();
            }
        }
    }
}
