using Entities.Models;
using System;
using System.Collections.Generic;

namespace StoreApp.Components
{
    public class ReturnRequestViewModel
    {
        public Order Order { get; set; }
        public DateTime ReturnDeadline { get; set; }
        public List<int> SelectedLineIds { get; set; } = new();
        public string Reason { get; set; } = string.Empty;
        public string DetailedReason { get; set; } = string.Empty;

        public HashSet<int> LockedLineIds { get; set; } = new();

        public Dictionary<int, ReturnStatus> LineStatus { get; set; } = new();
        public Dictionary<int, DateTime?> LineProcessedAt { get; set; } = new();
        public Dictionary<int, string?> LineAdminNotes { get; set; } = new();

        public int DaysRemaining => (ReturnDeadline - DateTime.UtcNow).Days;
    }
}