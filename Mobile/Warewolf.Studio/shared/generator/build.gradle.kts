plugins {
    kotlin("jvm") version "2.4.10"
}

kotlin {
    jvmToolchain(21)
}

dependencies {
    testImplementation(kotlin("test"))
    // Reads the tokenizer's golden fixture (wordpiece-golden.json).
    testImplementation("org.jetbrains.kotlinx:kotlinx-serialization-json:1.8.1")
}

tasks.test {
    useJUnitPlatform()
}
