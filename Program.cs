using System;
using System.IO;
using ImageMagick;

namespace ImageMagickExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string gsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gs");
            MagickNET.SetGhostscriptDirectory(gsDirectory);

            string inputPdfPath = args[0];
            string inputPdfName = Path.GetFileNameWithoutExtension(inputPdfPath);
            string inputPdfDirectory = Path.GetDirectoryName(inputPdfPath);

            // Carpeta de salida para las imágenes en la carpeta de la aplicación
            string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string jpegOutputPath = Path.Combine(executableDirectory, inputPdfName);

            Directory.CreateDirectory(jpegOutputPath);

            MagickReadSettings settings = new MagickReadSettings();
            settings.Density = new Density(150);

            using (MagickImageCollection images = new MagickImageCollection())
            {
                images.Read(inputPdfPath, settings);

                int pageNumber = 1;
                foreach (MagickImage image in images)
                {
                    string outputImagePath = Path.Combine(jpegOutputPath, $"output-{pageNumber:D3}.jpg");
                    image.Alpha(AlphaOption.Remove);
                    image.Quality = 10;
                    image.Write(outputImagePath);
                    pageNumber++;
                }
            }

            using (MagickImageCollection grayscaleImages = new MagickImageCollection())
            {
                foreach (string jpgFile in Directory.GetFiles(jpegOutputPath, "*.jpg"))
                {
                    using (MagickImage image = new MagickImage(jpgFile))
                    {
                        image.ColorType = ColorType.Grayscale;
                        image.Density = new Density(300, DensityUnit.PixelsPerInch);
                        grayscaleImages.Add(image.Clone());
                    }
                }

                string outputPdfFileName = $"{inputPdfName}_digitalizado.pdf";

                // Ruta completa para el archivo PDF de salida en la misma ruta que el PDF de entrada
                string outputPdfPath = Path.Combine(inputPdfDirectory, outputPdfFileName);

                grayscaleImages.Write(outputPdfPath);
            }

            // Eliminar la carpeta con las imágenes
            Directory.Delete(jpegOutputPath, true);
        }
    }
}