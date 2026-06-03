using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diary
{
    /// <summary>
    /// Реалізація репозиторію на основі JSON-файлу.
    /// </summary>
    public class JsonEventRepository : IEventRepository
    {
        private readonly string _filePath;
        private List<DiaryEvent> _events = null!;

        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public JsonEventRepository(string filePath)
        {
            _filePath = filePath;
            Load();
        }

        public void Add(DiaryEvent ev)
        {
            ev.Id = _events.Count > 0 ? _events.Max(e => e.Id) + 1 : 1;
            _events.Add(ev);
            Save();
        }

        public void Update(DiaryEvent ev)
        {
            int idx = _events.FindIndex(e => e.Id == ev.Id);
            if (idx >= 0) { _events[idx] = ev; Save(); }
        }

        public void Delete(int id)
        {
            _events.RemoveAll(e => e.Id == id);
            Save();
        }

        public List<DiaryEvent> GetAll() => new List<DiaryEvent>(_events);

        public DiaryEvent? GetById(int id) =>
            _events.FirstOrDefault(e => e.Id == id);

        private void Load()
        {
            if (!File.Exists(_filePath)) { _events = new List<DiaryEvent>(); return; }
            try
            {
                string json = File.ReadAllText(_filePath);
                _events = JsonSerializer.Deserialize<List<DiaryEvent>>(json, _options)
                          ?? new List<DiaryEvent>();
            }
            catch { _events = new List<DiaryEvent>(); }
        }

        private void Save()
        {
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_events, _options));
        }
    }
}
