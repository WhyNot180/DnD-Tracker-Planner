using System.Windows.Controls;
using System.Windows.Shapes;

namespace Dungeons_and_Dragons_Tracker_Planner
{
    internal class PathPair
    {

        public Path path;

        public Grid primaryGrid;

        public Grid secondaryGrid;

        public PathPair(Path path, Grid primaryGrid, Grid secondaryGrid) 
        {
            this.path = path;
            this.primaryGrid = primaryGrid;
            this.secondaryGrid = secondaryGrid;
        }
    }
}