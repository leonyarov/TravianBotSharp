﻿using MainCore.Commands.UI.MainLayout;
using MainCore.Common;
using MainCore.Common.Enums;
using MainCore.Common.Extensions;
using MainCore.Entities;
using MainCore.Infrasturecture.AutoRegisterDi;
using MainCore.Repositories;
using MainCore.Services;
using MainCore.UI.Enums;
using MainCore.UI.Stores;
using MainCore.UI.ViewModels.Abstract;
using MediatR;
using ReactiveUI;
using System.Reactive.Linq;
using System.Reflection;
using Unit = System.Reactive.Unit;

namespace MainCore.UI.ViewModels.UserControls
{
    [RegisterAsSingleton(withoutInterface: true)]
    public class MainLayoutViewModel : ViewModelBase
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfRepository _unitOfRepository;

        private readonly AccountTabStore _accountTabStore;
        private readonly SelectedItemStore _selectedItemStore;
        private readonly IDialogService _dialogService;

        public ListBoxItemViewModel Accounts { get; } = new();
        public AccountTabStore AccountTabStore => _accountTabStore;

        public MainLayoutViewModel(AccountTabStore accountTabStore, SelectedItemStore selectedItemStore, IMediator mediator, IDialogService dialogService, IUnitOfRepository unitOfRepository)
        {
            _accountTabStore = accountTabStore;
            _selectedItemStore = selectedItemStore;
            _dialogService = dialogService;
            _mediator = mediator;
            _unitOfRepository = unitOfRepository;

            AddAccountCommand = ReactiveCommand.CreateFromTask(AddAccountHandler);
            AddAccountsCommand = ReactiveCommand.CreateFromTask(AddAccountsHandler);

            DeleteAccountCommand = ReactiveCommand.CreateFromTask(DeleteAccountHandler);
            LoginCommand = ReactiveCommand.CreateFromTask(LoginHandler);
            LogoutCommand = ReactiveCommand.CreateFromTask(LogoutTask);
            PauseCommand = ReactiveCommand.CreateFromTask(PauseTask);
            RestartCommand = ReactiveCommand.CreateFromTask(RestartTask);

            var accountObservable = this.WhenAnyValue(x => x.Accounts.SelectedItem);
            accountObservable.BindTo(_selectedItemStore, vm => vm.Account);

            accountObservable.Subscribe(x =>
            {
                var tabType = AccountTabType.Normal;
                if (x is null) tabType = AccountTabType.NoAccount;
                _accountTabStore.SetTabType(tabType);
            });
        }

        public async Task Load()
        {
            var tasks = new Task[]
            {
                LoadAccountList(),
                LoadVersion(),
            };
            await Task.WhenAll(tasks);
        }

        private async Task AddAccountHandler()
        {
            Accounts.SelectedItem = null;
            await _mediator.Send(new AddAccountCommand());
        }

        private async Task AddAccountsHandler()
        {
            Accounts.SelectedItem = null;
            await _mediator.Send(new AddAccountsCommand());
        }

        private async Task DeleteAccountHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var result = _dialogService.ShowConfirmBox("Information", $"Are you sure want to delete \n {Accounts.SelectedItem.Content}");
            if (!result) return;
            var accountId = new AccountId(Accounts.SelectedItemId);
            await _mediator.Send(new DeleteAccountCommand(accountId));
        }

        private async Task LoginHandler()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            var result = await _mediator.Send(new LoginAccountCommand(accountId));

            if (result.IsFailed) _dialogService.ShowMessageBox("Error", result.Errors.Select(x => x.Message).First());
        }

        private async Task LogoutTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }

            var accountId = new AccountId(Accounts.SelectedItemId);
            await _mediator.Send(new LogoutAccountCommand(accountId));
        }

        private async Task PauseTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(Accounts.SelectedItemId);

            await _mediator.Send(new PauseAccountCommand(accountId));
        }

        private async Task RestartTask()
        {
            if (!Accounts.IsSelected)
            {
                _dialogService.ShowMessageBox("Warning", "No account selected");
                return;
            }
            var accountId = new AccountId(Accounts.SelectedItemId);

            await _mediator.Send(new RestartAccountCommand(accountId));
        }

        public async Task LoadStatus(AccountId accountId, StatusEnums status)
        {
            var account = await Observable.Start(() =>
            {
                var account = Accounts.Items.FirstOrDefault(x => x.Id == accountId.Value);
                return account;
            }, RxApp.TaskpoolScheduler);

            await Observable.Start(() =>
            {
                account.Color = status.GetColor();
            }, RxApp.MainThreadScheduler);
        }

        public async Task LoadAccountList()
        {
            var items = await Observable.Start(() =>
            {
                var items = _unitOfRepository.AccountRepository.GetItems();
                return items;
            }, RxApp.TaskpoolScheduler);

            await Observable.Start(() =>
            {
                Accounts.Load(items);
            }, RxApp.MainThreadScheduler);
        }

        private async Task LoadVersion()
        {
            var version = await Observable.Start(() =>
            {
                var versionAssembly = Assembly.GetExecutingAssembly().GetName().Version;
                var version = new Version(versionAssembly.Major, versionAssembly.Minor, versionAssembly.Build);
                return version;
            }, RxApp.TaskpoolScheduler);

            await Observable.Start(() =>
            {
                Version = $"{version} - {Constants.Server}";
            }, RxApp.MainThreadScheduler);
        }

        private string _version;

        public string Version
        {
            get => _version;
            set => this.RaiseAndSetIfChanged(ref _version, value);
        }

        public ReactiveCommand<Unit, Unit> AddAccountCommand { get; }
        public ReactiveCommand<Unit, Unit> AddAccountsCommand { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccountCommand { get; }
        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> LogoutCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        public ReactiveCommand<Unit, Unit> RestartCommand { get; }
    }
}