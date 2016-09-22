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

        public new Image Result
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
