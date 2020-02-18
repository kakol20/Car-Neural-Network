using UnityEngine;

public class BetterColor
{
    public float A;
    public float H, S, V;
    public float R, G, B;

    public BetterColor()
    {
        R = 0f;
        G = 0f;
        B = 0f;

        RGBtoHSV();
    }

    public BetterColor(float R, float G, float B)
    {
        this.R = R;
        this.G = G;
        this.B = B;
        this.A = 1f;

        RGBtoHSV();
    }

    public BetterColor(Color color)
    {
        R = color.r;
        G = color.g;
        B = color.b;
        A = color.a;

        RGBtoHSV();
    }

    public BetterColor(BetterColor copyColor)
    {
        R = copyColor.R;
        G = copyColor.G;
        B = copyColor.B;
        A = copyColor.A;

        RGBtoHSV();
    }

    public Color GetColor()
    {
        return new Color(R, G, B, A);
    }

    public void HSVtoRGB()
    {
        Color temp = Color.HSVToRGB(H, S, V);

        R = temp.r;
        G = temp.g;
        B = temp.b;
    }

    public void MixColors(BetterColor color1, BetterColor color2)
    {
        float difference = Mathf.Abs(color1.H - color2.H);
        H = (color1.H + color2.H) / 2f;

        if (difference > 0.5f)
        {
            H += 0.5f;
        }

        WrapHue();

        HSVtoRGB();
    }

    public void ModifyColor()
    {
        WrapHue();

        S = 0.75f;
        V = 1f;

        HSVtoRGB();
    }

    public void MutateColor()
    {
        float rand = Random.Range(0f, 16f);

        if (rand < 2f)
        {
            H *= -1f;
        }
        else if (rand < 4f)
        {
            H *= Random.Range(1f, 2f);
        }
        else if (rand < 6f)
        {
            H *= Random.Range(0f, 1f);
        }
        else if (rand < 8f)
        {
            H = Random.Range(0f, 1f);
        }

        WrapHue();

        HSVtoRGB();
    }

    private void RGBtoHSV()
    {
        Color temp = new Color(R, G, B);

        Color.RGBToHSV(temp, out H, out S, out V);
    }

    private void WrapHue()
    {
        if (H > 1f)
        {
            while (H > 1f) H -= 1f;
        }
        else if (H < 0f)
        {
            while (H < 0f) H += 1f;
        }
    }
}