using System;
using System.Windows.Input;

namespace TroLySoCaNhan.MVVM
{
    /// <summary>
    /// ICommand đơn giản, dùng cho binding Command trong MVVM.
    /// Hỗ trợ cả Action (không tham số) và Action&lt;object&gt; (có tham số).
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
            : this(_ => execute(), canExecute == null ? null : new Func<object?, bool>(_ => canExecute()))
        {
        }

        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        public void Execute(object? parameter) => _execute(parameter);

        /// <summary>
        /// Yêu cầu WPF re-evaluate CanExecute trên tất cả CommandBindings.
        /// Gọi khi điều kiện enable thay đổi.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler? CanExecuteChanged;
    }
}
