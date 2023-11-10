void NStep_float(in float x, in int N, out float value) {
    // Calculate the width of each step
    float stepWidth = 1.0 / float(N);

    // Determine which step we are currently in
    int stepIndex = int(x / stepWidth);

    // Normalize the step index to be in the range [0,1]
    float normalizedStepValue = float(stepIndex) / float(N - 1);

    value =  normalizedStepValue;
}
