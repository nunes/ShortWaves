using UnityEngine;

namespace ShortWaves.MiniGame
{
    public enum WaveType
    {
        Sine,
        Square
    }

    [System.Serializable]
    public struct WaveData
    {
        public float frequency;
        public float amplitude;
        public WaveType type;
        public float phase;

        public WaveData(float frequency, float amplitude, WaveType type, float phase = 0f)
        {
            this.frequency = frequency;
            this.amplitude = amplitude;
            this.type = type;
            this.phase = phase;
        }
    }
}
