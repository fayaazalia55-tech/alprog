using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EvacuationDashboard
{
    public class GeminiService
    {
        private const string ApiKey = "AQ.Ab8RN6L84o7_z_E-7ZJfuNCJVBbGd3I2_DI3uHbHaCgHr1UzDA";
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";

        private readonly HttpClient _httpClient;

        public GeminiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> TanyaCopilot(string instruksiUser)
        {
            if (string.IsNullOrWhiteSpace(ApiKey) || ApiKey.StartsWith("PASTE_"))
            {
                return "⚠️ API Key belum terisi dengan benar di file GeminiService.cs!";
            }

            try
            {
                // PERBAIKAN PROMPT: Mengubah kepribadian AI menjadi cerdas, luwes, dan melarang penggunaan simbol Markdown
                string systemPrompt = "Kamu adalah AI Copilot Sistem Evakuasi Pintar Gedung Bertingkat yang sangat cerdas, responsif, dan profesional. " +
                                     "Tugasmu adalah membantu memberikan panduan keselamatan, menjawab pertanyaan telemetri, dan memberikan solusi evakuasi dengan bahasa yang natural dan informatif. " +
                                     "PERATURAN MUTLAK: JANGAN PERNAH menggunakan format markdown berupa tanda bintang (**) atau simbol markdown lainnya di dalam jawabanmu. " +
                                     "Gunakan teks biasa dengan susunan baris baru (Enter) yang rapi agar nyaman dipandang di layar aplikasi. " +
                                     "Pertanyaan user: " + instruksiUser;

                var payload = new
                {
                    contents = new[]
                    {
                        new { parts = new[] { new { text = systemPrompt } } }
                    }
                };

                string jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync($"{ApiUrl}{ApiKey}", content).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    using (JsonDocument doc = JsonDocument.Parse(jsonResponse))
                    {
                        string jawabanAI = doc.RootElement
                            .GetProperty("candidates")[0]
                            .GetProperty("content")
                            .GetProperty("parts")[0]
                            .GetProperty("text").GetString();

                        return jawabanAI?.Trim() ?? "Gemini mengirim respon kosong.";
                    }
                }
                else
                {
                    string errorDetails = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return $"🔴 Google Menolak (Status: {response.StatusCode}). Detail: {errorDetails}";
                }
            }
            catch (Exception ex)
            {
                return $"❌ Gagal tersambung ke server Google: {ex.Message}";
            }
        }
    }
}