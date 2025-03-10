using Duo.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Duo.Views
{
    public sealed partial class ItemsView : UserControl
    {
        public ItemsView()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => this.Bindings.Update();
        }

        public ItemViewModel ViewModel
        {
            get => (ItemViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(ItemViewModel), typeof(ItemsView), 
            new PropertyMetadata(null));
    }
} 