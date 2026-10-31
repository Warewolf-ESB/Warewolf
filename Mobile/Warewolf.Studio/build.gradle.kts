// Plugin versions live here once, so every module (JVM, multiplatform, Android) shares one Kotlin.
plugins {
    kotlin("jvm") version "2.4.10" apply false
    kotlin("multiplatform") version "2.4.10" apply false
    kotlin("plugin.serialization") version "2.4.10" apply false
    kotlin("plugin.compose") version "2.4.10" apply false
    id("org.jetbrains.compose") version "1.12.2" apply false
    id("com.android.application") version "9.5.0" apply false
    id("com.android.kotlin.multiplatform.library") version "9.5.0" apply false
}
