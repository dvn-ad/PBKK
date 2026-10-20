using System;
using System.Windows;
using System.Windows.Controls;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.Views.Dialogs
{
    public partial class CategoryDialog : Window
    {
        private readonly CategoryRepository _categoryRepo = new();
        private readonly Category? _existingCategory;

        public bool IsSaved { get; private set; }

        public CategoryDialog(Category? existing = null)
        {
            InitializeComponent();
            _existingCategory = existing;

            if (_existingCategory != null)
            {
                TxtTitle.Text = "Edit Kategori";
                TxtName.Text = _existingCategory.CategoryName;

                foreach (ComboBoxItem item in CmbType.Items)
                {
                    if (item.Tag?.ToString() == _existingCategory.Type)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }

                foreach (ComboBoxItem item in CmbColor.Items)
                {
                    if (item.Tag?.ToString() == _existingCategory.ColorCode)
                    {
                        item.IsSelected = true;
                        break;
                    }
                }
            }
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            string name = TxtName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Nama kategori harus diisi.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtName.Focus();
                return;
            }

            string type = ((ComboBoxItem)CmbType.SelectedItem).Tag?.ToString() ?? "Expense";
            string color = ((ComboBoxItem)CmbColor.SelectedItem).Tag?.ToString() ?? "#EF4444";

            try
            {
                if (_existingCategory == null)
                {
                    var newCat = new Category
                    {
                        UserId = user.UserId,
                        CategoryName = name,
                        Type = type,
                        ColorCode = color,
                        IconName = "Tag"
                    };
                    _categoryRepo.Add(newCat);
                }
                else
                {
                    _existingCategory.CategoryName = name;
                    _existingCategory.Type = type;
                    _existingCategory.ColorCode = color;
                    _categoryRepo.Update(_existingCategory);
                }

                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan kategori: {ex.Message}", "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
