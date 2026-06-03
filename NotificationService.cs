using System;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Diary
{
    /// <summary>Надсилає Windows Toast-сповіщення про заходи.</summary>
    public static class NotificationService
    {
        /// <summary>Сповіщення з назвою заходу, хвилинами до початку та місцем.</summary>
        public static void SendEventReminder(DiaryEvent ev, int minutesLeft)
        {
            try
            {
                var builder = new ToastContentBuilder()
                    .AddText("Нагадування про захід")
                    .AddText(minutesLeft <= 0
                        ? $"\"{ev.Title}\" починається зараз!"
                        : $"\"{ev.Title}\" починається через {minutesLeft} хв")
                    .SetToastDuration(ToastDuration.Long);

                if (!string.IsNullOrWhiteSpace(ev.Location))
                    builder.AddText($"Місце: {ev.Location}");

                builder.Show();
            }
            catch { }
        }
    }
}
