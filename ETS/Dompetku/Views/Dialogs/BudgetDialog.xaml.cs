using System;
using System.Globalization;
using System.Windows;
using Dompetku.Data;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.Views.Dialogs
{
    public partial class BudgetDialog : Window
    {
        private readonly CategoryRepository _categoryRepo = new();
        private readonly BudgetRepository _budgetRepo = new();
        private readonly Budget? _existingBudget;

        public bool IsSaved { get; private set; }

        public BudgetDialog(int defaultMonth, int defaultYear, Budget? existing = null)
        {
            InitializeComponent();
            _existingBudget = existing;

            InitDatePickers(defaultMonth, defaultYear);
            LoadCategories();

            if (_existingBudget != null)
            {
                TxtLimit.Text = _existingBudget.LimitAmount.ToString("N0");
                CmbCategory.SelectedValue = _existingBudget.CategoryId;
                CmbCategory.IsEnabled = false; // Kategori tidak diubah, cukup edit limit
            }
        }

        private void InitDatePickers(int month, int year)
        {
            for (int m = 1; m <= 12; m++)
            {
                var monthName = CultureInfo.GetCultureInfo("id-ID").DateTimeFormat.GetMonthName(m);
                CmbMonth.Items.Add(new { MonthNum = m, DisplayName = monthName });
            }
            CmbMonth.DisplayMemberPath = "DisplayName";
            CmbMonth.SelectedValuePath = "MonthNum";
            CmbMonth.SelectedValue = month;

            int curYear = DateTime.Now.Year;
            for (int y = curYear - 2; y <= curYear + 3; y++)
            {
                CmbYear.Items.Add(y);
            }
            CmbYear.SelectedItem = year;
        }

        private void LoadCategories()
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            var categories = _categoryRepo.GetByType(user.UserId, "Expense");
            CmbCategory.ItemsSource = categories;
            if (categories.Count > 0) CmbCategory.SelectedIndex = 0;
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            var user = AuthService.CurrentUser;
            if (user == null) return;

            if (CmbCategory.SelectedValue == null)
            {
                MessageBox.Show("Silakan pilih kategori pengeluaran.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var textAmt = TxtLimit.Text.Trim().Replace("Rp", "").Replace(".", "").Replace(",", ".").Trim();
            if (!decimal.TryParse(textAmt, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal limit) || limit <= 0)
            {
                MessageBox.Show("Masukkan nominal batas anggaran yang valid dan lebih dari 0.", "Validasi Gagal", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtLimit.Focus();
                return;
            }

            int catId = (int)CmbCategory.SelectedValue;
            int month = (int)CmbMonth.SelectedValue;
            int year = (int)CmbYear.SelectedItem;

            try
            {
                var budget = new Budget
                {
                    UserId = user.UserId,
                    CategoryId = catId,
                    LimitAmount = limit,
                    PeriodMonth = month,
                    PeriodYear = year
                };

                _budgetRepo.SaveOrUpdate(budget);
                IsSaved = true;
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Gagal menyimpan anggaran: {ex.Message}", "Kesalahan Database", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
