using System;
using System.Collections.ObjectModel;
using TroLySoCaNhan.MVVM;

namespace TroLySoCaNhan.ViewModels
{
    public class GroupViewModel : ViewModelBase
    {
        public ObservableCollection<GroupMemberItem> Members { get; } = new ObservableCollection<GroupMemberItem>();

        private GroupMemberItem? _selectedMember;
        public GroupMemberItem? SelectedMember
        {
            get => _selectedMember;
            set => SetProperty(ref _selectedMember, value);
        }

        public GroupViewModel()
        {
            LoadDemoMembers();
        }

        private void LoadDemoMembers()
        {
            Members.Clear();

            Members.Add(new GroupMemberItem
            {
                TenHienThi = "Nguyễn Văn A",
                MaNgauNhien = "UID-8A9B2C",
                Email = "vana@example.com",
                VaiTroTrongNhom = "Owner",
                QuyenHienThi = "Toàn quyền",
                NgayThamGia = DateTime.Now.AddDays(-12)
            });

            Members.Add(new GroupMemberItem
            {
                TenHienThi = "Trần Thị B",
                MaNgauNhien = "UID-3F7K1M",
                Email = "thib@example.com",
                VaiTroTrongNhom = "Manager",
                QuyenHienThi = "Xem, Thêm, Sửa",
                NgayThamGia = DateTime.Now.AddDays(-7)
            });

            Members.Add(new GroupMemberItem
            {
                TenHienThi = "Lê Văn C",
                MaNgauNhien = "UID-9Q2X5Z",
                Email = "vanc@example.com",
                VaiTroTrongNhom = "Member",
                QuyenHienThi = "Xem tài liệu",
                NgayThamGia = DateTime.Now.AddDays(-3)
            });
        }
    }

    public class GroupMemberItem : ViewModelBase
    {
        public string TenHienThi { get; set; } = string.Empty;
        public string MaNgauNhien { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VaiTroTrongNhom { get; set; } = string.Empty;
        public string QuyenHienThi { get; set; } = string.Empty;
        public DateTime NgayThamGia { get; set; }
    }
}