using HotelManagementSystem.Data;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagementSystem.Services
{
    public class ManagerService : IManagerService
    {
        private readonly ApplicationDbContext _context;

        public ManagerService(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- DASHBOARD & TASKS ---
        public ManagerDashboardViewModel GetManagerDashboardData()
        {
            var pendingTasks = _context.HousekeepingTasks.Where(t => t.TaskStatus == "PENDING").ToList();

            // Fix 1: Fetch actual invoice data from the database
            var invoices = _context.Invoices.ToList();
            var totalRevenue = invoices.Sum(i => i.TotalAmount);
            var recentInvoices = invoices.OrderByDescending(i => i.InvoiceDate).Take(10).ToList();

            return new ManagerDashboardViewModel
            {
                TotalRooms = _context.Rooms.Count(),
                AvailableRooms = _context.Rooms.Count(r => r.Status == "AVAILABLE"),
                OccupiedRooms = _context.Rooms.Count(r => r.Status == "OCCUPIED" || r.Status == "BOOKED"),
                MaintenanceRooms = _context.Rooms.Count(r => r.Status == "MAINTENANCE"),

                PendingCleaningTasks = pendingTasks.Count,
                WeeklyRevenueGrowth = 0m,

                // Fix 1: Map the fetched data to the ViewModel
                TotalRevenue = totalRevenue,
                RecentInvoices = recentInvoices,

                AvailableStaff = _context.Users.Where(u => u.Role != "Admin").ToList(),
                PendingTasksList = pendingTasks,
                AllRooms = _context.Rooms.ToList()
            };
        }

        public void AssignStaffToTask(int taskId, int staffId, DateTime targetDate, TimeSpan deadlineTime)
        {
            var task = _context.HousekeepingTasks.Find(taskId);
            if (task != null)
            {
                task.AssignedStaffId = staffId;
                task.TaskStatus = "ASSIGNED";
                task.TaskDate = targetDate.Date.Add(deadlineTime);
                _context.SaveChanges();
            }
        }

        // --- STAFF MANAGEMENT ---
        public IEnumerable<User> GetAllStaff()
        {
            return _context.Users.Where(u => u.Role != "Admin").ToList();
        }

        public User? GetStaffById(int userId)
        {
            return _context.Users.Find(userId);
        }

        public void AddStaff(User staff)
        {
            staff.Password = "hotel@123";
            staff.Username = "temp_" + Guid.NewGuid().ToString().Substring(0, 8) + "@hotel.com";

            _context.Users.Add(staff);
            _context.SaveChanges();

            staff.Username = $"staff{staff.UserId}@hotel.com";
            _context.SaveChanges();
        }

        public void UpdateStaff(User updatedStaff)
        {
            var existingStaff = _context.Users.Find(updatedStaff.UserId);
            if (existingStaff != null)
            {
                existingStaff.Name = updatedStaff.Name;
                existingStaff.Phone = updatedStaff.Phone;
                existingStaff.Role = updatedStaff.Role;
                _context.SaveChanges();
            }
        }

        public void DeleteStaff(int userId)
        {
            var staff = _context.Users.Find(userId);
            if (staff != null && staff.Role != "Admin" && staff.Role != "Manager")
            {
                var attendanceRecords = _context.Attendances.Where(a => a.UserId == userId);
                _context.Attendances.RemoveRange(attendanceRecords);

                var tasks = _context.HousekeepingTasks.Where(t => t.AssignedStaffId == userId);
                foreach (var task in tasks)
                {
                    task.AssignedStaffId = 0;
                    task.TaskStatus = "PENDING";
                }

                _context.Users.Remove(staff);
                _context.SaveChanges();
            }
        }

        // --- ATTENDANCE ---
        public AttendanceViewModel GetAttendanceByDate(DateTime date)
        {
            var staffList = _context.Users.Where(u => u.Role == "Housekeeping" || u.Role == "Receptionist" || u.Role == "Manager").ToList();
            var attendanceRecords = _context.Attendances.Where(a => a.Date.Date == date.Date).ToList();

            var vm = new AttendanceViewModel { SelectedDate = date.Date };

            foreach (var staff in staffList)
            {
                var record = attendanceRecords.FirstOrDefault(a => a.UserId == staff.UserId);
                vm.Records.Add(new StaffAttendanceRecord
                {
                    UserId = staff.UserId,
                    Name = staff.Name ?? "Unnamed",
                    Role = staff.Role,
                    IsPresent = record?.IsPresent
                });
            }
            return vm;
        }

        public void MarkAttendance(int userId, DateTime date, bool isPresent)
        {
            var existing = _context.Attendances.FirstOrDefault(a => a.UserId == userId && a.Date.Date == date.Date);
            if (existing != null)
            {
                existing.IsPresent = isPresent;
            }
            else
            {
                _context.Attendances.Add(new Attendance { UserId = userId, Date = date.Date, IsPresent = isPresent });
            }
            _context.SaveChanges();
        }
    }
}