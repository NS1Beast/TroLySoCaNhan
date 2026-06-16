using System;
using TroLySoCaNhan.ViewModels;
using Wpf.Ui.Controls;

namespace TroLySoCaNhan.Views
{
    public partial class DashBoard : FluentWindow
    {
        public DashBoard()
        {
            InitializeComponent();

            if (DataContext is DashboardViewModel vm)
            {
                RegisterViewModelEvents(vm);

                // Load dữ liệu ban đầu
                vm.LoadDocumentsCommand.Execute(null);
            }
        }

        private void RegisterViewModelEvents(DashboardViewModel vm)
        {
            vm.ProfileRequested += Vm_ProfileRequested;
            vm.GroupRequested += Vm_GroupRequested;
            vm.SettingsRequested += Vm_SettingsRequested;
            vm.UpgradeRequested += Vm_UpgradeRequested;
            vm.StorageRequested += Vm_StorageRequested;
        }

        private void UnregisterViewModelEvents(DashboardViewModel vm)
        {
            vm.ProfileRequested -= Vm_ProfileRequested;
            vm.GroupRequested -= Vm_GroupRequested;
            vm.SettingsRequested -= Vm_SettingsRequested;
            vm.UpgradeRequested -= Vm_UpgradeRequested;
            vm.StorageRequested -= Vm_StorageRequested;
        }

        private void Vm_ProfileRequested(object? sender, EventArgs e)
        {
            if (DataContext is not DashboardViewModel vm)
                return;

            var win = new ProfileWindow(vm.CurrentUser)
            {
                Owner = this
            };

            win.ShowDialog();
        }
        private void Vm_StorageRequested(object? sender, EventArgs e)
        {
            var win = new Storage
            {
                Owner = this
            };

            win.ShowDialog();
        }
        private void Vm_GroupRequested(object? sender, EventArgs e)
        {
            var win = new Group
            {
                Owner = this
            };

            win.ShowDialog();
        }

        private void Vm_SettingsRequested(object? sender, EventArgs e)
        {
            if (DataContext is not DashboardViewModel vm)
                return;

            var win = new ProfileWindow(vm.CurrentUser)
            {
                Owner = this
            };

            win.ShowDialog();
        }

        private void Vm_UpgradeRequested(object? sender, EventArgs e)
        {
            var dlg = new UpgradeDialog
            {
                Owner = this
            };

            dlg.ShowDialog();
        }

        protected override void OnClosed(EventArgs e)
        {
            if (DataContext is DashboardViewModel vm)
            {
                UnregisterViewModelEvents(vm);
            }

            base.OnClosed(e);
        }
    }
}