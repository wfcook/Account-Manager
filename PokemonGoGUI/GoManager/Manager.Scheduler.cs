using PokemonGoGUI.AccountScheduler;
using PokemonGoGUI.Enums;
using PokemonGoGUI.GoManager.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PokemonGoGUI.GoManager
{
    public partial class Manager
    {
        private DateTime _lastTrackerUpdate = new DateTime();

        public void AddSchedulerEvent()
        {
            if (AccountScheduler != null)
            {
                AccountScheduler.OnSchedule -= scheduler_OnSchedule;
                AccountScheduler.OnSchedule += scheduler_OnSchedule;
            }
        }

        public void AddScheduler(Scheduler scheduler)
        {
            if(AccountScheduler != null)
            {
                RemoveScheduler();
            }

            scheduler.OnSchedule += scheduler_OnSchedule;

            AccountScheduler = scheduler;
        }

        public void RemoveScheduler()
        {
            if(AccountScheduler != null)
            {
                AccountScheduler.OnSchedule -= scheduler_OnSchedule;
            }

            AccountScheduler = null;
        }

        private async void scheduler_OnSchedule(object sender, SchedulerEventArgs e)
        {
            DateTime currentTime = Tracker.GetCurrentHourDateTime();

            if (currentTime != _lastTrackerUpdate)
            {
                _lastTrackerUpdate = currentTime;

                Tracker.CalculatedTrackingHours();
            }

            Scheduler scheduler = e.Scheduler;

            //Allowing event to be called to update tracked hours when not running
            if(!scheduler.Enabled)
            {
                return;
            }

            //Don't auto start when max level is hit
            if(UserSettings.MaxLevel != 0 && Level >= UserSettings.MaxLevel)
            {
                return;
            }

            //Should not start with these states
            if(AccountState == AccountState.PermAccountBan || AccountState == AccountState.NotVerified)
            {
                return;
            }

            int delay = 0;

            lock(_rand)
            {
                delay = _rand.Next(0, 45000);
            }

            if (e.Scheduler.WithinTime())
            {
                if (State == Enums.BotState.Stopped)
                {
                    //Only auto start when both are below min values
                    //Otherwise we'll get constant start/stops
                    if ((PokemonCaught <= scheduler.PokemonLimiter.Min || scheduler.PokemonLimiter.Option == SchedulerOption.Nothing) &&
                        (PokestopsFarmed <= scheduler.PokeStoplimiter.Min || scheduler.PokeStoplimiter.Option == SchedulerOption.Nothing))
                    {
                        LogCaller(new LoggerEventArgs(String.Format("Auto starting (schedule) in {0} seconds...", delay/1000), LoggerTypes.Debug));

                        await Task.Delay(delay);

                        Start();
                    }
                }
            }
            else
            {
                if (State != Enums.BotState.Stopping && State != Enums.BotState.Stopped)
                {
                    LogCaller(new LoggerEventArgs("Auto stopping (schedule) ...", LoggerTypes.Debug));
                    Stop();
                }
            }


            if (!IsRunning)
            {
                return;
            }

            //Master stop
            if (scheduler.MasterOption == SchedulerOption.StartStop)
            {
                if (State != BotState.Stopping && State != BotState.Stopped)
                {
                    if (PokemonCaught >= scheduler.PokemonLimiter.Max && PokestopsFarmed >= scheduler.PokeStoplimiter.Max)
                    {
                        LogCaller(new LoggerEventArgs("Max pokemon and pokestop limit reached. Stopping", LoggerTypes.Debug));
                        Stop();

                        return;
                    }
                }
            }

            //Pokemon
            if (scheduler.PokemonLimiter.Option != SchedulerOption.Nothing)
            {
                if (PokemonCaught >= scheduler.PokemonLimiter.Max)
                {

                    switch (scheduler.PokemonLimiter.Option)
                    {
                        case SchedulerOption.DisableEnable: //No extra checks
                            if(UserSettings.CatchPokemon)
                            {
                                LogCaller(new LoggerEventArgs("Max pokemon limit reached. Disabling setting...", LoggerTypes.Debug));
                                UserSettings.CatchPokemon = false;
                                UserSettings.SnipePokemon = false;
                            }
                            break;
                        case SchedulerOption.StartStop: //Just stop it
                            LogCaller(new LoggerEventArgs("Max pokemon limit reached. Stopping bot...", LoggerTypes.Debug));
                            Stop();
                            break;
                    }
                }
                else if (PokemonCaught <= scheduler.PokemonLimiter.Min)
                {
                    switch (scheduler.PokemonLimiter.Option)
                    {
                        case SchedulerOption.DisableEnable: //No extra checks
                            if (!UserSettings.CatchPokemon)
                            {
                                LogCaller(new LoggerEventArgs("Min pokemon limit reached. Enabling catching...", LoggerTypes.Debug));
                                UserSettings.CatchPokemon = true;
                                UserSettings.SnipePokemon = true;
                            }
                            break;
                        case SchedulerOption.StartStop: //Start only if pokestop is disabled/nothing or pokestops below threshold
                            if (scheduler.PokeStoplimiter.Option != SchedulerOption.StartStop ||
                                PokestopsFarmed <= scheduler.PokeStoplimiter.Min)
                            {
                                if (State == BotState.Stopped)
                                {
                                    LogCaller(new LoggerEventArgs(String.Format("Min pokemon limit reached. Starting in {0} seconds", delay/1000), LoggerTypes.Debug));

                                    await Task.Delay(delay);

                                    Start();
                                }
                            }
                            break;
                    }
                }
            }

            //Pokestops
            if (scheduler.PokeStoplimiter.Option != SchedulerOption.Nothing)
            {
                if (PokestopsFarmed >= scheduler.PokeStoplimiter.Max)
                {
                    switch (scheduler.PokeStoplimiter.Option)
                    {
                        case SchedulerOption.DisableEnable: //No extra checks
                            if (UserSettings.SearchFortBelowPercent != 0)
                            {
                                LogCaller(new LoggerEventArgs("Max pokestop limit reached. Disabling...", LoggerTypes.Debug));
                                UserSettings.SearchFortBelowPercent = 0;
                            }
                            break;
                        case SchedulerOption.StartStop: //Just stop it
                            LogCaller(new LoggerEventArgs("Max pokestop limit reached. Stopping ...", LoggerTypes.Debug));
                            Stop();
                            break;
                    }
                }
                else if (PokestopsFarmed <= scheduler.PokeStoplimiter.Min)
                {
                    switch (scheduler.PokeStoplimiter.Option)
                    {
                        case SchedulerOption.DisableEnable: //No extra checks
                            if (UserSettings.SearchFortBelowPercent != 1000)
                            {
                                LogCaller(new LoggerEventArgs("Min pokestop limit reached. Enabling ...", LoggerTypes.Debug));
                                UserSettings.SearchFortBelowPercent = 1000;
                            }
                            break;
                        case SchedulerOption.StartStop: //Start only if pokemon is disabled/nothing or pokemon caught below threshold
                            if (scheduler.PokemonLimiter.Option != SchedulerOption.StartStop ||
                                PokemonCaught <= scheduler.PokemonLimiter.Min)
                            {
                                if (State == BotState.Stopped)
                                {
                                    LogCaller(new LoggerEventArgs(String.Format("Min pokestop limit reached. Starting in {0} seconds", delay/1000), LoggerTypes.Debug));

                                    await Task.Delay(delay);

                                    Start();
                                }
                            }
                            break;
                    }
                }
            }
        }
    }
}
