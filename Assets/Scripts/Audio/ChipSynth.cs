using UnityEngine;

// Tiny NES-style synthesizer: square/pulse/triangle/noise tones rendered into float buffers.
public static class ChipSynth
{
    public const int SampleRate = 22050;

    public enum Wave { Square, Pulse25, Triangle, Noise }

    public static float[] Tone(float freq, float seconds, Wave wave, float volume,
        float endFreq = -1f, float decayPower = 1f)
    {
        int count = Mathf.Max(1, (int)(seconds * SampleRate));
        var data = new float[count];
        float phase = 0f;
        float attack = 0.004f * SampleRate;
        float noiseValue = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / count;
            float f = endFreq > 0f ? Mathf.Lerp(freq, endFreq, t) : freq;
            phase += f / SampleRate;
            if (phase >= 1f)
            {
                phase -= 1f;
                if (wave == Wave.Noise) noiseValue = Random.value * 2f - 1f;
            }

            float sample;
            switch (wave)
            {
                case Wave.Square: sample = phase < 0.5f ? 1f : -1f; break;
                case Wave.Pulse25: sample = phase < 0.25f ? 1f : -1f; break;
                case Wave.Triangle: sample = Mathf.Abs(phase * 4f - 2f) - 1f; break;
                default: sample = noiseValue; break;
            }

            float envelope = Mathf.Min(1f, i / attack) * Mathf.Pow(1f - t, decayPower);
            data[i] = sample * envelope * volume;
        }
        return data;
    }

    public static float[] Silence(float seconds) => new float[Mathf.Max(1, (int)(seconds * SampleRate))];

    public static float[] Concat(params float[][] parts)
    {
        int total = 0;
        foreach (var p in parts) total += p.Length;
        var result = new float[total];
        int offset = 0;
        foreach (var p in parts)
        {
            System.Array.Copy(p, 0, result, offset, p.Length);
            offset += p.Length;
        }
        return result;
    }

    // Sum src into dest starting at offset (clipped to dest length).
    public static void Mix(float[] dest, float[] src, int offset)
    {
        for (int i = 0; i < src.Length && offset + i < dest.Length; i++)
            dest[offset + i] += src[i];
    }

    public static float MidiToFreq(int midi) => 440f * Mathf.Pow(2f, (midi - 69) / 12f);

    // Renders a looping two-voice track. Each array entry is one eighth note: a MIDI note, or 0 for a rest.
    public static float[] Track(int[] lead, int[] bass, float bpm)
    {
        int steps = Mathf.Max(lead.Length, bass.Length);
        int stepSamples = (int)(60f / bpm / 2f * SampleRate);
        var buffer = new float[steps * stepSamples];

        for (int i = 0; i < steps; i++)
        {
            if (i < lead.Length && lead[i] > 0)
                Mix(buffer, Tone(MidiToFreq(lead[i]), stepSamples * 0.9f / SampleRate, Wave.Pulse25, 0.12f, -1f, 0.4f),
                    i * stepSamples);
            if (i < bass.Length && bass[i] > 0)
                Mix(buffer, Tone(MidiToFreq(bass[i]), stepSamples * 0.95f / SampleRate, Wave.Triangle, 0.22f, -1f, 0.3f),
                    i * stepSamples);
        }
        return buffer;
    }
}
