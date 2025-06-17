using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Visualizes the audio spectrum as a texture and controls color/particle effects.
public class AudioSpectrum : MonoBehaviour
{
    [SerializeField] private Material material, back = default;
    [SerializeField] private AudioSource source = default;
    [SerializeField] private ParticleSystemRenderer PR;

    // Writes a binary image (0/1) to the material's main texture.
    public void WriteImage(int[,] data)
    {
        Texture2D texture = new Texture2D(data.GetLength(0), data.GetLength(1), TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < texture.height; y++)
            for (int x = 0; x < texture.width; x++)
            {
                texture.SetPixel(x, y, new Color(1, 1, 1, data[x, y]));
            }
        texture.Apply();
        Destroy(material.mainTexture);
        material.mainTexture = texture;
    }

    private void Start()
    {
        source = GetComponent<AudioSource>();
    }

    [SerializeField] int imagex = 512, imagey = 64, numBins = 32;
    private float[] frequencies;
    private void Update()
    {
        frequencies = new float[numBins];
        int[,] image = new int[imagex, imagey];
        float[] data = new float[8192];
        source.GetSpectrumData(data, 0, FFTWindow.Hanning);
        for (int i = 0; i < numBins; i++)
        {
            float t = (float)i / (numBins - 1);
            frequencies[i] = Mathf.Pow(10.0f, Mathf.Lerp(Mathf.Log10(20), Mathf.Log10(20000), t));
        }
        float[] binEnergy = new float[numBins], bindB = new float[numBins];
        int spectrumSize = data.Length;
        int binIndex = 0;

        for (int i = 0; i < spectrumSize; i++)
        {
            float frequency = AudioSettings.outputSampleRate * i / (float)spectrumSize;
            if (frequency > frequencies[binIndex])
            {
                binIndex++;
                if (binIndex == binEnergy.Length) break;
            }

            binEnergy[binIndex] += data[i] * data[i];
        }
        float maxPower = Mathf.Max(binEnergy);
        for (int i = 0; i < numBins; i++)
        {
            binEnergy[i] /= maxPower;
        }
        for (int i = 0; i < numBins; i++)
        {
            bindB[i] = 10.0f * Mathf.Log10(binEnergy[i]);
        }
        for (int i = 0; i < imagex; i++)
        {
            for (int j = 0; j < imagey; j++)
            {
                if (j <= (bindB[(i / 2) % bindB.Length]+18)/36*imagey)
                {
                    image[i, j] = 1;
                }
                else
                {
                    image[i, j] = 0;
                }
            }
            i++;
            for (int j = 0; j < imagey; j++)
            {
                image[i, j] = 0;
            }
        }
        float lerpSpeed = Time.deltaTime * 2f;
        float hue = (Array.IndexOf(bindB, bindB.Max()) - 1f) / bindB.Length;
        float saturation = 0.9f;
        float value = 0.8f;
        Color targetColor = Color.HSVToRGB(hue, saturation, value);
        back.color = Color.Lerp(back.color, targetColor, lerpSpeed);
        PR.maxParticleSize = 10.0f * Mathf.Log10(binEnergy.Sum())/100;
        WriteImage(image);
    }
}
