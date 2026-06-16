using System;
using System.Windows.Input;

namespace TroLySoCaNhan.MVVM
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        // Sự kiện báo cho giao diện (UI) biết trạng thái nút bấm đã thay đổi
        public event EventHandler? CanExecuteChanged;

        // Constructor nhận 1 hoặc 2 tham số (Hàm chạy, Hàm điều kiện)
        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }

        // Hàm ép giao diện cập nhật trạng thái mờ/sáng của nút
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}