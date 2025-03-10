using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Duo.Models;
using Duo.Services;
using Microsoft.UI.Xaml.Controls;

namespace Duo.ViewModels
{
    /// <summary>
    /// ViewModel for managing Item objects and interacting with the UI
    /// </summary>
    public class ItemViewModel : INotifyPropertyChanged
    {
        private readonly DatabaseService _databaseService;
        private ObservableCollection<Item> _items = new ObservableCollection<Item>();
        private string _newItemName = string.Empty;
        private Item? _selectedItem; // Can be null when nothing is selected

        public ItemViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            
            AddItemCommand = new RelayCommand(async _ => await AddItem(), _ => !string.IsNullOrWhiteSpace(NewItemName));
            DeleteItemCommand = new RelayCommand(async _ => await DeleteItem(), _ => SelectedItem != null);
            UpdateItemCommand = new RelayCommand(async _ => await UpdateItem(), _ => SelectedItem != null);
        }

        public ObservableCollection<Item> Items
        {
            get => _items;
            set
            {
                _items = value;
                OnPropertyChanged();
            }
        }

        public string NewItemName
        {
            get => _newItemName;
            set
            {
                _newItemName = value;
                OnPropertyChanged();
                (AddItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public Item? SelectedItem // Make nullable
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                (DeleteItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (UpdateItemCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddItemCommand { get; }
        public ICommand DeleteItemCommand { get; }
        public ICommand UpdateItemCommand { get; }

        public async Task LoadItemsAsync()
        {
            try
            {
                var items = await _databaseService.GetAllItemsAsync();
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error loading items: {ex.Message}");
            }
        }

        private async Task AddItem()
        {
            try
            {
                var newItem = new Item { Name = NewItemName };
                var newId = await _databaseService.AddItemAsync(newItem);
                newItem.Id = newId;
                Items.Add(newItem);
                NewItemName = string.Empty;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error adding item: {ex.Message}");
            }
        }

        private async Task DeleteItem()
        {
            if (SelectedItem == null) return;

            try
            {
                await _databaseService.DeleteItemAsync(SelectedItem.Id);
                Items.Remove(SelectedItem);
                SelectedItem = null;
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error deleting item: {ex.Message}");
            }
        }

        private async Task UpdateItem()
        {
            if (SelectedItem == null) return;

            try
            {
                await _databaseService.UpdateItemAsync(SelectedItem);
                // Refresh the item in the collection
                int index = Items.IndexOf(SelectedItem);
                if (index >= 0)
                {
                    Items[index] = SelectedItem;
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"Error updating item: {ex.Message}");
            }
        }

        private async Task ShowErrorAsync(string message)
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK"
            };

            await dialog.ShowAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged; // Make nullable

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null) // Make parameter nullable
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName ?? string.Empty));
        }
    }

    /// <summary>
    /// Simple ICommand implementation
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object>? _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler? CanExecuteChanged; // Make nullable

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter!);

        public void Execute(object? parameter) => _execute(parameter!);

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
} 