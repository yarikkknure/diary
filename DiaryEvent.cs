using System;

namespace Diary
{
    /// <summary>Модель запланованого заходу.</summary>
    public class DiaryEvent
    {
        public int      Id              { get; set; }
        public string   Title           { get; set; } = string.Empty;
        public DateTime StartDateTime   { get; set; }
        public int      DurationMinutes { get; set; }
        public string   Location        { get; set; } = string.Empty;
        public string   Notes           { get; set; } = string.Empty;

        public DateTime EndDateTime => StartDateTime.AddMinutes(DurationMinutes);
        public bool     IsPast      => EndDateTime < DateTime.Now;

        /// <summary>Повертає true, якщо заходи перетинаються за часом.</summary>
        public bool OverlapsWith(DiaryEvent other)
        {
            if (other.Id == Id) return false;
            return StartDateTime < other.EndDateTime && EndDateTime > other.StartDateTime;
        }

        public override string ToString() => $"{StartDateTime:dd.MM HH:mm}  {Title}";
    }
}
