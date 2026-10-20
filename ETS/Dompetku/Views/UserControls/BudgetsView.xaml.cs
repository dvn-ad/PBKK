using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Dompetku.Models;
using Dompetku.ViewModels;
using Dompetku.Views.Dialogs;

namespace Dompetku.Views.UserControls
{
    public partial class BudgetsView : UserControl
    {
        private bool _isInitialized;

        public BudgetsView()
        {
            InitializeComponent();
            InitSelectors();
        }

        private void InitSelectors()
        {
            for (int m = 1; m <= 12; m++)
            {
                var monthName = CultureInfo.GetCultureInfo("id-ID").DateTimeFormat.GetMonthName(m);
                CmbMonthSelector.Items.Add(new { MonthNum = m, DisplayName = monthName });
            }
            CmbMonthSelector.DisplayMemberPath = "DisplayName";
            CmbMonthSelector.SelectedValuePath = "MonthNum";
            CmbMonthSelector.SelectedValue = DateTime.Now.Month;

            int curYear = DateTime.Now.Year;
            for (int y = curYear - 2; y <= curYear + 3; y++)
            {
                CmbYearSelector.Items.Add(y);
            }
            CmbYearSelector.SelectedItem = curYear;

            _isInitialized = true;
        }

        private void OnPeriodChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized) return;
            if (DataContext is BudgetViewModel vm)
            {
                if (CmbMonthSelector.SelectedValue is int m) vm.SelectedMonth = m;
                if (CmbYearSelector.SelectedItem is int y) vm.SelectedYear = y;
            }
        }

        private void OnAddBudgetClick(object sender, RoutedEventArgs e)
        {
            int month = CmbMonthSelector.SelectedValue is int m ? m : DateTime.Now.Month;
            int year = CmbYearSelector.SelectedItem is int y ? y : DateTime.Now.Year;

            var dialog = new BudgetDialog(month, year)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true && dialog.IsSaved)
            {
                if (DataContext is BudgetViewModel vm) vm.LoadBudgets();
                if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm) mainVm.RefreshAllData();
            }
        }

        private void OnEditBudgetClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Budget b)
            {
                var dialog = new BudgetDialog(b.PeriodMonth, b.PeriodYear, b)
                {
                    Owner = Window.GetWindow(this)
                };

                if (dialog.ShowDialog() == true && dialog.IsSaved)
                {
                    if (DataContext is BudgetViewModel vm) vm.LoadBudgets();
                    if (Window.GetWindow(this)?.DataContext is MainViewModel mainVm) mainVm.RefreshAllData();
                }
            }
        }
    }
}
