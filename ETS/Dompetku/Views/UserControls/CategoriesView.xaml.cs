using System.Windows;
using System.Windows.Controls;
using Dompetku.Models;
using Dompetku.ViewModels;
using Dompetku.Views.Dialogs;

namespace Dompetku.Views.UserControls
{
    public partial class CategoriesView : UserControl
    {
        public CategoriesView()
        {
            InitializeComponent();
        }

        private void OnAddCategoryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CategoryDialog
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.IsSaved)
            {
                if (DataContext is CategoryViewModel vm) vm.LoadCategories();
                if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm) mainVm.RefreshAllData();
            }
        }

        private void OnEditCategoryClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Category c)
            {
                var dialog = new CategoryDialog(c)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true && dialog.IsSaved)
                {
                    if (DataContext is CategoryViewModel vm) vm.LoadCategories();
                    if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm) mainVm.RefreshAllData();
                }
            }
        }
    }
}
