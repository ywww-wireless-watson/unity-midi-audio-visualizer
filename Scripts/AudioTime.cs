using UnityEngine;

// Visualizes audio waveform data as a texture in real time.
// Attach this script to a GameObject with an AudioSource and assign a Material.
public class AudioTime : MonoBehaviour
{
    [SerializeField] private Material material = null; // Material to update with the waveform texture
    [SerializeField] private AudioSource source = null; // AudioSource to read waveform data from
    [SerializeField] private int imagex = 4096; // Texture width
    [SerializeField] private int imagey = 64;   // Texture height
    private float[] data; // Buffer for audio data

    // Writes a binary image (0/1) to the material's main texture.
    public void WriteImage(int[,] data)
    {
        Texture2D texture = new Texture2D(data.GetLength(0), data.GetLength(1), TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < texture.height; y++)
            for (int x = 0; x < texture.width; x++)
                texture.SetPixel(x, y, new Color(1, 1, 1, data[x, y]));
        texture.Apply();
        if (material.mainTexture != null)
            Destroy(material.mainTexture);
        material.mainTexture = texture;
    }

    private void Start()
    {
        if (source == null) source = GetComponent<AudioSource>();
        data = new float[16384];
    }

    private void Update()
    {
        if (source == null || material == null) return;
        source.GetOutputData(data, 0);
        int[,] image = new int[imagex, imagey];
        float amp = 1.5f;
        for (int i = 0; i < imagex && i < data.Length; i++)
        {
            for (int j = 0; j < imagey; j++)
            {
                float center = imagey / 2f;
                float pos = center + data[i] * imagey * amp;
                if ((data[i] > 0 && j < pos && j >= center) || (data[i] < 0 && j > pos && j <= center))
                    image[i, j] = 1;
                else
                    image[i, j] = 0;
            }
        }
        WriteImage(image);
    }
}
