using System;
using System.Collections.Generic;
using System.Linq;

namespace Diary
{
    /// <summary>
    /// Сервіс бізнес-логіки для управління заходами щоденника.
    /// Забезпечує: CRUD, нагадування, аналіз накладок, видалення/перенесення минулих заходів.
    /// </summary>
    public class DiaryService
    {
        private readonly IEventRepository _repo;

        public DiaryService(IEventRepository repo) => _repo = repo;

        // ── CRUD ──────────────────────────────────────────────────────────

        /// <summary>Додає новий захід. Назва є обов'язковою.</summary>
        public void AddEvent(string title, DateTime start, int durationMin, string location, string notes)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва заходу не може бути порожньою.");
            if (durationMin <= 0)
                throw new ArgumentException("Тривалість має бути більше 0 хвилин.");

            _repo.Add(new DiaryEvent
            {
                Title = title.Trim(),
                StartDateTime = start,
                DurationMinutes = durationMin,
                Location = location.Trim(),
                Notes = notes.Trim()
            });
        }

        /// <summary>Редагує існуючий захід.</summary>
        public void EditEvent(int id, string title, DateTime start, int durationMin, string location, string notes)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Назва заходу не може бути порожньою.");
            if (durationMin <= 0)
                throw new ArgumentException("Тривалість має бути більше 0 хвилин.");

            var ev = _repo.GetById(id) ?? throw new InvalidOperationException("Захід не знайдено.");
            ev.Title = title.Trim();
            ev.StartDateTime = start;
            ev.DurationMinutes = durationMin;
            ev.Location = location.Trim();
            ev.Notes = notes.Trim();
            _repo.Update(ev);
        }

        /// <summary>Видаляє захід.</summary>
        public void DeleteEvent(int id) => _repo.Delete(id);

        /// <summary>Повертає всі заходи, впорядковані за датою початку.</summary>
        public List<DiaryEvent> GetAll() =>
            _repo.GetAll().OrderBy(e => e.StartDateTime).ToList();

        // ── Нагадування ───────────────────────────────────────────────────

        /// <summary>
        /// Повертає найближчий майбутній захід відносно поточного часу.
        /// </summary>
        public DiaryEvent? GetNextEvent()
        {
            var now = DateTime.Now;
            return _repo.GetAll()
                .Where(e => e.StartDateTime >= now)
                .OrderBy(e => e.StartDateTime)
                .FirstOrDefault();
        }

        // ── Фільтр по даті ────────────────────────────────────────────────

        /// <summary>Повертає заходи на вказану дату (ціла доба).</summary>
        public List<DiaryEvent> GetByDate(DateTime date) =>
            _repo.GetAll()
                 .Where(e => e.StartDateTime.Date == date.Date)
                 .OrderBy(e => e.StartDateTime)
                 .ToList();

        /// <summary>Заходи, що починаються з сьогодні та пізніше.</summary>
        public List<DiaryEvent> GetUpcoming() =>
            _repo.GetAll()
                 .Where(e => e.StartDateTime.Date >= DateTime.Today)
                 .OrderBy(e => e.StartDateTime)
                 .ToList();

        // ── Аналіз накладок ───────────────────────────────────────────────

        /// <summary>
        /// Повертає список пар заходів, що мають перетин у часі.
        /// </summary>
        public List<(DiaryEvent A, DiaryEvent B)> GetOverlaps()
        {
            var all = _repo.GetAll().OrderBy(e => e.StartDateTime).ToList();
            var result = new List<(DiaryEvent, DiaryEvent)>();
            for (int i = 0; i < all.Count; i++)
                for (int j = i + 1; j < all.Count; j++)
                    if (all[i].OverlapsWith(all[j]))
                        result.Add((all[i], all[j]));
            return result;
        }

        // ── Видалення / перенесення минулих заходів ───────────────────────

        /// <summary>
        /// Видаляє всі заходи, що закінчилися до початку сьогоднішнього дня.
        /// </summary>
        public int DeletePastEvents()
        {
            var yesterday = DateTime.Today;
            var toDelete = _repo.GetAll()
                .Where(e => e.EndDateTime < yesterday)
                .Select(e => e.Id)
                .ToList();
            foreach (int id in toDelete) _repo.Delete(id);
            return toDelete.Count;
        }

        /// <summary>
        /// Переносить усі «вчорашні» заходи (що були до сьогодні) на задану кількість днів уперед.
        /// </summary>
        public int PostponePastEvents(int daysForward)
        {
            if (daysForward <= 0) throw new ArgumentException("Кількість днів має бути > 0.");
            var yesterday = DateTime.Today;
            var toPostpone = _repo.GetAll()
                .Where(e => e.EndDateTime < yesterday)
                .ToList();
            foreach (var ev in toPostpone)
            {
                ev.StartDateTime = ev.StartDateTime.AddDays(daysForward);
                _repo.Update(ev);
            }
            return toPostpone.Count;
        }
    }
}
