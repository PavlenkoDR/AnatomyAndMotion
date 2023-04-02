using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuscleAnatomyAndMotion
{
    public interface IMainFlyoutPageFlyoutMenuItem
    {

    }
    public class MainFlyoutPageFlyoutMenuItemMuscleInfo : IMainFlyoutPageFlyoutMenuItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class MainFlyoutPageFlyoutMenuItemAnatomy : IMainFlyoutPageFlyoutMenuItem
    {
        public int Id { get; set; }
        public string Title { get; set; }

        public string muscleAreaName { get; set; }
        public int maxLayer { get; set; }
        public float xOffset { get; set; }
        public float ContentScale { get; set; }
    }
}