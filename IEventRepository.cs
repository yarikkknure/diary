using System.Collections.Generic;

namespace Diary
{
    /// <summary>Інтерфейс репозиторію заходів.</summary>
    public interface IEventRepository
    {
        void              Add     (DiaryEvent ev);
        void              Update  (DiaryEvent ev);
        void              Delete  (int id);
        List<DiaryEvent>  GetAll  ();
        DiaryEvent?       GetById (int id);
    }
}
