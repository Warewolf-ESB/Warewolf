plugins {
    id("com.android.application")
    kotlin("plugin.compose")
}

android {
    namespace = "warewolf.studio.android"
    compileSdk = 37

    defaultConfig {
        applicationId = "online.warewolf.studio"
        minSdk = 26
        targetSdk = 36
        versionCode = 1
        versionName = "0.1.0"
    }

    buildFeatures {
        compose = true
    }
}

kotlin {
    jvmToolchain(21)
}

dependencies {
    implementation(project(":shared:app"))
    implementation("androidx.activity:activity-compose:1.13.0")
}
