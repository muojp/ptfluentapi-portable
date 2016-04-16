using System;

namespace PivotalTracker.FluentAPI.Domain
{
    public class Label
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}