import SwiftUI
import WarewolfStudio

/// The iOS app: the shared Compose UI (shared/app, MainViewController.kt), full screen.
@main
struct WarewolfStudioApp: App {
    var body: some Scene {
        WindowGroup {
            // The Compose screens keep clear of the notch and home indicator themselves (safeDrawingPadding).
            SharedAppView().ignoresSafeArea()
        }
    }
}

private struct SharedAppView: UIViewControllerRepresentable {
    func makeUIViewController(context: Context) -> UIViewController {
        MainViewControllerKt.MainViewController()
    }

    func updateUIViewController(_ uiViewController: UIViewController, context: Context) {}
}
