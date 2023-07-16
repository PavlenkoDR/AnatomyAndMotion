using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MuscleAnatomyAndMotion.App;

namespace MuscleAnatomyAndMotion
{
    public interface IMainFlyoutPageFlyoutMenuItem
    {

    }
    public class MainFlyoutPageFlyoutMenuItem : IMainFlyoutPageFlyoutMenuItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class MainFlyoutPageFlyoutMenuItemAnatomy : IMainFlyoutPageFlyoutMenuItem
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public BodyPartID bodyPartID { get; set; }
        public float xOffset { get; set; }
        public float ContentScale { get; set; }
    }
}