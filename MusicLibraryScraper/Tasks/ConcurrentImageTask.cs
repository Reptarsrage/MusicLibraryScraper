/// <summary>
/// Author: Justin Robb
/// Date: 9/25/2016
/// 
/// Project Description:
/// Adds album art to each file in a library of music using online image sources.
/// 
/// </summary>

namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.Drawing;

    public class ConcurrentImageTask : BaseTask<Image>
    {
        private object lockMe;

        public ConcurrentImageTask(Func<Image> func) : base(func)
        {
            lockMe = new object();
        }

        public virtual new Image Result
        {
            get
            {
                lock (lockMe)
                {
                    if (base.Exception == null && base.Result != null)
                    {
                        return (Image)base.Result.Clone();
                    }
                    else
                    {
                        return null;
                    }
                }
            }

        }
    }
}
