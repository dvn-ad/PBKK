using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        private readonly CategoryRepository _categoryRepo = new();

        public ObservableCollection<Category> IncomeCategories { get; } = new();
        public ObservableCollection<Category> ExpenseCategories { get; } = new();

        public ICommand DeleteCategoryCommand { get; }
        public ICommand RefreshCommand { get; }

        public event Action? RequestRefreshAll;

        public CategoryViewModel()
        {
            DeleteCategoryCommand = new RelayCommand(DeleteCategory);
            RefreshCommand = new RelayCommand(LoadCategories);

            LoadCategories();
        }

        public void LoadCategories()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            IncomeCategories.Clear();
            ExpenseCategories.Clear();

            foreach (var c in _categoryRepo.GetAll(user.UserId))
            {
                if (c.Type == "Income")
                    IncomeCategories.Add(c);
                else
                    ExpenseCategories.Add(c);
            }
        }

        private void DeleteCategory(object? param)
        {
            if (param is Category c)
            {
                var user = AuthService.CurrentUser;
                if (user == null) return;

                if (!_categoryRepo.CanDelete(c.CategoryId, user.UserId, out string reason))
                {
                    MessageBox.Show(reason, "Peringatan", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var res = MessageBox.Show($"Apakah Anda yakin ingin menghapus kategori '{c.CategoryName}'?", "Konfirmasi Hapus", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (res == MessageBoxResult.Yes)
                {
                    try
                    {
                        _categoryRepo.Delete(c.CategoryId, user.UserId);
                        LoadCategories();
                        RequestRefreshAll?.Invoke();
                        MessageBox.Show("Kategori berhasil dihapus.", "Informasi", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Gagal menghapus kategori: {ex.Message}", "Kesalahan", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
