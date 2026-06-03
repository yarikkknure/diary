using System;
using System.Collections.Generic;
using System.Linq;

namespace Diary
{
    /// <summary>Бізнес-логіка управління заходами щоденника.</summary>
    public class DiaryService
    {
        private readonly IEventRepository _repo;

        public DiaryService(IEventRepository repo) => _repo = repo;

        public void AddEvent(string title, DateTime start, int dur, string loc, string notes)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва не може бути порожньою.");
            _repo.Add(new DiaryEvent
            {
                Title           = title.Trim(),
                StartDateTime   = start,
                DurationMinutes = dur,
                Location        = loc.Trim(),
                Notes           = notes.Trim()
            });
        }

        public void EditEvent(int id, string title, DateTime start, int dur, string loc, string notes)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва не може бути порожньою.");
            var ev = _repo.GetById(id)!;
            ev.Title           = title.Trim();
            ev.StartDateTime   = start;
            ev.DurationMinutes = dur;
            ev.Location        = loc.Trim();
            ev.Notes           = notes.Trim();
            _repo.Update(ev);
        }

        public void DeleteEvent(int id) => _repo.Delete(id);

        public List<DiaryEvent> GetAll() =>
            _repo.GetAll().OrderBy(e => e.StartDateTime).ToList();

        /// <summary>Повертає найближчий майбутній захід.</summary>
        public DiaryEvent? GetNextEvent() =>
            _repo.GetAll()
                 .Where(e => e.StartDateTime >= DateTime.Now)
                 .OrderBy(e => e.StartDateTime)
                 .FirstOrDefault();

        public List<DiaryEvent> GetByDate(DateTime date) =>
            _repo.GetAll()
                 .Where(e => e.StartDateTime.Date == date.Date)
                 .OrderBy(e => e.StartDateTime)
                 .ToList();

        public List<DiaryEvent> GetUpcoming() =>
            _repo.GetAll()
                 .Where(e => e.StartDateTime.Date >= DateTime.Today)
                 .OrderBy(e => e.StartDateTime)
                 .ToList();

        /// <summary>Повертає пари заходів, що перетинаються за часом.</summary>
        public List<(DiaryEvent A, DiaryEvent B)> GetOverlaps()
        {
            var all = _repo.GetAll().OrderBy(e => e.StartDateTime).ToList();
            var res = new List<(DiaryEvent, DiaryEvent)>();
            for (int i = 0; i < all.Count; i++)
                for (int j = i + 1; j < all.Count; j++)
                    if (all[i].OverlapsWith(all[j]))
                        res.Add((all[i], all[j]));
            return res;
        }

        /// <summary>Видаляє заходи, що вже завершились до сьогодні.</summary>
        public int DeletePastEvents()
        {
            var ids = _repo.GetAll()
                .Where(e => e.EndDateTime < DateTime.Today)
                .Select(e => e.Id).ToList();
            foreach (var id in ids) _repo.Delete(id);
            return ids.Count;
        }

        /// <summary>Переносить минулі заходи на вказану кількість днів уперед.</summary>
        public int PostponePastEvents(int days)
        {
            var past = _repo.GetAll()
                .Where(e => e.EndDateTime < DateTime.Today).ToList();
            foreach (var ev in past)
            {
                ev.StartDateTime = ev.StartDateTime.AddDays(days);
                _repo.Update(ev);
            }
            return past.Count;
        }
    }
}
