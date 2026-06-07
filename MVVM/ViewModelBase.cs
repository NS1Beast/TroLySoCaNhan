using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TroLySoCaNhan.MVVM
{
    /// <summary>
    /// Base class cho tất cả ViewModel.
    /// Cài đặt sẵn INotifyPropertyChanged + SetProperty có hỗ trợ kiểm tra giá trị cũ.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gán giá trị mới cho property và raise PropertyChanged khi giá trị thay đổi.
        /// </summary>
        /// <returns>true nếu giá trị thay đổi, false nếu giống cũ.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
