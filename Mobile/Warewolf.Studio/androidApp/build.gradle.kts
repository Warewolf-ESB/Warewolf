plugins {
    id("com.android.application")
    kotlin("plugin.compose")
}

// CI (Dev/.azure/pipeline.yml, Compile_Release) passes the release version and a code that always
// grows (-PwwVersionName / -PwwVersionCode). Local builds keep the defaults.
val wwVersionName = providers.gradleProperty("wwVersionName").orElse("0.1.0").get()
val wwVersionCode = providers.gradleProperty("wwVersionCode").orElse("1").get().toInt()

// The release signing key is read from the environment, never from the repo. All four must be set,
// or the release build stays unsigned (local builds are unchanged).
val releaseKeystore = providers.environmentVariable("WW_ANDROID_KEYSTORE").orNull
val releaseKeystorePassword = providers.environmentVariable("WW_ANDROID_KEYSTORE_PASSWORD").orNull
val releaseKeyAlias = providers.environmentVariable("WW_ANDROID_KEY_ALIAS").orNull
val releaseKeyPassword = providers.environmentVariable("WW_ANDROID_KEY_PASSWORD").orNull
val hasReleaseKey = listOf(releaseKeystore, releaseKeystorePassword, releaseKeyAlias, releaseKeyPassword)
    .all { !it.isNullOrBlank() }

android {
    namespace = "warewolf.studio.android"
    compileSdk = 37

    defaultConfig {
        applicationId = "online.warewolf.studio"
        minSdk = 26
        targetSdk = 36
        versionCode = wwVersionCode
        versionName = wwVersionName
    }

    signingConfigs {
        if (hasReleaseKey) {
            create("release") {
                storeFile = file(releaseKeystore!!)
                storePassword = releaseKeystorePassword
                keyAlias = releaseKeyAlias
                keyPassword = releaseKeyPassword
            }
        }
    }

    buildTypes {
        release {
            // No R8 yet: Ktor and kotlinx-serialization have no keep rules here.
            isMinifyEnabled = false
            if (hasReleaseKey) {
                signingConfig = signingConfigs.getByName("release")
            }
        }
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
