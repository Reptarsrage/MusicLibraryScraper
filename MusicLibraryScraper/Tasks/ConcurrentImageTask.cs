

namespace MusicLibraryScraper.Tasks
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    public class ConcurrentImageTask : Task<Image>
    {
        private object lockMe;

        public ConcurrentImageTask(Func<Image> func) : base(func)
        {
            lockMe = new object();
        }

        public new Image Result
        {
            get
            {
                lock (lockMe)
                {
                    if (base.Result != null)
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
