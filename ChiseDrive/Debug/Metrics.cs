using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ChiseDrive.Debug
{
    public class Metric
    {
        public readonly String Name;

        // These are the Metrics of importance
        public double AverageTimeTicks
        {
            get
            {
                return totaltime.Ticks / (double)count;
            }
        }
        public int TotalCalls
        {
            get
            {
                return count;
            }
        }
        public TimeSpan TotalTime
        {
            get
            {
                return totaltime;
            }
        }
        public TimeSpan BiggestPeak
        {
            get
            {
                return longesttime;
            }
        }

        int count;
        //long openticks;
        //long peakticks;
        
        TimeSpan totaltime;
        TimeSpan longesttime;
        //DateTime opentime;

        Stopwatch stopwatch;

        public Metric(String name)
        {
            this.Name = name;
            stopwatch = new Stopwatch();
            Reset();
        }

        public Metric Clone()
        {
            Metric copy = new Metric(this.Name);

            copy.count = this.count;
            //copy.peakticks = this.peakticks;

            
            //copy.opentime = this.opentime;
            copy.longesttime = this.longesttime;
            copy.totaltime = this.totaltime;
            

            return copy;
        }

        public void Reset()
        {
            count = 0;
            stopwatch.Reset();

            totaltime = TimeSpan.Zero;
            longesttime = TimeSpan.Zero;
            //opentime = DateTime.Now;
        }

        public void Open()
        {
            stopwatch.Reset();
            stopwatch.Start();

            //this.opentime = DateTime.Now;
        }

        public void Close()
        {
            stopwatch.Stop();

            totaltime += stopwatch.Elapsed;

            //long elapsed = stopwatch.ElapsedTicks - openticks;
            //peakticks = peakticks > elapsed ? peakticks : elapsed;

            count++;

            longesttime = longesttime > stopwatch.Elapsed ? longesttime : stopwatch.Elapsed;

            /*
            long elapsedticks = stopwatch.ElapsedTicks - startticks;
            TimeSpan timespan = DateTime.Now - opentime;

            count++;
            totaltime += timespan;
            longesttime = longesttime > timespan ? longesttime : timespan;
            */
        }
    }

    public class Metrics
    {
        static List<Metric> data = new List<Metric>();

        /// <summary>
        /// Set to true to run metrics, set to false to skip
        /// </summary>
        static public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }
        static bool active = false;

        static Metric FindMetric(String name)
        {
            foreach (Metric m in data)
            {
                if (m.Name == name) return m;
            }

            Metric temp = new Metric(name);
            data.Add(temp);
            return temp;
        }

        /// <summary>
        /// Opens a metric for study.
        /// </summary>
        /// <param name="name"></param>
        static public void OpenMetric(String name)
        {
            if (!active) return;
            FindMetric(name).Open();
        }

        /// <summary>
        /// Closes the open metric and timestamps.
        /// </summary>
        /// <param name="name"></param>
        static public void CloseMetric(String name)
        {
            if (!active) return;
            FindMetric(name).Close();
        }

        /// <summary>
        /// Clears all the metric time data (good when you want instant metrics, and not over time metrics)
        /// </summary>
        static public void ResetAll()
        {
            foreach (Metric m in data)
            {
                m.Reset();
            }
        }

        public const int TotalTime = 0;
        public const int PeakTime = 1;

        /// <summary>
        /// Returns the top offenders list.
        /// </summary>
        static public List<Metric> TopOffenders(int listsize, int sortby)
        {
            if (!active) return null;

            switch (sortby)
            {
                case TotalTime:
                    {
                        data.Sort(delegate(Metric value1, Metric value2)
                        {
                            return value1.TotalTime.CompareTo(value2.TotalTime);
                        });
                        data.Reverse();
                    }
                    break;
                case PeakTime:
                    {
                        data.Sort(delegate(Metric value1, Metric value2)
                        {
                            return value1.BiggestPeak.CompareTo(value2.BiggestPeak);
                        });
                        data.Reverse();
                    }
                    break;
            }
            List<Metric> returnlist = new List<Metric>();
            for (int i = 0; i < listsize && i < data.Count; i++)
            {
                returnlist.Add(data[i].Clone());
            }
            return returnlist;
        }
    }
}