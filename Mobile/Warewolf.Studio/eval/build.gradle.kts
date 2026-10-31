plugins {
    kotlin("jvm")
    application
}

kotlin {
    jvmToolchain(21)
}

dependencies {
    implementation(project(":shared:generator"))
    implementation("org.jetbrains.kotlinx:kotlinx-serialization-json:1.8.1")
    // Runs the trained System 1 classifier on the desktop; the phones get their own ONNX Runtime builds.
    implementation("com.microsoft.onnxruntime:onnxruntime:1.31.1")
    // Checks generated input forms against the JSON Schema 2020-12 meta-schema (bakeoff forms).
    implementation("com.networknt:json-schema-validator:1.5.6")
    testImplementation(kotlin("test"))
}

application {
    mainClass.set("warewolf.studio.eval.MainKt")
}

tasks.test {
    useJUnitPlatform()
}
