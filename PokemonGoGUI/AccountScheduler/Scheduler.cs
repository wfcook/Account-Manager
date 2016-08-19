using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace PokemonGoGUI.AccountScheduler
{
    public class Scheduler
    {
        public delegate void CheckHandler(object sender, SchedulerEventArgs e);
        public event CheckHandler OnSchedule;

        private const int Minute = 60000;

        public string Name { get; set; }
        public bool Enabled { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public double OffSet { get; set; }

        public SchedulerLimiter PokeStoplimiter { get; set; }
        public SchedulerLimiter PokemonLimiter { get; set; }

        private Timer _timer = new Timer(Minute * 5);

        public int CheckTime
        {
            get
            {
                return _checkTime / Minute;
            }
            set
            {
                _checkTime = value * Minute;

                _timer.Interval = _checkTime;
            }

        }

        private int _checkTime = Minute * 5;

        public Scheduler()
        {
            Name = "Change Name";
            PokeStoplimiter = new SchedulerLimiter();
            PokemonLimiter = new SchedulerLimiter();

            _timer = new Timer(_checkTime);
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;

            _timer.Start();
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if(Enabled)
            {
                CheckCaller();
            }
        }

        private void CheckCaller()
        {
            CheckHandler caller = OnSchedule;

            if (caller != null)
            {
                caller(this, new SchedulerEventArgs(this));
            }
        }

    }

    public class SchedulerEventArgs : EventArgs
    {
        public Scheduler Scheduler { get; set; }

        public SchedulerEventArgs(Scheduler scheduler)
        {
            Scheduler = scheduler;
        }
    }
}
