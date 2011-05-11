using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RebuildFileLists
{
    internal class Breakdown
    {
        public long Known = 0;
        public long Total = 0;

        public int Percent
        {
            get
            {
                if (this.Total == 0)
                {
                    return 0;
                }

                return (int)Math.Floor((
                    (float)this.Known /
                    (float)this.Total) * 100.0);
            }
        }

        public override string ToString()
        {
            return string.Format("{0}/{1} ({2}%)",
                this.Known,
                this.Total,
                this.Percent);
        }
    }
}
