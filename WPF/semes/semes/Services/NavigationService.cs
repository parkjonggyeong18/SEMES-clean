using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace semes.Services
{
    internal class NavigationService
    {
        private readonly Frame _mainFrame;

        public NavigationService(Frame mainFrame)
        {
            _mainFrame = mainFrame;
        }

        public void NavigateToDashboard()
        {
            _mainFrame.Navigate(new DashboardPage());
        }

        public void NavigateToDefectDetetcion()
        {
            _mainFrame.Navigate(new DefectDetectionPage());
        }

        public void NavigateToDefectStats()
        {
            _mainFrame.Navigate(new DefectStatsPage());
        }
    }
}
