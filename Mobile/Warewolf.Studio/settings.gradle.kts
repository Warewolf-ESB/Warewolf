rootProject.name = "warewolf-studio"

pluginManagement {
    repositories {
        google()
        gradlePluginPortal()
        mavenCentral()
    }
}

dependencyResolutionManagement {
    repositories {
        google()
        mavenCentral()
    }
}

// System 1 baseline (Plan.md): plain Kotlin/JVM, deprecated, kept for comparison runs.
include(":shared:generator")
// Desktop-only System 1 evaluation harness; never shipped in the app.
include(":eval")
// The app (Plan.md, P2): shared Kotlin Multiplatform code and Compose screens (Android, iOS, desktop)...
include(":shared:app")
// ...and the Android host. The iOS host is an Xcode project built on a Mac (README.md).
include(":androidApp")
