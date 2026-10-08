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
            var progressDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "BISTracker");
            var progressPath = ProgressFilePaths.ClassicPhaseOne(progressDirectory);
            var hasDraftProgress = File.Exists(ProgressFilePaths.Draft(progressDirectory)) && !File.Exists(progressPath);
            var catalogDirectory = Path.Combine(progressDirectory, "Catalogs");
            ICatalogPackImporter? importer = new CatalogPackImporter(catalogDirectory);
            ICharacterTrackerService service = new CharacterTrackerService(new CharacterCatalog(catalogDirectory),
                new JsonWorkspaceRepository(ProgressFilePaths.Characters(progressDirectory)), new JsonProgressRepository(progressPath));
#if DRAFT_PREVIEW
            if (Common.DraftPreview.IsRequested)
            {
                service = new CharacterTrackerService(Common.DraftPreview.Catalog, Common.DraftPreview.Workspace, new Common.PreviewProgressRepository());
                hasDraftProgress = false;
                importer = Common.DraftPreview.Importer;
            }
#endif
            _window = new MainWindow(new TrackerViewModel(service, hasDraftProgress, importer));
            _window.Activate();
        }
    }
}
