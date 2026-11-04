namespace MTSS.DTOs.Reports
{
    public class SocietyDashboardResponse
    {
        public string SocietyName { get; set; } = string.Empty;

        // Infrastructure
        public int TotalWings { get; set; }
        public int TotalFloors { get; set; }
        public int TotalFlats { get; set; }
        public int OccupiedFlats { get; set; }
        public int VacantFlats { get; set; }

        // Residents
        public int TotalResidents { get; set; }
        public int ActiveResidents { get; set; }
        public int InactiveResidents { get; set; }
        public int ResidentsAtHome { get; set; }

        // Amenities
        public int TotalAmenities { get; set; }

        // Guest entries
        public int TotalGuestEntries { get; set; }
        public int PendingGuestEntries { get; set; }
        public int ApprovedGuestEntries { get; set; }
        public int RejectedGuestEntries { get; set; }

        // Parcels
        public int TotalParcels { get; set; }
        public int ReceivedParcels { get; set; }
        public int CollectedParcels { get; set; }

        // Complaints
        public int TotalComplaints { get; set; }
        public int OpenComplaints { get; set; }
        public int InProgressComplaints { get; set; }
        public int ResolvedComplaints { get; set; }
        public int RejectedComplaints { get; set; }

        // Amenity access
        public int TotalAccessAttempts { get; set; }
        public int SuccessfulAccessAttempts { get; set; }
        public int FailedAccessAttempts { get; set; }
    }
}