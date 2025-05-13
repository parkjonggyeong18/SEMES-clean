using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;

public class ZMapGenerator
{
    const int cols = 12000, rows = 4000;
    const float baseHeight = 2.1f;
    const float pcb_x_um = 250000, pcb_y_um = 77500;

    Random rand = new();
    float[,] zmap = new float[rows, cols];
    byte[,] mask = new byte[rows, cols]; // Die 영역 마스크

    public void Generate(string zmapType, int numDefects, out string csvPath, out string tifPath)
    {
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string baseName = $"pcb_zmap_{zmapType}_{numDefects}_{timestamp}";
        string outputDir = @"C:\Users\SSAFY\Desktop";

        csvPath = Path.Combine(outputDir, baseName + ".csv");
        tifPath = Path.Combine(outputDir, baseName + ".tif");

        GenerateZMap(zmapType);
        MaskDies();
        AddDefects(numDefects);
        SaveAsCsv(csvPath);
        SaveAsTif(tifPath);
    }

    private void GenerateZMap(string type)
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                zmap[y, x] = baseHeight + (float)(rand.NextDouble() * 0.01 - 0.005);

        if (type == "slope_x" || type == "slope_xy")
        {
            float a = (float)(rand.NextDouble() * 2 - 1);
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    zmap[y, x] += a * x / (float)cols;
        }

        if (type == "slope_y" || type == "slope_xy")
        {
            float b = (float)(rand.NextDouble() * 2 - 1);
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                    zmap[y, x] += b * y / (float)rows;
        }

        if (type == "curve")
        {
            float cx = (float)(rand.NextDouble() * 0.6 + 0.2);
            float cy = (float)(rand.NextDouble() * 0.6 + 0.2);
            float k = (float)(rand.NextDouble() * 0.2 + 0.1);
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < cols; x++)
                {
                    float fx = x / (float)cols - cx;
                    float fy = y / (float)rows - cy;
                    zmap[y, x] += k * (fx * fx + fy * fy);
                }
        }
    }

    private void MaskDies()
    {
        int dx_um = 10000, dy_um = 11200, gapx_um = 12000, gapy_um = 13200;
        int dx = (int)(dx_um * cols / pcb_x_um);
        int dy = (int)(dy_um * rows / pcb_y_um);
        int gapx = (int)(gapx_um * cols / pcb_x_um);
        int gapy = (int)(gapy_um * rows / pcb_y_um);

        int startx = (cols - (18 * gapx)) / 2;
        int starty = (rows - (4 * gapy)) / 2;

        for (int iy = 0; iy < 5; iy++)
            for (int ix = 0; ix < 19; ix++)
            {
                int cx = startx + ix * gapx;
                int cy = starty + iy * gapy;

                int x0 = Math.Max(0, cx - dx / 2);
                int x1 = Math.Min(cols, cx + dx / 2);
                int y0 = Math.Max(0, cy - dy / 2);
                int y1 = Math.Min(rows, cy + dy / 2);

                for (int y = y0; y < y1; y++)
                    for (int x = x0; x < x1; x++)
                    {
                        zmap[y, x] = 0;
                        mask[y, x] = 1;
                    }
            }
    }

    private void AddDefects(int num)
    {
        int tries = 0, count = 0;
        while (count < num && tries < 100000)
        {
            tries++;
            int w = rand.Next(10, 31), h = rand.Next(10, 31);
            int cx = rand.Next(w / 2, cols - w / 2);
            int cy = rand.Next(h / 2, rows - h / 2);
            int x0 = cx - w / 2, x1 = cx + w / 2;
            int y0 = cy - h / 2, y1 = cy + h / 2;

            bool conflict = false;
            for (int y = y0; y < y1 && !conflict; y++)
                for (int x = x0; x < x1 && !conflict; x++)
                    if (mask[y, x] != 0) conflict = true;
            if (conflict) continue;

            float baseMax = 0;
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                    baseMax = Math.Max(baseMax, zmap[y, x]);

            float hval = baseMax + (float)(rand.NextDouble() * 0.4 + 0.6f);
            for (int y = y0; y < y1; y++)
                for (int x = x0; x < x1; x++)
                    zmap[y, x] = hval;

            count++;
        }
    }

    private void SaveAsCsv(string path)
    {
        using var sw = new StreamWriter(path);
        for (int y = 0; y < rows; y++)
        {
            string[] line = new string[cols];
            for (int x = 0; x < cols; x++)
                line[x] = zmap[y, x].ToString("F6");
            sw.WriteLine(string.Join(",", line));
        }
    }

    private void SaveAsTif(string path)
    {
        float min = float.MaxValue, max = float.MinValue;
        foreach (var z in zmap) { min = Math.Min(min, z); max = Math.Max(max, z); }

        byte[] pixels = new byte[cols * rows];
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++)
                pixels[y * cols + x] = (byte)(((zmap[y, x] - min) / (max - min)) * 255);

        var bmp = BitmapSource.Create(cols, rows, 96, 96, PixelFormats.Gray8, null, pixels, cols);
        using var fs = new FileStream(path, FileMode.Create);
        var encoder = new TiffBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bmp));
        encoder.Save(fs);
    }
}
