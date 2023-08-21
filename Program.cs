using System;
using System.IO;
using ImageMagick;

namespace ImageMagickExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Configurar la ruta a la carpeta de Ghostscript
            string gsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "gs");
            MagickNET.SetGhostscriptDirectory(gsDirectory);

            // Ruta completa del archivo PDF de entrada
            string inputPdfPath = args[0];

            // Ruta donde se guardarán las imágenes JPEG convertidas
            //string jpegOutputPath = "C:\\Users\\Orlando\\Desktop\\reduccionPDF";

            // Ruta del directorio donde se encuentra el archivo ejecutable
            string executableDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Ruta donde se guardarán las imágenes JPEG convertidas (en el directorio del ejecutable)
            string jpegOutputPath = Path.Combine(executableDirectory, "reduccionPDF");

            // Crear el directorio de salida si no existe
            Directory.CreateDirectory(jpegOutputPath);

            // Configuración de conversión para generar imágenes
            MagickReadSettings settings = new MagickReadSettings();
            settings.Density = new Density(150);

            using (MagickImageCollection images = new MagickImageCollection())
            {
                // Leer el PDF y convertir cada página a imágenes
                images.Read(inputPdfPath, settings);

                int pageNumber = 1;
                foreach (MagickImage image in images)
                {
                    string outputImagePath = Path.Combine(jpegOutputPath, $"output-{pageNumber:D3}.jpg");
                    image.Quality = 10;
                    image.Write(outputImagePath);
                    pageNumber++;
                }
            }

            // Convertir imágenes a escala de grises y generar PDF
            using (MagickImageCollection grayscaleImages = new MagickImageCollection())
            {
                foreach (string jpgFile in Directory.GetFiles(jpegOutputPath, "*.jpg"))
                {
                    using (MagickImage image = new MagickImage(jpgFile))
                    {
                        image.ColorType = ColorType.Grayscale;
                        image.Density = new Density(300, DensityUnit.PixelsPerInch);
                        grayscaleImages.Add(image.Clone()); // Clonar la imagen para evitar el error de objeto desechado
                    }
                }

                // Obtener el nombre del archivo sin la extensión
                string inputFileName = Path.GetFileNameWithoutExtension(inputPdfPath);

                // Construir el nuevo nombre de archivo con "_digitalizado" y ".pdf" añadidos
                string outputPdfFileName = $"{inputFileName}_digitalizado.pdf";

                // Ruta completa para el archivo de salida PDF
                string outputPdfPath = Path.Combine(outputPdfFileName);

                grayscaleImages.Write(outputPdfPath);
            }


            // Eliminar contenido de la carpeta "reduccionPDF"
            foreach (string file in Directory.GetFiles(jpegOutputPath))
            {
                File.Delete(file);
            }

            Console.WriteLine("Conversión completada.");
        }
    }
}
