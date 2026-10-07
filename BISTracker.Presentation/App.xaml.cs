using Microsoft.UI.Xaml;
using System;
using System.IO;
using BISTracker.Application;
using BISTracker.Infrastructure;
using BISTracker.Presentation.Features.Tracking.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace BISTracker.Presentation
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Microsoft.UI.Xaml.Application
    {
        private Window? _window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var progressPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BISTracker", "draft-progress.json");
            var service = new TrackerService(new SampleBisCatalog(), new JsonProgressRepository(progressPath));
#if DRAFT_PREVIEW
            if (Common.DraftPreview.IsRequested)
                service = new TrackerService(new SampleBisCatalog(), new Common.PreviewProgressRepository());
#endif
            _window = new MainWindow(new TrackerViewModel(service));
            _window.Activate();
        }
    }
}
