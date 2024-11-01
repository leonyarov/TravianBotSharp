﻿using FluentValidation;
using MainCore.Commands.UI;
using MainCore.UI.Models.Input;
using MainCore.UI.Models.Output;
using MainCore.UI.ViewModels.Abstract;
using ReactiveUI;
using System.Reactive.Linq;

namespace MainCore.UI.ViewModels.Tabs
{
    [RegisterSingleton<AddAccountViewModel>]
    public class AddAccountViewModel : TabViewModelBase
    {
        public AccountInput AccountInput { get; } = new();
        public AccessInput AccessInput { get; } = new();

        private readonly IValidator<AccessInput> _accessInputValidator;
        private readonly IDialogService _dialogService;

        public ReactiveCommand<Unit, Unit> AddAccess { get; }
        public ReactiveCommand<Unit, Unit> EditAccess { get; }
        public ReactiveCommand<Unit, Unit> DeleteAccess { get; }
        public ReactiveCommand<Unit, Unit> AddAccount { get; }

        public AddAccountViewModel(IValidator<AccessInput> accessInputValidator, IDialogService dialogService)
        {
            _accessInputValidator = accessInputValidator;
            _dialogService = dialogService;

            AddAccess = ReactiveCommand.CreateFromTask(AddAccessHandler);
            EditAccess = ReactiveCommand.CreateFromTask(EditAccessHandler);
            DeleteAccess = ReactiveCommand.Create(DeleteAccessHandler);
            AddAccount = ReactiveCommand.CreateFromTask(AddAccountHandler);

            this.WhenAnyValue(vm => vm.SelectedAccess)
                .WhereNotNull()
                .Subscribe(x => x.CopyTo(AccessInput));

            DeleteAccess.Subscribe(x => SelectedAccess = null);
            AddAccount.Subscribe(x =>
            {
                AccountInput.Clear();
                AccessInput.Clear();
            });
        }

        private async Task AddAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            if (string.IsNullOrEmpty(AccountInput.Username))
            {
                AccountInput.Username = AccessInput.Username;
            }

            AccountInput.Accesses.Add(AccessInput.Clone());
        }

        private async Task EditAccessHandler()
        {
            var result = _accessInputValidator.Validate(AccessInput);

            if (!result.IsValid)
            {
                await _dialogService.MessageBox.Handle(new MessageBoxData("Error", result.ToString()));
                return;
            }

            AccessInput.CopyTo(SelectedAccess);
        }

        private void DeleteAccessHandler()
        {
            AccountInput.Accesses.Remove(SelectedAccess);
        }

        private async Task AddAccountHandler()
        {
            var addAccountCommand = Locator.Current.GetService<AddAccountCommand>();
            await addAccountCommand.Execute(AccountInput, default);
        }

        private AccessInput _selectedAccess;

        public AccessInput SelectedAccess
        {
            get => _selectedAccess;
            set => this.RaiseAndSetIfChanged(ref _selectedAccess, value);
        }
    }
}