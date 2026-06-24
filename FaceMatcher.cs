#pragma warning disable CA1416 // Membungkam peringatan kompatibilitas platform Windows

using System;
using System.Collections.Generic;
using System.IO;
using OpenCvSharp;
using OpenCvSharp.Face;

namespace EvacuationDashboard.Services
{
    public class FaceMatcher
    {
        private LBPHFaceRecognizer _recognizer;
        private string _folderPath;

        public FaceMatcher()
        {
            _folderPath = Path.Combine(FileSystem.AppDataDirectory, "FaceDatabase");
            _recognizer = LBPHFaceRecognizer.Create();
        }

        public void TrainSystem()
        {
            try
            {
                if (!Directory.Exists(_folderPath)) return;

                var images = new List<Mat>();
                var labels = new List<int>();

                // 1. Baca semua file kelompok 1 (Boleh Masuk) secara dinamis
                string[] filesGrup1 = Directory.GetFiles(_folderPath, "1_*.jpg");
                foreach (string file in filesGrup1)
                {
                    using (Mat gray = Cv2.ImRead(file, ImreadModes.Grayscale))
                    {
                        if (!gray.Empty())
                        {
                            Mat resized = new Mat();
                            // FIX: Mengunci nama menjadi OpenCvSharp.Size agar tidak tabrakan dengan MAUI
                            Cv2.Resize(gray, resized, new OpenCvSharp.Size(200, 200));
                            images.Add(resized);
                            labels.Add(1);
                        }
                    }
                }

                // 2. Baca semua file kelompok 2 (Dilarang Masuk / Penyusup) secara dinamis
                string[] filesGrup2 = Directory.GetFiles(_folderPath, "2_*.jpg");
                foreach (string file in filesGrup2)
                {
                    using (Mat gray = Cv2.ImRead(file, ImreadModes.Grayscale))
                    {
                        if (!gray.Empty())
                        {
                            Mat resized = new Mat();
                            // FIX: Mengunci nama menjadi OpenCvSharp.Size agar tidak tabrakan dengan MAUI
                            Cv2.Resize(gray, resized, new OpenCvSharp.Size(200, 200));
                            images.Add(resized);
                            labels.Add(2);
                        }
                    }
                }

                // Jalankan training hanya jika data gambar tersedia
                if (images.Count > 0)
                {
                    _recognizer.Train(images, labels);
                }
            }
            catch
            {
                // Mencegah aplikasi crash senyap
            }
        }

        public (int label, double confidence) PeriksaWajahSecaraPresisi(string imagePath)
        {
            try
            {
                using (Mat src = Cv2.ImRead(imagePath, ImreadModes.Grayscale))
                {
                    if (src.Empty()) return (-1, 999.0);

                    // Samakan ukuran wajah yang discan dengan ukuran data training (200x200)
                    using (Mat resized = new Mat())
                    {
                        // FIX: Mengunci nama menjadi OpenCvSharp.Size agar tidak tabrakan dengan MAUI
                        Cv2.Resize(src, resized, new OpenCvSharp.Size(200, 200));

                        int label = -1;
                        double confidence = 0.0;

                        _recognizer.Predict(resized, out label, out confidence);
                        return (label, confidence);
                    }
                }
            }
            catch
            {
                return (-1, 999.0);
            }
        }
    }
}