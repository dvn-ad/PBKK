using System;
using System.Globalization;
using System.Windows.Controls;
using Dompetku.ViewModels;

namespace Dompetku.Views.UserControls
{
    public partial class ReportsView : UserControl
    {
        private bool _isInitialized;

        public ReportsView()
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
            if (DataContext is ReportViewModel vm)
            {
                if (CmbMonthSelector.SelectedValue is int m) vm.SelectedMonth = m;
                if (CmbYearSelector.SelectedItem is int y) vm.SelectedYear = y;
            }
        }
    }
}
