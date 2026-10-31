plugins {
    kotlin("multiplatform")
    kotlin("plugin.serialization")
    kotlin("plugin.compose")
    id("org.jetbrains.compose")
    id("com.android.kotlin.multiplatform.library")
}

kotlin {
    jvmToolchain(21)

    android {
        namespace = "warewolf.studio.app"
        compileSdk = 37
        minSdk = 26
    }

    // Desktop: runs the shared tests, and the app itself on a developer's machine (README.md).
    jvm("desktop")

    listOf(iosArm64(), iosSimulatorArm64()).forEach { target ->
        target.binaries.framework {
            baseName = "WarewolfStudio"
            isStatic = true
        }
    }

    sourceSets {
        commonMain.dependencies {
            // api: the platform hosts (androidApp, iOS) build their own screens on these.
            api("org.jetbrains.compose.runtime:runtime:1.12.2")
            api("org.jetbrains.compose.foundation:foundation:1.12.2")
            api("org.jetbrains.compose.ui:ui:1.12.2")
            implementation("org.jetbrains.compose.material3:material3:1.9.0")
            implementation("io.ktor:ktor-client-core:3.6.1")
            implementation("org.jetbrains.kotlinx:kotlinx-coroutines-core:1.11.0")
            implementation("org.jetbrains.kotlinx:kotlinx-serialization-json:1.12.0")
        }
        commonTest.dependencies {
            implementation(kotlin("test"))
            implementation("io.ktor:ktor-client-mock:3.6.1")
            implementation("org.jetbrains.kotlinx:kotlinx-coroutines-test:1.11.0")
        }
        androidMain.dependencies {
            implementation("io.ktor:ktor-client-okhttp:3.6.1")
        }
        named("desktopMain").dependencies {
            implementation(compose.desktop.currentOs)
            implementation("io.ktor:ktor-client-java:3.6.1")
            implementation("org.jetbrains.kotlinx:kotlinx-coroutines-swing:1.11.0")
        }
        iosMain.dependencies {
            implementation("io.ktor:ktor-client-darwin:3.6.1")
        }
    }
}

compose.desktop {
    application {
        mainClass = "warewolf.studio.app.MainKt"
    }
}
