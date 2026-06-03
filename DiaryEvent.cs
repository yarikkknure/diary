using System;

namespace Diary
{
    /// <summary>
    /// Модель запланованого заходу щоденника.
    /// </summary>
    public class DiaryEvent
    {
        public int Id { get; set; }

        /// <summary>Назва заходу.</summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>Дата та час початку.</summary>
        public DateTime StartDateTime { get; set; }

        /// <summary>Тривалість у хвилинах.</summary>
        public int DurationMinutes { get; set; }

        /// <summary>Місце проведення.</summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>Додаткові нотатки.</summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>Дата та час кінця заходу.</summary>
        public DateTime EndDateTime => StartDateTime.AddMinutes(DurationMinutes);

        /// <summary>Чи вже минув захід.</summary>
        public bool IsPast => EndDateTime < DateTime.Now;

        /// <summary>Повертає true, якщо цей захід перетинається з іншим.</summary>
        public bool OverlapsWith(DiaryEvent other)
        {
            if (other.Id == Id) return false;
            return StartDateTime < other.EndDateTime &&
                   EndDateTime > other.StartDateTime;
        }

        public override string ToString() =>
            $"{StartDateTime:dd.MM HH:mm}  {Title}";
    }
}
