// Entities/Models/ReturnRequest.cs
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public class ReturnRequest
    {
        public int ReturnRequestId { get; set; }
        
        [Required]
        public int OrderId { get; set; }
        public Order Order { get; set; }
        
        [Required]
        public string UserId { get; set; }
        
        [Required]
        public DateTime RequestedAt { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; }
        
        public string? DetailedReason { get; set; }
        
        public ReturnStatus Status { get; set; }
        
        public DateTime? ProcessedAt { get; set; }
        public string? ProcessedBy { get; set; }
        public string? AdminNotes { get; set; }
        
        public List<ReturnRequestLine> Lines { get; set; } = new();
    }
    
    public class ReturnRequestLine
    {
        public int ReturnRequestLineId { get; set; }
        
        public int ReturnRequestId { get; set; }
        public ReturnRequest ReturnRequest { get; set; }
        
        public int CartLineId { get; set; }
        public CartLine CartLine { get; set; }
        
        public int Quantity { get; set; }
    }
    
    public enum ReturnStatus
    {
        Pending,      // Beklemede
        Approved,     // Onaylandı
        Rejected,     // Reddedildi
        Completed     // Tamamlandı (ürün iade alındı, para iadesi yapıldı)
    }
}