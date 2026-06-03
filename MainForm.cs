using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Diary
{
    /// <summary>
    /// Головна форма програми «Щоденник — Планувальник заходів».
    /// Простий мінімалістичний інтерфейс без кольорових акцентів.
    /// </summary>
    public class MainForm : Form
    {
        // ── Service ───────────────────────────────────────────────────────
        private DiaryService _svc = null!;
        private List<DiaryEvent> _events = new();
        private DateTime _filterDate = DateTime.Today;
        private bool _showAll = true;

        // ── Timer ─────────────────────────────────────────────────────────
        private System.Windows.Forms.Timer _timer = null!;

        // ── Controls ──────────────────────────────────────────────────────
        // Top
        private Label lblReminder   = null!;

        // Left
        private Panel    pnlLeft    = null!;
        private GroupBox grpFilter  = null!;
        private Button   btnAll     = null!;
        private Button   btnToday   = null!;
        private Button   btnTomorrow = null!;
        private Button   btnAfterTomorrow = null!;
        private DateTimePicker dtpFilter = null!;
        private ListBox  lstEvents  = null!;

        // Right
        private Panel    pnlRight   = null!;
        private Label    lblDTitle  = null!;
        private Label    lblDDate   = null!;
        private Label    lblDDur    = null!;
        private Label    lblDLoc    = null!;
        private Label    lblDNotesH = null!;
        private TextBox  txtDNotes  = null!;
        private Label    lblOverlap = null!;

        // Bottom
        private Panel  pnlBottom  = null!;
        private Button btnNew     = null!;
        private Button btnEdit    = null!;
        private Button btnDelete  = null!;
        private Button btnOverlaps = null!;
        private Button btnCleanup = null!;

        // ── Constructor ───────────────────────────────────────────────────
        public MainForm()
        {
            Text          = "Щоденник — Планувальник заходів";
            Size          = new Size(900, 580);
            MinimumSize   = new Size(720, 460);
            StartPosition = FormStartPosition.CenterScreen;
            Font          = new Font("Segoe UI", 9f);
            BackColor     = SystemColors.Control;

            InitService();
            BuildUI();
            SetupTimer();
            Reload();
        }

        // ── Init ──────────────────────────────────────────────────────────
        private void InitService()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "diary_events.json");
            _svc = new DiaryService(new JsonEventRepository(path));
        }

        // ── Timer ─────────────────────────────────────────────────────────
        private void SetupTimer()
        {
            _timer = new System.Windows.Forms.Timer { Interval = 60_000 };
            _timer.Tick += (_, __) => UpdateReminder();
            _timer.Start();
            UpdateReminder();
        }

        private void UpdateReminder()
        {
            var next = _svc.GetNextEvent();
            if (next == null)
            {
                lblReminder.Text = "Найближчих заходів немає.";
                return;
            }
            var diff = next.StartDateTime - DateTime.Now;
            string when;
            if (diff.TotalSeconds <= 0)
                when = "зараз";
            else if (diff.TotalMinutes < 60)
                when = $"через {(int)diff.TotalMinutes} хв";
            else if (diff.TotalHours < 24)
                when = $"через {(int)diff.TotalHours} год {diff.Minutes} хв";
            else
                when = next.StartDateTime.ToString("dd.MM HH:mm");

            lblReminder.Text = $"Найближчий: \"{next.Title}\" — {when}";
        }

        // ── Build UI ──────────────────────────────────────────────────────
        private void BuildUI()
        {
            SuspendLayout();

            // ── Top strip (reminder) ──────────────────────────────────────
            var pnlTop = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 30,
                BackColor = SystemColors.ControlLight,
            };
            var sepTop = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 1,
                BackColor = SystemColors.ControlDark,
            };
            lblReminder = new Label
            {
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(6, 0, 0, 0),
                Font      = new Font("Segoe UI", 9f),
            };
            pnlTop.Controls.Add(lblReminder);
            pnlTop.Controls.Add(sepTop);
            Controls.Add(pnlTop);

            // ── Bottom bar (buttons) ──────────────────────────────────────
            pnlBottom = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 38,
                BackColor = SystemColors.ControlLight,
                Padding   = new Padding(6, 5, 6, 5),
            };
            var sepBot = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = SystemColors.ControlDark,
            };

            btnNew      = MkBtn("Новий");
            btnEdit     = MkBtn("Змінити");
            btnDelete   = MkBtn("Видалити");
            btnOverlaps = MkBtn("Накладки");
            btnCleanup  = MkBtn("Очистити");

            btnNew.Click      += BtnNew_Click;
            btnEdit.Click     += BtnEdit_Click;
            btnDelete.Click   += BtnDelete_Click;
            btnOverlaps.Click += BtnOverlaps_Click;
            btnCleanup.Click  += BtnCleanup_Click;

            // Lay out buttons left-to-right
            var flowBot = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                Padding       = new Padding(2, 0, 0, 0),
            };
            flowBot.Controls.AddRange(new Control[]
                { btnNew, btnEdit, btnDelete, btnOverlaps, btnCleanup });

            pnlBottom.Controls.Add(flowBot);
            pnlBottom.Controls.Add(sepBot);
            Controls.Add(pnlBottom);

            // ── Left panel (filter + list) ────────────────────────────────
            pnlLeft = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 260,
                BackColor = SystemColors.Control,
            };
            var sepLeft = new Label
            {
                Dock      = DockStyle.Right,
                Width     = 1,
                BackColor = SystemColors.ControlDark,
            };

            // Filter group
            grpFilter = new GroupBox
            {
                Text    = "Фільтр",
                Dock    = DockStyle.Top,
                Height  = 118,
                Padding = new Padding(6),
            };

            btnAll          = MkNavBtn("Всi майбутнi");
            btnToday        = MkNavBtn("Сьогодні");
            btnTomorrow     = MkNavBtn("Завтра");
            btnAfterTomorrow = MkNavBtn("Позавтра");

            btnAll.Click           += (_, __) => { _showAll = true; Reload(); };
            btnToday.Click         += (_, __) => FilterDay(DateTime.Today);
            btnTomorrow.Click      += (_, __) => FilterDay(DateTime.Today.AddDays(1));
            btnAfterTomorrow.Click += (_, __) => FilterDay(DateTime.Today.AddDays(2));

            // 2×2 button grid in GroupBox
            int bw = 108, bh = 24, gx = 6, gy = 18;
            btnAll.Bounds           = new Rectangle(gx,          gy,       bw, bh);
            btnToday.Bounds         = new Rectangle(gx + bw + 4, gy,       bw, bh);
            btnTomorrow.Bounds      = new Rectangle(gx,          gy + 28,  bw, bh);
            btnAfterTomorrow.Bounds = new Rectangle(gx + bw + 4, gy + 28,  bw, bh);

            var lblDtp = new Label
            {
                Text     = "Дата:",
                Location = new Point(gx, gy + 58),
                Size     = new Size(38, 22),
                TextAlign = ContentAlignment.MiddleLeft,
            };
            dtpFilter = new DateTimePicker
            {
                Location = new Point(gx + 42, gy + 58),
                Width    = bw * 2 + 4 - 42,
                Format   = DateTimePickerFormat.Short,
                Value    = DateTime.Today,
            };
            dtpFilter.ValueChanged += (_, __) => FilterDay(dtpFilter.Value.Date);

            grpFilter.Controls.Add(btnAll);
            grpFilter.Controls.Add(btnToday);
            grpFilter.Controls.Add(btnTomorrow);
            grpFilter.Controls.Add(btnAfterTomorrow);
            grpFilter.Controls.Add(lblDtp);
            grpFilter.Controls.Add(dtpFilter);

            // List
            lstEvents = new ListBox
            {
                Dock         = DockStyle.Fill,
                BorderStyle  = BorderStyle.None,
                Font         = new Font("Segoe UI", 9f),
                ItemHeight   = 38,
                DrawMode     = DrawMode.OwnerDrawFixed,
                IntegralHeight = false,
                BackColor    = SystemColors.Window,
            };
            lstEvents.DrawItem              += LstEvents_DrawItem;
            lstEvents.SelectedIndexChanged  += LstEvents_SelectedIndexChanged;
            lstEvents.DoubleClick           += (_, __) => BtnEdit_Click(null, EventArgs.Empty);

            pnlLeft.Controls.Add(lstEvents);
            pnlLeft.Controls.Add(grpFilter);
            pnlLeft.Controls.Add(sepLeft);
            Controls.Add(pnlLeft);

            // ── Right panel (detail) ──────────────────────────────────────
            pnlRight = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = SystemColors.Window,
                Padding   = new Padding(14, 10, 14, 10),
            };

            lblOverlap = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 20,
                Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Visible   = false,
            };
            lblDTitle = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 34,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = SystemColors.ControlText,
            };
            lblDDate  = MkDetailLabel();
            lblDDur   = MkDetailLabel();
            lblDLoc   = MkDetailLabel();
            var sepDetail = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = SystemColors.ControlLight,
            };
            lblDNotesH = new Label
            {
                Text   = "Нотатки:",
                Dock   = DockStyle.Top,
                Height = 20,
                Font   = new Font("Segoe UI", 9f, FontStyle.Bold),
            };
            txtDNotes = new TextBox
            {
                Dock       = DockStyle.Fill,
                Multiline  = true,
                ReadOnly   = true,
                BackColor  = SystemColors.Window,
                BorderStyle = BorderStyle.None,
                Font       = new Font("Segoe UI", 9.5f),
                ScrollBars = ScrollBars.Vertical,
            };

            // Add in reverse dock order (Fill must be last)
            pnlRight.Controls.Add(txtDNotes);
            pnlRight.Controls.Add(lblDNotesH);
            pnlRight.Controls.Add(sepDetail);
            pnlRight.Controls.Add(lblDLoc);
            pnlRight.Controls.Add(lblDDur);
            pnlRight.Controls.Add(lblDDate);
            pnlRight.Controls.Add(lblOverlap);
            pnlRight.Controls.Add(lblDTitle);
            Controls.Add(pnlRight);

            ResumeLayout(false);
        }

        private static Button MkBtn(string text) => new Button
        {
            Text     = text,
            Width    = 90,
            Height   = 26,
            Margin   = new Padding(0, 0, 4, 0),
        };

        private static Button MkNavBtn(string text) => new Button
        {
            Text      = text,
            FlatStyle = FlatStyle.Standard,
            Font      = new Font("Segoe UI", 8.5f),
        };

        private static Label MkDetailLabel() => new Label
        {
            Dock      = DockStyle.Top,
            Height    = 20,
            ForeColor = SystemColors.ControlText,
            Font      = new Font("Segoe UI", 9f),
        };

        // ── Custom list draw ──────────────────────────────────────────────
        private void LstEvents_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= _events.Count) return;
            var ev = _events[e.Index];
            bool sel  = (e.State & DrawItemState.Selected) != 0;
            bool past = ev.IsPast;

            Color bg = sel ? SystemColors.Highlight : SystemColors.Window;
            Color fg = sel ? SystemColors.HighlightText
                     : past ? Color.Gray
                     : SystemColors.WindowText;

            using var bgBr = new SolidBrush(bg);
            e.Graphics.FillRectangle(bgBr, e.Bounds);

            // Overlap mark — thin left border
            var overlaps = _svc.GetOverlaps();
            bool hasOverlap = overlaps.Any(p => p.A.Id == ev.Id || p.B.Id == ev.Id);
            if (hasOverlap)
            {
                using var markBr = new SolidBrush(Color.DarkRed);
                e.Graphics.FillRectangle(markBr, e.Bounds.X, e.Bounds.Y, 3, e.Bounds.Height);
            }

            int px = e.Bounds.X + (hasOverlap ? 8 : 5);
            int py = e.Bounds.Y + 4;

            using var fgBr   = new SolidBrush(fg);
            using var grayBr = new SolidBrush(Color.Gray);
            using var titleF = new Font("Segoe UI", 9f, FontStyle.Bold);
            using var metaF  = new Font("Segoe UI", 8f);

            string title = ev.Title.Length > 32 ? ev.Title[..29] + "..." : ev.Title;
            string meta  = $"{ev.StartDateTime:HH:mm}  +{ev.DurationMinutes} хв" +
                           (string.IsNullOrWhiteSpace(ev.Location) ? "" : $"  {ev.Location}");
            if (meta.Length > 35) meta = meta[..32] + "...";

            e.Graphics.DrawString(title, titleF, fgBr,   px, py);
            e.Graphics.DrawString(meta,  metaF,  grayBr, px, py + 18);

            // Day-group header (right side)
            if (e.Index == 0 || _events[e.Index - 1].StartDateTime.Date != ev.StartDateTime.Date)
            {
                string dayHdr = ev.StartDateTime.Date == DateTime.Today ? "Сьогодні"
                              : ev.StartDateTime.Date == DateTime.Today.AddDays(1) ? "Завтра"
                              : ev.StartDateTime.ToString("ddd dd.MM");
                using var hdrF  = new Font("Segoe UI", 7.5f, FontStyle.Bold);
                using var hdrBr = new SolidBrush(sel ? fg : Color.Gray);
                var sz = e.Graphics.MeasureString(dayHdr, hdrF);
                e.Graphics.DrawString(dayHdr, hdrF, hdrBr,
                    e.Bounds.Right - sz.Width - 4, e.Bounds.Y + 4);
            }

            // Separator
            using var linePen = new Pen(SystemColors.ControlLight);
            e.Graphics.DrawLine(linePen, e.Bounds.Left, e.Bounds.Bottom - 1,
                                          e.Bounds.Right, e.Bounds.Bottom - 1);
        }

        // ── Events ────────────────────────────────────────────────────────
        private void LstEvents_SelectedIndexChanged(object? sender, EventArgs e)
        {
            int idx = lstEvents.SelectedIndex;
            if (idx < 0 || idx >= _events.Count) { ClearDetail(); return; }
            ShowDetail(_events[idx]);
        }

        private void BtnNew_Click(object? sender, EventArgs e)
        {
            using var f = new EventForm();
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _svc.AddEvent(f.EventTitle, f.EventStart,
                                  f.EventDuration, f.EventLocation, f.EventNotes);
                    Reload();
                    UpdateReminder();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            int idx = lstEvents.SelectedIndex;
            if (idx < 0 || idx >= _events.Count) return;
            var ev = _events[idx];
            using var f = new EventForm(ev);
            if (f.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    _svc.EditEvent(ev.Id, f.EventTitle, f.EventStart,
                                   f.EventDuration, f.EventLocation, f.EventNotes);
                    Reload();
                    UpdateReminder();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Помилка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            int idx = lstEvents.SelectedIndex;
            if (idx < 0 || idx >= _events.Count) return;
            var ev = _events[idx];
            if (MessageBox.Show($"Видалити захід \"{ev.Title}\"?",
                    "Підтвердження", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                    == DialogResult.Yes)
            {
                _svc.DeleteEvent(ev.Id);
                Reload();
                UpdateReminder();
            }
        }

        private void BtnOverlaps_Click(object? sender, EventArgs e)
        {
            var overlaps = _svc.GetOverlaps();
            if (overlaps.Count == 0)
            {
                MessageBox.Show("Накладок між заходами не знайдено.",
                    "Аналіз накладок", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Знайдено {overlaps.Count} накладок:\n");
            foreach (var (a, b) in overlaps)
            {
                sb.AppendLine($"  \"{a.Title}\" ({a.StartDateTime:dd.MM HH:mm}–{a.EndDateTime:HH:mm})");
                sb.AppendLine($"  перетинається з \"{b.Title}\" ({b.StartDateTime:dd.MM HH:mm}–{b.EndDateTime:HH:mm})");
                sb.AppendLine();
            }
            MessageBox.Show(sb.ToString(), "Аналіз накладок",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void BtnCleanup_Click(object? sender, EventArgs e)
        {
            var dlg = MessageBox.Show(
                "Що зробити із заходами, які вже минули?\n\n" +
                "Так       — Видалити\n" +
                "Ні         — Перенести на 7 днів уперед\n" +
                "Скасувати — Нічого не робити",
                "Очистити минулi", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (dlg == DialogResult.Yes)
            {
                int n = _svc.DeletePastEvents();
                MessageBox.Show($"Видалено {n} минулих заходів.", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (dlg == DialogResult.No)
            {
                int n = _svc.PostponePastEvents(7);
                MessageBox.Show($"Перенесено {n} заходів на 7 днів уперед.", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Reload();
            UpdateReminder();
        }

        // ── Helpers ───────────────────────────────────────────────────────
        private void FilterDay(DateTime date)
        {
            _showAll    = false;
            _filterDate = date.Date;
            dtpFilter.Value = date;
            Reload();
        }

        private void Reload()
        {
            // Remember current selected id
            int prevId = lstEvents.SelectedIndex >= 0 && lstEvents.SelectedIndex < _events.Count
                ? _events[lstEvents.SelectedIndex].Id : -1;

            _events = _showAll ? _svc.GetUpcoming() : _svc.GetByDate(_filterDate);

            lstEvents.BeginUpdate();
            lstEvents.Items.Clear();
            foreach (var ev in _events)
                lstEvents.Items.Add(ev.ToString());
            lstEvents.EndUpdate();

            // Restore selection
            int restoreIdx = _events.FindIndex(e => e.Id == prevId);
            if (restoreIdx >= 0)
                lstEvents.SelectedIndex = restoreIdx;
            else if (_events.Count > 0)
                lstEvents.SelectedIndex = 0;
            else
                ClearDetail();

            // Overlap warning
            int cnt = _svc.GetOverlaps().Count;
            lblOverlap.Text    = cnt > 0 ? $"Увага: є {cnt} накладок у розкладi!" : "";
            lblOverlap.Visible = cnt > 0;
        }

        private void ShowDetail(DiaryEvent ev)
        {
            lblDTitle.Text  = ev.Title;
            lblDDate.Text   = $"Дата/Час:   {ev.StartDateTime:dddd, dd MMMM yyyy,  HH:mm}";
            lblDDur.Text    = $"Тривалiсть: {ev.DurationMinutes} хв  (завершення о {ev.EndDateTime:HH:mm})";
            lblDLoc.Text    = string.IsNullOrWhiteSpace(ev.Location)
                              ? "Мiсце:      не вказано"
                              : $"Мiсце:      {ev.Location}";
            lblDNotesH.Visible = !string.IsNullOrWhiteSpace(ev.Notes);
            txtDNotes.Text   = ev.Notes;
            txtDNotes.Visible = !string.IsNullOrWhiteSpace(ev.Notes);
        }

        private void ClearDetail()
        {
            lblDTitle.Text  = "";
            lblDDate.Text   = "";
            lblDDur.Text    = "";
            lblDLoc.Text    = "";
            txtDNotes.Text  = "";
            lblDNotesH.Visible = false;
            txtDNotes.Visible  = false;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            base.OnFormClosed(e);
        }
    }
}
