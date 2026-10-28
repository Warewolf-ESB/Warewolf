rootProject.name = "warewolf-studio"

pluginManagement {
    repositories {
        gradlePluginPortal()
        mavenCentral()
    }
}

dependencyResolutionManagement {
    repositories {
        mavenCentral()
    }
}

// Plain Kotlin/JVM for now (Plan.md, System 1 baseline). The module moves under a Kotlin
// Multiplatform build once the Android and iOS hosts exist.
include(":shared:generator")
// Desktop-only System 1 evaluation harness; never shipped in the app.
include(":eval")
