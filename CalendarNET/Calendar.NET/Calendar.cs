﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace Calendar.NET
{
    /// <summary>
    /// An enumeration describing various ways to view the calendar
    /// </summary>
    public enum CalendarViews
    {
        /// <summary>
        /// Renders the Calendar in a month view
        /// </summary>
        Month = 1,
        /// <summary>
        /// Renders the Calendar in a day view
        /// </summary>
        Day = 2
    }

    /// <summary>
    /// A Winforms Calendar Control
    /// </summary>
    public class Calendar : UserControl
    {
        private DateTime _calendarDate;
        private Font _dayOfWeekFont;
        private Font _daysFont;
        private Font _todayFont;
        private Font _dateHeaderFont;
        private Font _dayViewTimeFont;
        private bool _showArrowControls;
        private bool _showTodayButton;
        private bool _showDateInHeader;
        private TodayButton _btnToday;
        private NavigateLeftButton _btnLeft;
        private NavigateRightButton _btnRight;
        private bool _showingToolTip;
        private bool _showEventTooltips;
        private bool _loadPresetHolidays;
        private CalendarEvent _clickedEvent;
        private bool _showDisabledEvents;
        private bool _showDashedBorderOnDisabledEvents;
        private bool _dimDisabledEvents;
        private bool _highlightCurrentDay;
        private CalendarViews _calendarView;
        private readonly ScrollPanel _scrollPanel;
        private DateTime _selectedeventdate;
        private bool _pointedtoevent;

        private readonly List<IEvent> _events;
        private readonly List<Rectangle> _rectangles;
        private readonly Dictionary<int, Point> _calendarDays;
        private readonly List<CalendarEvent> _calendarEvents;
        private readonly EventToolTip _eventTip;
        private ContextMenuStrip _contextMenuStrip1;
        private System.ComponentModel.IContainer components;
        private ToolStripMenuItem _miProperties;

        private const int MarginSize = 20;

        /// <summary>
        /// Indicates the font for the times on the day view
        /// </summary>
        public Font DayViewTimeFont
        {
            get { return _dayViewTimeFont; }
            set
            {
                _dayViewTimeFont = value;
                if (_calendarView == CalendarViews.Day)
                    _scrollPanel.Refresh();
                else Refresh();
            }
        }

        /// <summary>
        /// Indicates the type of calendar to render, Month or Day view
        /// </summary>
        public CalendarViews CalendarView
        {
            get { return _calendarView; }
            set
            {
                _calendarView = value;
                _scrollPanel.Visible = value == CalendarViews.Day;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether today's date should be highlighted
        /// </summary>
        public bool HighlightCurrentDay
        {
            get { return _highlightCurrentDay; }
            set
            {
                _highlightCurrentDay = value;
                Refresh();
            }
        }

        /// <summary>
        /// The cliked event date
        /// </summary>
        public DateTime SelectedEventDate 
        {
            get { return _selectedeventdate; }
        }

        /// <summary>
        /// The cliked event date
        /// </summary>
        public bool Pointedtoevent
        {
            get { return _pointedtoevent; }
        }

        /// <summary>
        /// Indicates whether events can be right-clicked and edited
        /// </summary>
        public bool AllowEditingEvents
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether disabled events will appear as "dimmed".
        /// This property is only used if <see cref="ShowDisabledEvents"/> is set to true.
        /// </summary>
        public bool DimDisabledEvents
        {
            get { return _dimDisabledEvents; }
            set
            {
                _dimDisabledEvents = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates if a dashed border should show up around disabled events.
        /// This property is only used if <see cref="ShowDisabledEvents"/> is set to true.
        /// </summary>
        public bool ShowDashedBorderOnDisabledEvents
        {
            get { return _showDashedBorderOnDisabledEvents; }
            set
            {
                _showDashedBorderOnDisabledEvents = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether disabled events should show up on the calendar control
        /// </summary>
        public bool ShowDisabledEvents
        {
            get { return _showDisabledEvents; }
            set
            {
                _showDisabledEvents = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether Federal Holidays are automatically preloaded onto the calendar
        /// </summary>
        public bool LoadPresetHolidays
        {
            get { return _loadPresetHolidays; }
            set
            {
                _loadPresetHolidays = value;
                if (_loadPresetHolidays)
                {
                    _events.Clear();
                    PresetHolidays();
                    Refresh();
                }
                else
                {
                    _events.Clear();
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Indicates whether hovering over an event will display a tooltip of the event
        /// </summary>
        public bool ShowEventTooltips
        {
            get { return _showEventTooltips; }
            set { _showEventTooltips = value; _eventTip.Visible = false; }
        }

        /// <summary>
        /// Get or Set this value to the Font you wish to use to render the date in the upper right corner
        /// </summary>
        public Font DateHeaderFont
        {
            get { return _dateHeaderFont; }
            set
            {
                _dateHeaderFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether the date should be displayed in the upper right hand corner of the calendar control
        /// </summary>
        public bool ShowDateInHeader
        {
            get { return _showDateInHeader; }
            set
            {
                _showDateInHeader = value;
                if (_calendarView == CalendarViews.Day)
                    ResizeScrollPanel();

                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether the calendar control should render the previous/next month buttons
        /// </summary>
        public bool ShowArrowControls
        {
            get { return _showArrowControls; }
            set
            {
                _showArrowControls = value;
                _btnLeft.Visible = value;
                _btnRight.Visible = value;
                if (_calendarView == CalendarViews.Day)
                    ResizeScrollPanel();
                Refresh();
            }
        }

        /// <summary>
        /// Indicates whether the calendar control should render the Today button
        /// </summary>
        public bool ShowTodayButton
        {
            get { return _showTodayButton; }
            set
            {
                _showTodayButton = value;
                _btnToday.Visible = value;
                if (_calendarView == CalendarViews.Day)
                    ResizeScrollPanel();
                Refresh();
            }
        }

        /// <summary>
        /// The font used to render the Today button
        /// </summary>
        public Font TodayFont
        {
            get { return _todayFont; }
            set
            {
                _todayFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// The font used to render the number days on the calendar
        /// </summary>
        public Font DaysFont
        {
            get { return _daysFont; }
            set
            {
                _daysFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// The font used to render the days of the week text
        /// </summary>
        public Font DayOfWeekFont
        {
            get { return _dayOfWeekFont; }
            set
            {
                _dayOfWeekFont = value;
                Refresh();
            }
        }

        /// <summary>
        /// The Date that the calendar is currently showing
        /// </summary>
        public DateTime CalendarDate
        {
            get { return _calendarDate; }
            set
            {
                _calendarDate = value;
                Refresh();
            }
        }

        /// <summary>
        /// Calendar Constructor
        /// </summary>
        public Calendar()
        {
            InitializeComponent();
            _calendarDate = DateTime.Now;
            _dayOfWeekFont = new Font("Arial", 10, FontStyle.Regular);
            _daysFont = new Font("Arial", 10, FontStyle.Regular);
            _todayFont = new Font("Arial", 10, FontStyle.Bold);
            _dateHeaderFont = new Font("Arial", 12, FontStyle.Bold);
            _dayViewTimeFont = new Font("Arial", 10, FontStyle.Bold);
            _showArrowControls = true;
            _showDateInHeader = true;
            _showTodayButton = true;
            _showingToolTip = false;
            _clickedEvent = null;
            _showDisabledEvents = false;
            _showDashedBorderOnDisabledEvents = true;
            _dimDisabledEvents = true;
            AllowEditingEvents = true;
            _highlightCurrentDay = true;
            _calendarView = CalendarViews.Month;
            _scrollPanel = new ScrollPanel();
            _pointedtoevent = false;

            _scrollPanel.RightButtonClicked += ScrollPanelRightButtonClicked;

            _events = new List<IEvent>();
            _rectangles = new List<Rectangle>();
            _calendarDays = new Dictionary<int, Point>();
            _calendarEvents = new List<CalendarEvent>();
            _showEventTooltips = true;
            _eventTip = new EventToolTip { Visible = false };

            Controls.Add(_eventTip);

            LoadPresetHolidays = true;

            _scrollPanel.Visible = false;
            Controls.Add(_scrollPanel);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this._btnToday = new TodayButton();
            this._btnLeft = new NavigateLeftButton();
            this._btnRight = new NavigateRightButton();
            this._contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this._miProperties = new System.Windows.Forms.ToolStripMenuItem();
            this._contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // _btnToday
            // 
            this._btnToday.BackColor = System.Drawing.Color.Transparent;
            this._btnToday.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this._btnToday.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this._btnToday.ButtonFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this._btnToday.ButtonText = "Today";
            this._btnToday.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(144)))), ((int)(((byte)(254)))));
            this._btnToday.HighlightBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(198)))), ((int)(((byte)(198)))));
            this._btnToday.HighlightButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this._btnToday.Location = new System.Drawing.Point(19, 20);
            this._btnToday.Name = "_btnToday";
            this._btnToday.Size = new System.Drawing.Size(72, 29);
            this._btnToday.TabIndex = 0;
            this._btnToday.TextColor = System.Drawing.Color.Black;
            this._btnToday.ButtonClicked += new CoolButton.ButtonClickedArgs(this.BtnTodayButtonClicked);
            // 
            // _btnLeft
            // 
            this._btnLeft.BackColor = System.Drawing.Color.Transparent;
            this._btnLeft.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this._btnLeft.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this._btnLeft.ButtonFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this._btnLeft.ButtonText = "<";
            this._btnLeft.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(144)))), ((int)(((byte)(254)))));
            this._btnLeft.HighlightBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(198)))), ((int)(((byte)(198)))));
            this._btnLeft.HighlightButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this._btnLeft.Location = new System.Drawing.Point(98, 20);
            this._btnLeft.Name = "_btnLeft";
            this._btnLeft.Size = new System.Drawing.Size(42, 29);
            this._btnLeft.TabIndex = 1;
            this._btnLeft.TextColor = System.Drawing.Color.Black;
            this._btnLeft.ButtonClicked += new CoolButton.ButtonClickedArgs(this.BtnLeftButtonClicked);
            // 
            // _btnRight
            // 
            this._btnRight.BackColor = System.Drawing.Color.Transparent;
            this._btnRight.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this._btnRight.ButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(243)))), ((int)(((byte)(243)))));
            this._btnRight.ButtonFont = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            this._btnRight.ButtonText = ">";
            this._btnRight.FocusColor = System.Drawing.Color.FromArgb(((int)(((byte)(77)))), ((int)(((byte)(144)))), ((int)(((byte)(254)))));
            this._btnRight.HighlightBorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(198)))), ((int)(((byte)(198)))), ((int)(((byte)(198)))));
            this._btnRight.HighlightButtonColor = System.Drawing.Color.FromArgb(((int)(((byte)(246)))), ((int)(((byte)(246)))), ((int)(((byte)(246)))));
            this._btnRight.Location = new System.Drawing.Point(138, 20);
            this._btnRight.Name = "_btnRight";
            this._btnRight.Size = new System.Drawing.Size(42, 29);
            this._btnRight.TabIndex = 2;
            this._btnRight.TextColor = System.Drawing.Color.Black;
            this._btnRight.ButtonClicked += new CoolButton.ButtonClickedArgs(this.BtnRightButtonClicked);
            // 
            // _contextMenuStrip1
            // 
            this._contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._miProperties});
            this._contextMenuStrip1.Name = "_contextMenuStrip1";
            this._contextMenuStrip1.Size = new System.Drawing.Size(137, 26);
            // 
            // _miProperties
            // 
            this._miProperties.Name = "_miProperties";
            this._miProperties.Size = new System.Drawing.Size(136, 22);
            this._miProperties.Text = "Properties...";
            this._miProperties.Click += new System.EventHandler(this.MenuItemPropertiesClick);
            // 
            // Calendar
            // 
            this.Controls.Add(this._btnRight);
            this.Controls.Add(this._btnLeft);
            this.Controls.Add(this._btnToday);
            this.Name = "Calendar";
            this.Size = new System.Drawing.Size(512, 440);
            this.Load += new System.EventHandler(this.CalendarLoad);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.CalendarPaint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.CalendarMouseClick);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.CalendarMouseMove);
            this.Resize += new System.EventHandler(this.CalendarResize);
            this.DoubleBuffered = true;
            this._contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Adds an event to the calendar
        /// </summary>
        /// <param name="calendarEvent">The <see cref="IEvent"/> to add to the calendar</param>
        public void AddEvent(IEvent calendarEvent)
        {
            _events.Add(calendarEvent);
            Refresh();
        }

        /// <summary>
        /// Removes an event from the calendar
        /// </summary>
        /// <param name="calendarEvent">The <see cref="IEvent"/> to remove to the calendar</param>
        public void RemoveEvent(IEvent calendarEvent)
        {
            _events.Remove(calendarEvent);
            Refresh();
        }

        /// <summary>
        /// Clear all items from the calendar
        /// </summary>
        public void Clearall()
        {
            _events.Clear();
            Refresh();
        }

        private void CalendarLoad(object sender, EventArgs e)
        {
            if (Parent != null)
                Parent.Resize += ParentResize;
            ResizeScrollPanel();
        }

        private void CalendarPaint(object sender, PaintEventArgs e)
        {
            if (_showingToolTip)
                return;

            if (_calendarView == CalendarViews.Month)
                RenderMonthCalendar(e);
            if (_calendarView == CalendarViews.Day)
                RenderDayCalendar(e);
        }

        private void CalendarMouseMove(object sender, MouseEventArgs e)
        {
            if (!_showEventTooltips)
                return;

            int num = _calendarEvents.Count;
            for (int i = 0; i < num; i++)
            {
                var z = _calendarEvents[i];

                if ((z.EventArea.Contains(e.X, e.Y) && z.Event.TooltipEnabled && _calendarView == CalendarViews.Month) ||
                    (_calendarView == CalendarViews.Day && z.EventArea.Contains(e.X, e.Y + _scrollPanel.ScrollOffset) && z.Event.TooltipEnabled))
                {
                    _eventTip.ShouldRender = false;
                    _showingToolTip = true;
                    _eventTip.EventToolTipText = z.Event.EventItems;
                    if (z.Event.IgnoreTimeComponent == false)
                        _eventTip.EventToolTipText += "\n" + z.Event.Date.ToShortTimeString();
                    _eventTip.Location = new Point(e.X + 5, e.Y - _eventTip.CalculateSize().Height);
                    _eventTip.ShouldRender = true;
                    _eventTip.Visible = true;
                    _pointedtoevent = true;
                    _selectedeventdate = z.Event.Date;

                    _showingToolTip = false;
                    return;
                }
            }

            _eventTip.Visible = false;
            _eventTip.ShouldRender = false;
            _pointedtoevent = false;
        }

        private void ScrollPanelRightButtonClicked(object sender, MouseEventArgs e)
        {
            if (AllowEditingEvents && _calendarView == CalendarViews.Day)
            {
                int num = _calendarEvents.Count;
                for (int i = 0; i < num; i++)
                {
                    var z = _calendarEvents[i];

                    if (z.EventArea.Contains(e.X, e.Y + _scrollPanel.ScrollOffset) && !z.Event.ReadOnlyEvent)
                    {
                        _clickedEvent = z;
                        _contextMenuStrip1.Show(_scrollPanel, new Point(e.X, e.Y));
                        _eventTip.Visible = false;
                        _eventTip.ShouldRender = false;
                        break;
                    }
                }
            }
        }

        //private void CalendarMouseClick(object sender, MouseEventArgs e)
        //{
        //    if (e.Button == MouseButtons.Right && AllowEditingEvents)
        //    {
        //        if (_calendarView == CalendarViews.Month)
        //        {
        //            int num = _calendarEvents.Count;
        //            for (int i = 0; i < num; i++)
        //            {
        //                var z = _calendarEvents[i];

        //                if (z.EventArea.Contains(e.X, e.Y) && !z.Event.ReadOnlyEvent)
        //                {
        //                    _clickedEvent = z;
        //                    _contextMenuStrip1.Show(this, e.Location);
        //                    _eventTip.Visible = false;
        //                    _eventTip.ShouldRender = false;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}

        private void CalendarMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && AllowEditingEvents)
            {
                if (_calendarView == CalendarViews.Month)
                {
                    int num = _calendarEvents.Count;
                    for (int i = 0; i < num; i++)
                    {
                        var z = _calendarEvents[i];

                        if (z.EventArea.Contains(e.X, e.Y) && !z.Event.ReadOnlyEvent)
                        {
                            _clickedEvent = z;
                            //_contextMenuStrip1.Show(this, e.Location);
                            //_eventTip.Visible = false;
                            _eventTip.ShouldRender = false;
                            break;
                        }
                    }
                }
            }
        }

        private void BtnTodayButtonClicked(object sender)
        {
            _calendarDate = DateTime.Now;
            Refresh();
        }

        private void BtnLeftButtonClicked(object sender)
        {
            if (_calendarView == CalendarViews.Month)
                _calendarDate = _calendarDate.AddMonths(-1);
            else if (_calendarView == CalendarViews.Day)
                _calendarDate = _calendarDate.AddDays(-1);
            Refresh();
        }

        private void BtnRightButtonClicked(object sender)
        {
            if (_calendarView == CalendarViews.Day)
                _calendarDate = _calendarDate.AddDays(1);
            else if (_calendarView == CalendarViews.Month)
                _calendarDate = _calendarDate.AddMonths(1);
            Refresh();
        }

        private void MenuItemPropertiesClick(object sender, EventArgs e)
        {
            if (_clickedEvent == null)
                return;

            var ed = new EventDetails { Event = _clickedEvent.Event };

            if (ed.ShowDialog(this) == DialogResult.OK)
            {
                _events.Remove(_clickedEvent.Event);
                _events.Add(ed.NewEvent);
                Refresh();
            }
            _clickedEvent = null;
        }

        private void ParentResize(object sender, EventArgs e)
        {
            ResizeScrollPanel();
            Refresh();
        }

        private void PresetHolidays()
        {
            var aprilFools = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 4, 1),
                RecurringFrequency = RecurringFrequencies.Yearly,
                EventText = "April Fools Day"
            };
            AddEvent(aprilFools);

            var memorialDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 5, 28),
                RecurringFrequency = RecurringFrequencies.Custom,
                EventText = "Memorial Day",
                CustomRecurringFunction = MemorialDayHandler
            };
            AddEvent(memorialDay);

            var newYears = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 1, 1),
                RecurringFrequency = RecurringFrequencies.Yearly,
                EventText = "New Years Day"
            };
            AddEvent(newYears);

            var mlkDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 1, 15),
                RecurringFrequency = RecurringFrequencies.Custom,
                EventText = "Martin Luther King Jr. Day",
                CustomRecurringFunction = MlkDayHandler
            };
            AddEvent(mlkDay);

            var presidentsDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 2, 15),
                RecurringFrequency = RecurringFrequencies.Custom,
                EventText = "President's Day",
                CustomRecurringFunction = MlkDayHandler
            };
            AddEvent(presidentsDay);

            var independanceDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 7, 4),
                RecurringFrequency = RecurringFrequencies.Yearly,
                EventText = "Independence Day"
            };
            AddEvent(independanceDay);

            var laborDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 9, 1),
                RecurringFrequency = RecurringFrequencies.Custom,
                EventText = "Labor Day",
                CustomRecurringFunction = LaborDayHandler
            };
            AddEvent(laborDay);

            var columbusDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 10, 14),
                RecurringFrequency = RecurringFrequencies.Custom,
                EventText = "Columbus Day",
                CustomRecurringFunction = ColumbusDayHandler
            };
            AddEvent(columbusDay);

            var veteransDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 11, 11),
                RecurringFrequency = RecurringFrequencies.Yearly,
                EventText = "Veteran's Day"
            };
            AddEvent(veteransDay);

            var thanksgivingDay = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 11, 11),
                RecurringFrequency = RecurringFrequencies.Custom,
                EventText = "Thanksgiving Day",
                CustomRecurringFunction = ThanksgivingDayHandler
            };
            AddEvent(thanksgivingDay);

            var christmas = new HolidayEvent
            {
                Date = new DateTime(DateTime.Now.Year, 12, 25),
                RecurringFrequency = RecurringFrequencies.Yearly,
                EventText = "Christmas Day"
            };
            AddEvent(christmas);
        }

        [CustomRecurringFunction("Thanksgiving Day Handler", "Selects the fourth Thursday in the month")]
        private bool ThanksgivingDayHandler(IEvent evnt, DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Thursday && dt.Day > 21 && dt.Day <= 28 && dt.Month == evnt.Date.Month)
                return true;
            return false;
        }

        [CustomRecurringFunction("Columbus Day Handler", "Selects the second Monday in the month")]
        private bool ColumbusDayHandler(IEvent evnt, DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Monday && dt.Day > 7 && dt.Day <= 14 && dt.Month == evnt.Date.Month)
                return true;
            return false;
        }

        [CustomRecurringFunction("Labor Day Handler", "Selects the first Monday in the month")]
        private bool LaborDayHandler(IEvent evnt, DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Monday && dt.Day <= 7 && dt.Month == evnt.Date.Month)
                return true;
            return false;
        }

        [CustomRecurringFunction("Martin Luther King Jr. Day Handler", "Selects the third Monday in the month")]
        private bool MlkDayHandler(IEvent evnt, DateTime dt)
        {
            if (dt.DayOfWeek == DayOfWeek.Monday && dt.Day > 14 && dt.Day <= 21 && dt.Month == evnt.Date.Month)
                return true;
            return false;
        }

        [CustomRecurringFunction("Memorial Day Handler", "Selects the last Monday in the month")]
        private bool MemorialDayHandler(IEvent evnt, DateTime dt)
        {
            DateTime dt2 = LastDayOfWeekInMonth(dt, DayOfWeek.Monday);
            if (dt.Month == evnt.Date.Month && dt2.Day == dt.Date.Day)
                return true;

            return false;
        }

        private DateTime LastDayOfWeekInMonth(DateTime day, DayOfWeek dow)
        {
            DateTime lastDay = new DateTime(day.Year, day.Month, 1).AddMonths(1).AddDays(-1);
            DayOfWeek lastDow = lastDay.DayOfWeek;

            int diff = dow - lastDow;

            if (diff > 0) diff -= 7;

            System.Diagnostics.Debug.Assert(diff <= 0);

            return lastDay.AddDays(diff);
        }

        private int Max(params float[] value)
        {
            return (int)value.Max(i => Math.Ceiling(i));
        }

        private bool DayForward(IEvent evnt, DateTime day)
        {
            if (evnt.ThisDayForwardOnly)
            {
                int c = DateTime.Compare(day, evnt.Date);

                if (c >= 0)
                    return true;

                return false;
            }

            return true;
        }

        internal Bitmap RequestImage()
        {
            const int cellHourWidth = 60;
            const int cellHourHeight = 30;
            var bmp = new Bitmap(ClientSize.Width, cellHourWidth * 24);
            Graphics g = Graphics.FromImage(bmp);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            var dt = new DateTime(_calendarDate.Year, _calendarDate.Month, _calendarDate.Day, 0, 0, 0);
            int xStart = 0;
            int yStart = 0;

            g.DrawRectangle(Pens.Black, 0, 0, ClientSize.Width - MarginSize * 2 - 2, cellHourHeight * 24);
            for (int i = 0; i < 24; i++)
            {
                var textWidth = (int)g.MeasureString(dt.ToString("htt").ToLower(), _dayViewTimeFont).Width;
                g.DrawRectangle(Pens.Black, xStart, yStart, cellHourWidth, cellHourHeight);
                g.DrawLine(Pens.Black, xStart + cellHourWidth, yStart + cellHourHeight,
                           ClientSize.Width - MarginSize * 2 - 3, yStart + cellHourHeight);
                g.DrawLine(Pens.DarkGray, xStart + cellHourWidth, yStart + cellHourHeight / 2,
                           ClientSize.Width - MarginSize * 2 - 3, yStart + cellHourHeight / 2);

                g.DrawString(dt.ToString("htt").ToLower(), _dayViewTimeFont, Brushes.Black, xStart + cellHourWidth - textWidth, yStart);
                yStart += cellHourHeight;
                dt = dt.AddHours(1);
            }

            dt = new DateTime(_calendarDate.Year, _calendarDate.Month, _calendarDate.Day, 23, 59, 0);

            List<IEvent> evnts = _events.Where(evnt => NeedsRendering(evnt, dt)).ToList().OrderBy(d => d.Date).ToList();

            xStart = cellHourWidth + 1;
            yStart = 0;

            g.Clip = new Region(new Rectangle(0, 0, ClientSize.Width - MarginSize * 2 - 2, cellHourHeight * 24));
            _calendarEvents.Clear();
            for (int i = 0; i < 24; i++)
            {
                dt = new DateTime(_calendarDate.Year, _calendarDate.Month, _calendarDate.Day, 0, 0, 0);
                dt = dt.AddHours(i);
                foreach (var evnt in evnts)
                {
                    TimeSpan ts = TimeSpan.FromHours(evnt.EventLengthInHours);

                    if (evnt.Date.Ticks >= dt.Ticks && evnt.Date.Ticks < dt.Add(ts).Ticks && evnt.EventLengthInHours > 0 && i >= evnt.Date.Hour)
                    {
                        int divisor = evnt.Date.Minute == 0 ? 1 : 60 / evnt.Date.Minute;
                        Color clr = Color.FromArgb(175, evnt.EventColor.R, evnt.EventColor.G, evnt.EventColor.B);
                        g.FillRectangle(new SolidBrush(GetFinalBackColor()), xStart, yStart + cellHourHeight / divisor + 1, ClientSize.Width - MarginSize * 2 - cellHourWidth - 3, cellHourHeight * ts.Hours - 1);
                        g.FillRectangle(new SolidBrush(clr), xStart, yStart + cellHourHeight / divisor + 1, ClientSize.Width - MarginSize * 2 - cellHourWidth - 3, cellHourHeight * ts.Hours - 1);
                        g.DrawString(evnt.EventText, evnt.EventFont, new SolidBrush(evnt.EventTextColor), xStart, yStart + cellHourHeight / divisor);

                        var ce = new CalendarEvent
                                     {
                                         Event = evnt,
                                         Date = dt,
                                         EventArea = new Rectangle(xStart, yStart + cellHourHeight / divisor + 1,
                                                                   ClientSize.Width - MarginSize * 2 - cellHourWidth - 3,
                                                                   cellHourHeight * ts.Hours)
                                     };
                        _calendarEvents.Add(ce);
                    }
                }
                yStart += cellHourHeight;
            }

            g.Dispose();
            return bmp;
        }

        private Color GetFinalBackColor()
        {
            Control c = this;

            while (c != null)
            {
                if (c.BackColor != Color.Transparent)
                    return c.BackColor;
                c = c.Parent;
            }

            return Color.Transparent;
        }

        private void ResizeScrollPanel()
        {
            int controlsSpacing = ((!_showTodayButton) && (!_showDateInHeader) && (!_showArrowControls)) ? 0 : 30;

            _scrollPanel.Location = new Point(MarginSize, MarginSize + controlsSpacing);
            _scrollPanel.Size = new Size(ClientSize.Width - MarginSize * 2 - 1, ClientSize.Height - MarginSize - 1 - controlsSpacing);
        }

        private void RenderDayCalendar(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if (_showDateInHeader)
            {
                SizeF dateHeaderSize = g.MeasureString(
                    _calendarDate.ToString("MMMM") + " " + _calendarDate.Day.ToString(CultureInfo.InvariantCulture) +
                    ", " + _calendarDate.Year.ToString(CultureInfo.InvariantCulture), DateHeaderFont);

                g.DrawString(
                    _calendarDate.ToString("MMMM") + " " + _calendarDate.Day.ToString(CultureInfo.InvariantCulture) +
                    ", " + _calendarDate.Year.ToString(CultureInfo.InvariantCulture),
                    _dateHeaderFont, Brushes.Black, ClientSize.Width - MarginSize - dateHeaderSize.Width,
                    MarginSize);
            }
        }

        private void RenderMonthCalendar(PaintEventArgs e)
        {
            _calendarDays.Clear();
            _calendarEvents.Clear();
            var bmp = new Bitmap(ClientSize.Width, ClientSize.Height);
            Graphics g = Graphics.FromImage(bmp);
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            SizeF sunSize = g.MeasureString("Sun", _dayOfWeekFont);
            SizeF monSize = g.MeasureString("Mon", _dayOfWeekFont);
            SizeF tueSize = g.MeasureString("Tue", _dayOfWeekFont);
            SizeF wedSize = g.MeasureString("Wed", _dayOfWeekFont);
            SizeF thuSize = g.MeasureString("Thu", _dayOfWeekFont);
            SizeF friSize = g.MeasureString("Fri", _dayOfWeekFont);
            SizeF satSize = g.MeasureString("Sat", _dayOfWeekFont);
            SizeF dateHeaderSize = g.MeasureString(
                _calendarDate.ToString("MMMM") + " " + _calendarDate.Year.ToString(CultureInfo.InvariantCulture), _dateHeaderFont);
            int headerSpacing = Max(sunSize.Height, monSize.Height, tueSize.Height, wedSize.Height, thuSize.Height, friSize.Height,
                          satSize.Height) + 5;
            int controlsSpacing = ((!_showTodayButton) && (!_showDateInHeader) && (!_showArrowControls)) ? 0 : 30;
            int cellWidth = (ClientSize.Width - MarginSize * 2) / 7;
            int numWeeks = NumberOfWeeks(_calendarDate.Year, _calendarDate.Month);
            int cellHeight = (ClientSize.Height - MarginSize * 2 - headerSpacing - controlsSpacing) / numWeeks;
            int xStart = MarginSize;
            int yStart = MarginSize;
            DayOfWeek startWeekEnum = new DateTime(_calendarDate.Year, _calendarDate.Month, 1).DayOfWeek;
            int startWeek = ((int)startWeekEnum) + 1;
            int rogueDays = startWeek - 1;

            yStart += headerSpacing + controlsSpacing;

            int counter = 1;
            int counter2 = 1;

            bool first = false;
            bool first2 = false;

            _btnToday.Location = new Point(MarginSize, MarginSize);

            for (int y = 0; y < numWeeks; y++)
            {
                for (int x = 0; x < 7; x++)
                {
                    if (rogueDays == 0 && counter <= DateTime.DaysInMonth(_calendarDate.Year, _calendarDate.Month))
                    {
                        if (!_calendarDays.ContainsKey(counter))
                            _calendarDays.Add(counter, new Point(xStart, (int)(yStart + 2f + g.MeasureString(counter.ToString(CultureInfo.InvariantCulture), _daysFont).Height)));

                        if (_calendarDate.Year == DateTime.Now.Year && _calendarDate.Month == DateTime.Now.Month
                         && counter == DateTime.Now.Day && _highlightCurrentDay)
                        {
                            g.FillRectangle(new SolidBrush(Color.FromArgb(234, 234, 234)), xStart, yStart, cellWidth, cellHeight);
                        }

                        if (first == false)
                        {
                            first = true;
                            if (_calendarDate.Year == DateTime.Now.Year && _calendarDate.Month == DateTime.Now.Month
                         && counter == DateTime.Now.Day)
                            {
                                g.DrawString(
                                    _calendarDate.ToString("MMM") + " " + counter.ToString(CultureInfo.InvariantCulture),
                                    _todayFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                            else
                            {
                                g.DrawString(
                                    _calendarDate.ToString("MMM") + " " + counter.ToString(CultureInfo.InvariantCulture),
                                    _daysFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                        }
                        else
                        {
                            if (_calendarDate.Year == DateTime.Now.Year && _calendarDate.Month == DateTime.Now.Month
                         && counter == DateTime.Now.Day)
                            {
                                g.DrawString(counter.ToString(CultureInfo.InvariantCulture), _todayFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                            else
                            {
                                g.DrawString(counter.ToString(CultureInfo.InvariantCulture), _daysFont, Brushes.Black, xStart + 5, yStart + 2);
                            }
                        }
                        counter++;
                    }
                    else if (rogueDays > 0)
                    {
                        int dm =
                            DateTime.DaysInMonth(_calendarDate.AddMonths(-1).Year, _calendarDate.AddMonths(-1).Month) -
                            rogueDays + 1;
                        g.DrawString(dm.ToString(CultureInfo.InvariantCulture), _daysFont, new SolidBrush(Color.FromArgb(170, 170, 170)), xStart + 5, yStart + 2);
                        rogueDays--;
                    }

                    g.DrawRectangle(Pens.DarkGray, xStart, yStart, cellWidth, cellHeight);
                    if (rogueDays == 0 && counter > DateTime.DaysInMonth(_calendarDate.Year, _calendarDate.Month))
                    {
                        if (first2 == false)
                            first2 = true;
                        else
                        {
                            if (counter2 == 1)
                            {
                                g.DrawString(_calendarDate.AddMonths(1).ToString("MMM") + " " + counter2.ToString(CultureInfo.InvariantCulture), _daysFont,
                                             new SolidBrush(Color.FromArgb(170, 170, 170)), xStart + 5, yStart + 2);
                            }
                            else
                            {
                                g.DrawString(counter2.ToString(CultureInfo.InvariantCulture), _daysFont,
                                             new SolidBrush(Color.FromArgb(170, 170, 170)), xStart + 5, yStart + 2);
                            }
                            counter2++;
                        }
                    }
                    xStart += cellWidth;
                }
                xStart = MarginSize;
                yStart += cellHeight;
            }
            xStart = MarginSize + ((cellWidth - (int)sunSize.Width) / 2);
            yStart = MarginSize + controlsSpacing;

            g.DrawString("Sun", _dayOfWeekFont, Brushes.Black, xStart, yStart);
            xStart = MarginSize + ((cellWidth - (int)monSize.Width) / 2) + cellWidth;
            g.DrawString("Mon", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)tueSize.Width) / 2) + cellWidth * 2;
            g.DrawString("Tue", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)wedSize.Width) / 2) + cellWidth * 3;
            g.DrawString("Wed", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)thuSize.Width) / 2) + cellWidth * 4;
            g.DrawString("Thu", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)friSize.Width) / 2) + cellWidth * 5;
            g.DrawString("Fri", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            xStart = MarginSize + ((cellWidth - (int)satSize.Width) / 2) + cellWidth * 6;
            g.DrawString("Sat", _dayOfWeekFont, Brushes.Black, xStart, yStart);

            if (_showDateInHeader)
            {
                g.DrawString(
                    _calendarDate.ToString("MMMM") + " " + _calendarDate.Year.ToString(CultureInfo.InvariantCulture),
                    _dateHeaderFont, Brushes.Black, ClientSize.Width - MarginSize - dateHeaderSize.Width,
                    MarginSize);
            }

            _events.Sort(new EventComparer());

            for (int i = 1; i <= DateTime.DaysInMonth(_calendarDate.Year, _calendarDate.Month); i++)
            {
                int renderOffsetY = 0;

                foreach (IEvent v in _events)
                {
                    var dt = new DateTime(_calendarDate.Year, _calendarDate.Month, i, 23, 59, _calendarDate.Second);
                    if (NeedsRendering(v, dt))
                    {
                        int alpha = 255;
                        if (!v.Enabled && _dimDisabledEvents)
                            alpha = 64;
                        Color alphaColor = Color.FromArgb(alpha, v.EventColor.R, v.EventColor.G, v.EventColor.B);

                        int offsetY = renderOffsetY;
                        Region r = g.Clip;
                        Point point = _calendarDays[i];
                        SizeF sz = g.MeasureString(v.EventText, v.EventFont);
                        int yy = point.Y - 1;
                        int xx = ((cellWidth - (int)sz.Width) / 2) + point.X;

                        if (sz.Width > cellWidth)
                            xx = point.X;
                        if (renderOffsetY + sz.Height > cellHeight - 10)
                            continue;
                        g.Clip = new Region(new Rectangle(point.X + 1, point.Y + offsetY, cellWidth - 1, (int)sz.Height));
                        g.FillRectangle(new SolidBrush(alphaColor), point.X + 1, point.Y + offsetY, cellWidth - 1, sz.Height);
                        if (!v.Enabled && _showDashedBorderOnDisabledEvents)
                        {
                            var p = new Pen(new SolidBrush(Color.FromArgb(255, 0, 0, 0))) { DashStyle = DashStyle.Dash };
                            g.DrawRectangle(p, point.X + 1, point.Y + offsetY, cellWidth - 2, sz.Height - 1);
                        }
                        g.DrawString(v.EventText, v.EventFont, new SolidBrush(v.EventTextColor), xx, yy + offsetY);
                        g.Clip = r;

                        var ev = new CalendarEvent
                        {
                            EventArea =
                                new Rectangle(point.X + 1, point.Y + offsetY, cellWidth - 1,
                                              (int)sz.Height),
                            Event = v,
                            Date = dt
                        };

                        _calendarEvents.Add(ev);
                        renderOffsetY += (int)sz.Height + 1;
                    }
                }
            }
            _rectangles.Clear();

            g.Dispose();
            e.Graphics.DrawImage(bmp, 0, 0, ClientSize.Width, ClientSize.Height);
            bmp.Dispose();
        }

        private bool NeedsRendering(IEvent evnt, DateTime day)
        {
            if (!evnt.Enabled && !_showDisabledEvents)
                return false;

            DayOfWeek dw = evnt.Date.DayOfWeek;

            if (evnt.RecurringFrequency == RecurringFrequencies.Daily)
            {
                return DayForward(evnt, day);
            }
            if (evnt.RecurringFrequency == RecurringFrequencies.Weekly && day.DayOfWeek == dw)
            {
                return DayForward(evnt, day);
            }
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryWeekend && (day.DayOfWeek == DayOfWeek.Saturday ||
                day.DayOfWeek == DayOfWeek.Sunday))
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryMonWedFri && (day.DayOfWeek == DayOfWeek.Monday ||
                day.DayOfWeek == DayOfWeek.Wednesday || day.DayOfWeek == DayOfWeek.Friday))
            {
                return DayForward(evnt, day);
            }
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryTueThurs && (day.DayOfWeek == DayOfWeek.Thursday ||
                day.DayOfWeek == DayOfWeek.Tuesday))
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.EveryWeekday && (day.DayOfWeek != DayOfWeek.Sunday &&
                day.DayOfWeek != DayOfWeek.Saturday))
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.Yearly && evnt.Date.Month == day.Month &&
                evnt.Date.Day == day.Day)
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.Monthly && evnt.Date.Day == day.Day)
                return DayForward(evnt, day);
            if (evnt.RecurringFrequency == RecurringFrequencies.Custom && evnt.CustomRecurringFunction != null)
            {
                if (evnt.CustomRecurringFunction(evnt, day))
                    return DayForward(evnt, day);
                return false;
            }

            if (evnt.RecurringFrequency == RecurringFrequencies.None && evnt.Date.Year == day.Year &&
                evnt.Date.Month == day.Month && evnt.Date.Day == day.Day)
                return DayForward(evnt, day);
            return false;
        }

        private int NumberOfWeeks(int year, int month)
        {
            return NumberOfWeeks(new DateTime(year, month, DateTime.DaysInMonth(year, month)));
        }

        private int NumberOfWeeks(DateTime date)
        {
            var beginningOfMonth = new DateTime(date.Year, date.Month, 1);

            while (date.Date.AddDays(1).DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek)
                date = date.AddDays(1);

            return (int)Math.Truncate(date.Subtract(beginningOfMonth).TotalDays / 7f) + 1;
        }

        private void CalendarResize(object sender, EventArgs e)
        {
            if (_calendarView == CalendarViews.Day)
                ResizeScrollPanel();
        }
    }
}