using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Diary
{
    /// <summary>Репозиторій на основі JSON-файлу.</summary>
    public class JsonEventRepository : IEventRepository
    {
        private readonly string          _filePath;
        private          List<DiaryEvent> _events = null!;

        private static readonly JsonSerializerOptions _opt =
            new JsonSerializerOptions { WriteIndented = true };

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
            int i = _events.FindIndex(e => e.Id == ev.Id);
            if (i >= 0) { _events[i] = ev; Save(); }
        }

        public void Delete(int id)
        {
            _events.RemoveAll(e => e.Id == id);
            Save();
        }

        public List<DiaryEvent> GetAll()    => new(_events);
        public DiaryEvent?      GetById(int id) =>
            _events.FirstOrDefault(e => e.Id == id);

        private void Load()
        {
            if (!File.Exists(_filePath)) { _events = new(); return; }
            try
            {
                _events = JsonSerializer.Deserialize<List<DiaryEvent>>(
                    File.ReadAllText(_filePath), _opt) ?? new();
            }
            catch { _events = new(); }
        }

        private void Save() =>
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_events, _opt));
    }
}
