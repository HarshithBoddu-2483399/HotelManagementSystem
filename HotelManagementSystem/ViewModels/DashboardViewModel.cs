namespace HotelManagementSystem.ViewModels
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int PendingTasks { get; set; }
        public int RoomsAvailable { get; set; }
        public int RoomsOccupied { get; set; }
        public int RoomsMaintenance { get; set; }
    }
}