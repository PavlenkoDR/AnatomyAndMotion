using System;
using System.Collections.Generic;
using System.Text;

namespace MuscleAnatomyAndMotion.Controllers
{
    public interface IIntentController
    {
        void ShareMediaFile(string title, List<string> filesPath);
    }
}
