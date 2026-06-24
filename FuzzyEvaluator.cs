using System;

namespace EvacuationDashboard
{
    public class FuzzyEvaluator
    {
        // ====================================================================
        // 1. FUZZIFIKASI: Menghitung Derajat Keanggotaan Kurva Grafik (0.0 sampai 1.0)
        // ====================================================================

        // Fungsi Keanggotaan SUHU
        private static double KurvaSuhuAman(double temp)
        {
            if (temp <= 30) return 1.0;
            if (temp > 30 && temp < 40) return (40 - temp) / (40 - 30);
            return 0.0;
        }

        private static double KurvaSuhuWaspada(double temp)
        {
            if (temp <= 30 || temp >= 55) return 0.0;
            if (temp > 30 && temp <= 42.5) return (temp - 30) / (42.5 - 30);
            return (55 - temp) / (55 - 42.5);
        }

        private static double KurvaSuhuBahaya(double temp)
        {
            if (temp <= 45) return 0.0;
            if (temp > 45 && temp < 60) return (temp - 45) / (60 - 45);
            return 1.0;
        }

        // Fungsi Keanggotaan ASAP
        private static double KurvaAsapAman(double smoke)
        {
            if (smoke <= 0.05) return 1.0;
            if (smoke > 0.05 && smoke < 0.20) return (0.20 - smoke) / (0.20 - 0.05);
            return 0.0;
        }

        private static double KurvaAsapWaspada(double smoke)
        {
            if (smoke <= 0.10 || smoke >= 0.45) return 0.0;
            if (smoke > 0.10 && smoke <= 0.25) return (smoke - 0.10) / (0.25 - 0.10);
            return (0.45 - smoke) / (0.45 - 0.25);
        }

        private static double KurvaAsapBahaya(double smoke)
        {
            if (smoke <= 0.35) return 0.0;
            if (smoke > 0.35 && smoke < 0.50) return (smoke - 0.35) / (0.50 - 0.35);
            return 1.0;
        }

        // ====================================================================
        // 2. INFERENSI (RULE EVALUATION) & 3. DEFUZZIFIKASI (WEIGHTED AVERAGE)
        // ====================================================================
        public static (double RiskPercentage, string StatusText, string ColorHex) JalankanAnalisisFuzzy(double temp, double smoke)
        {
            // Ambil nilai fuzzifikasi
            double uSuhuAman = KurvaSuhuAman(temp);
            double uSuhuWaspada = KurvaSuhuWaspada(temp);
            double uSuhuBahaya = KurvaSuhuBahaya(temp);

            double uAsapAman = KurvaAsapAman(smoke);
            double uAsapWaspada = KurvaAsapWaspada(smoke);
            double uAsapBahaya = KurvaAsapBahaya(smoke);

            double totalBobotAturan = 0;
            double totalNilaiPredikat = 0;

            // KETETAPAN OUTPUT SUGENO CONSTANT: Aman = 12.4%, Waspada = 48.2%, Bahaya Kritis = 99.9%

            // RULE 1: IF Suhu Aman AND Asap Aman THEN Risiko AMAN (12.4)
            double r1 = Math.Min(uSuhuAman, uAsapAman);
            totalNilaiPredikat += r1 * 12.4; totalBobotAturan += r1;

            // RULE 2: IF Suhu Waspada OR Asap Waspada THEN Risiko WASPADA (48.2)
            double r2 = Math.Max(uSuhuWaspada, uAsapWaspada);
            totalNilaiPredikat += r2 * 48.2; totalBobotAturan += r2;

            // RULE 3: IF Suhu Bahaya OR Asap Bahaya THEN Risiko BAHAYA CRITICAL (99.9)
            double r3 = Math.Max(uSuhuBahaya, uAsapBahaya);
            totalNilaiPredikat += r3 * 99.9; totalBobotAturan += r3;

            // RULE 4 (Pencegahan Silang): IF Suhu Aman AND Asap Waspada THEN Risiko WASPADA (35.0)
            double r4 = Math.Min(uSuhuAman, uAsapWaspada);
            totalNilaiPredikat += r4 * 35.0; totalBobotAturan += r4;

            // Perhitungan Akhir Defuzzifikasi (Mencari Nilai Riil Berbobot)
            double hasilCrispRisk = 12.4; // Nilai dasar aman jika tidak ada aturan terpicu
            if (totalBobotAturan > 0)
            {
                hasilCrispRisk = totalNilaiPredikat / totalBobotAturan;
            }

            // Menentukan Warna Banner dan Teks Status Berdasarkan Hasil Perhitungan Logika Fuzzy
            if (hasilCrispRisk < 25.0)
            {
                return (hasilCrispRisk, $"🟢 SAFE (Risk Level: {hasilCrispRisk:F1}% - All systems normal)", "#2ECC71");
            }
            else if (hasilCrispRisk >= 25.0 && hasilCrispRisk < 75.0)
            {
                return (hasilCrispRisk, $"🟡 STANDBY (Risk Level: {hasilCrispRisk:F1}% - High heat & smoke detected)", "#E67E22");
            }
            else
            {
                return (hasilCrispRisk, $"🚨 EVACUATE IMMEDIATELY (Risk Level: {hasilCrispRisk:F1}% - Critical danger alert!)", "#E74C3C");
            }
        }
    }
}
