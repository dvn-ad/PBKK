using System;
using System.Windows.Input;
using Dompetku.Models;
using Dompetku.Services;

namespace Dompetku.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private object? _currentView;
        public object? CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        private string _activeMenu = "Dashboard";
        public string ActiveMenu
        {
            get => _activeMenu;
            set => SetProperty(ref _activeMenu, value);
        }

        public User? CurrentUser => AuthService.CurrentUser;
        public string UserDisplayName => AuthService.CurrentUser?.FullName ?? "Pengguna";
        public string UsernameDisplay => $"@{AuthService.CurrentUser?.Username ?? "user"}";

        public DashboardViewModel DashboardVM { get; }
        public TransactionViewModel TransactionVM { get; }
        public WalletViewModel WalletVM { get; }
        public BudgetViewModel BudgetVM { get; }
        public ReportViewModel ReportVM { get; }
        public CategoryViewModel CategoryVM { get; }

        public ICommand NavigateDashboardCommand { get; }
        public ICommand NavigateTransactionsCommand { get; }
        public ICommand NavigateWalletsCommand { get; }
        public ICommand NavigateBudgetsCommand { get; }
        public ICommand NavigateReportsCommand { get; }
        public ICommand NavigateCategoriesCommand { get; }
        public ICommand LogoutCommand { get; }

        public event Action? RequestLogout;
        public event Action? RequestOpenQuickAddTransaction;
        public event Action? RequestOpenQuickTransfer;

        public ICommand QuickAddTransactionCommand { get; }
        public ICommand QuickTransferCommand { get; }

        public MainViewModel()
        {
            DashboardVM = new DashboardViewModel();
            TransactionVM = new TransactionViewModel();
            WalletVM = new WalletViewModel();
            BudgetVM = new BudgetViewModel();
            ReportVM = new ReportViewModel();
            CategoryVM = new CategoryViewModel();

            // Wire refresh events across modules
            Action refreshAll = RefreshAllData;
            TransactionVM.RequestRefreshAll += refreshAll;
            WalletVM.RequestRefreshAll += refreshAll;
            BudgetVM.RequestRefreshAll += refreshAll;
            CategoryVM.RequestRefreshAll += refreshAll;

            NavigateDashboardCommand = new RelayCommand(() => Navigate("Dashboard", DashboardVM));
            NavigateTransactionsCommand = new RelayCommand(() => Navigate("Transactions", TransactionVM));
            NavigateWalletsCommand = new RelayCommand(() => Navigate("Wallets", WalletVM));
            NavigateBudgetsCommand = new RelayCommand(() => Navigate("Budgets", BudgetVM));
            NavigateReportsCommand = new RelayCommand(() => Navigate("Reports", ReportVM));
            NavigateCategoriesCommand = new RelayCommand(() => Navigate("Categories", CategoryVM));

            QuickAddTransactionCommand = new RelayCommand(() => RequestOpenQuickAddTransaction?.Invoke());
            QuickTransferCommand = new RelayCommand(() => RequestOpenQuickTransfer?.Invoke());

            LogoutCommand = new RelayCommand(() =>
            {
                AuthService.Logout();
                RequestLogout?.Invoke();
            });

            // Default view
            Navigate("Dashboard", DashboardVM);
        }

        public void Navigate(string menuName, object view)
        {
            ActiveMenu = menuName;
            CurrentView = view;
        }

        public void RefreshAllData()
        {
            DashboardVM.LoadData();
            TransactionVM.LoadFilterLookups();
            TransactionVM.ApplyFilter();
            WalletVM.LoadData();
            BudgetVM.LoadBudgets();
            ReportVM.LoadReport();
            CategoryVM.LoadCategories();
        }
    }
}
