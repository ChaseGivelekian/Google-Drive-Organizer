﻿using System.Text;
using System.Text.Json;
using Google_API_Integration.Interfaces;

namespace Google_API_Integration.Services.Gemini;

public class GeminiService(string apiKey) : IGeminiService
{
    private readonly HttpClient _httpClient = new();
    private readonly string _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));

    private const string ApiBaseUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent";

    public async Task<string> GenerateContentAsync(string prompt,
        string systemPrompt = "")
    {
        var requestContent = new
        {
            system_instruction = new
            {
                parts = new[]
                {
                    new
                    {
                        text = """
                               Format your response using the following structure:
                               1. Use 'FONT: ' to indicate the style of the response and end with ' :FONT_END' Put this at the end of the response.
                               2. Use 'SPACING: ' to indicate the spacing of the response and end with ' :SPACING_END' Put this at the end of the response.
                               3. Use 'SIZE: ' to indicate the size of the response and end with ' :SIZE_END' Put this at the end of the response.
                               4. Use '## HEADING: ' for section headings and end with ' :HEADING_END'
                               5. Use '### SUBHEADING: ' for subsection headings and end with ' :SUBHEADING_END'
                               6. Use 'PARAGRAPH: ' to start paragraphs and end with ' :PARAGRAPH_END'
                               7. Use 'BOLD: ' before bold text and end with ' :BOLD_END'
                               8. Use [**around text for inline bold text and end with**]
                               8. Use 'LIST_ITEM_BULLET: ' before each bullet point list item and end with ' :LIST_ITEM_END'
                               9. Use 'INDENT_FIRST_LINE: ' to indicate indentation
                               10. Use 'ALIGNMENT_START: ' to left align text and end with ' :ALIGNMENT_END'
                               11. Use 'ALIGNMENT_CENTER: ' to center align text and end with ' :ALIGNMENT_END'
                               12. Use 'ALIGNMENT_END: ' to right align text and end with ' :ALIGNMENT_END'

                               Examples:
                               ## HEADING: Introduction :HEADING_END
                               
                               ## HEADING: ALIGNMENT_CENTER: This is a centered heading :ALIGNMENT_END :HEADING_END
                               
                               ### SUBHEADING: Background :SUBHEADING_END
                               
                               PARAGRAPH: This is a normal paragraph text. :PARAGRAPH_END
                               
                               PARAGRAPH: INDENT_FIRST_LINE: This is an indented paragraph. :PARAGRAPH_END
                               
                               PARAGRAPH: [**This is a bold paragraph.**] :PARAGRAPH_END
                               
                               BOLD: This is an important section. :BOLD_END
                               
                               LIST_ITEM_BULLET: First point :LIST_ITEM_END
                               
                               LIST_ITEM_BULLET: Second point :LIST_ITEM_END
                               
                               LIST_ITEM_BULLET: [**This is a bold list item.**] :LIST_ITEM_END
                               
                               FONT: Times New Roman :FONT_END
                               SPACING: 2 :SPACING_END
                               SIZE: 12 :SIZE_END
                               
                               
                               1. Always remember to put the FONT, SPACING, AND SIZE at the end of the response.
                               2. Use BOLD: to indicate a bold section of text and end with :BOLD_END for inline bold text use [**bold text**].
                               3. Always follow the format above.
                               4. If it isn't specified use the default values of FONT: Times New Roman, SPACING: 1.5, SIZE: 12.
                               5. Write in first person. Don't specify who you are. Don't do this: "I, name, etc."
                               6. Always use the correct grammar and spelling.
                               7. Always indent paragraph elements unless otherwise told not to.
                               8. Center the title of the document unless otherwise told not to. Put the alignment information inside of the parent element ie. paragraphs, headers, subheadings, and list items.
                               """ + systemPrompt
                    }
                }
            },
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        return await SendRequestAsync(requestContent);
    }

    public async Task<string> AnalyzeDocumentContentAsync(string documentContent)
    {
        var prompt = $"Analyze the following document content: {documentContent}";
        return await GenerateContentAsync(prompt);
    }

    public async Task<string> CompleteAssignment(Dictionary<string, string> assignmentInformation, string systemPrompt = "")
    {
        var prompt =
            $"Complete this assignment from the students point of view based on the following information: Don't add anything else to your response outside of the assignment that you are being asked to complete. Remember to use lots of inline bold text, please! Here is the assignments information: {string.Join(",\n", assignmentInformation.Select(kv => $"{kv.Key}: {kv.Value}"))}";
        return await GenerateContentAsync(prompt, systemPrompt);
    }

    public async Task<string> SummarizeSubmissionAsync(string submissionContent, string assignmentDescription)
    {
        var prompt =
            $"Assignment description: {assignmentDescription}\n\nSubmission content: {submissionContent}\n\nPlease analyze this submission in relation to the assignment requirements and provide feedback.";
        return await GenerateContentAsync(prompt);
    }

    /// <summary>
    /// Sends a request to the Gemini API and returns the response.
    /// </summary>
    /// <param name="requestContent">Object that is in the Gemini REST format</param>
    /// <returns>Returns a string of the AI's response</returns>
    private async Task<string> SendRequestAsync(object requestContent)
    {
        var url = $"{ApiBaseUrl}?key={_apiKey}";

        var requestJson = JsonSerializer.Serialize(requestContent);
        var httpContent = new StringContent(requestJson, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(url, httpContent);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonSerializer.Deserialize<JsonElement>(responseJson);

        // Extract the text from the response
        if (responseObj.TryGetProperty("candidates", out var candidates) &&
            candidates.GetArrayLength() > 0 &&
            candidates[0].TryGetProperty("content", out var content) &&
            content.TryGetProperty("parts", out var parts) &&
            parts.GetArrayLength() > 0 &&
            parts[0].TryGetProperty("text", out var text))
        {
            return text.GetString() ?? string.Empty;
        }

        return string.Empty;
    }
}