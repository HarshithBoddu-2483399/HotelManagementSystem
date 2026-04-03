using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Services
{
    public interface IManagerService
    {
        // --- Dashboard & Tasks ---
        ManagerDashboardViewModel GetManagerDashboardData();
        void AssignStaffToTask(int taskId, int staffId, DateTime targetDate, TimeSpan deadlineTime);

        // --- Staff Management ---
        IEnumerable<User> GetAllStaff();
        User? GetStaffById(int userId);
        void AddStaff(User staff);
        void UpdateStaff(User staff);
        void DeleteStaff(int userId);

        // --- Attendance ---
        AttendanceViewModel GetAttendanceByDate(DateTime date);
        void MarkAttendance(int userId, DateTime date, bool isPresent);
    }
}