﻿namespace Google_API_Integration.Interfaces;

public interface IGeminiService
{
    /// <summary>
    /// Generate content using the Gemini API.
    /// </summary>
    /// <param name="prompt">Prompt for the Gemini API</param>
    /// <param name="systemPrompt">Optional parameter for the AI"</param>
    /// <returns>Returns a string of the response from the AI</returns>
    Task<string> GenerateContentAsync(string prompt, string systemPrompt = "");

    /// <summary>
    /// Analyze the document content using the Gemini API.
    /// </summary>
    /// <param name="documentContent">String of the document's content</param>
    /// <returns>Returns a string with the AI's response</returns>
    Task<string> AnalyzeDocumentContentAsync(string documentContent);

    /// <summary>
    /// This method is used to complete an assignment based on the provided information.
    /// </summary>
    /// <param name="assignmentInformation">A dictionary that holds the description as the key and the information as the corresponding value</param>
    /// <param name="systemPrompt">This is an optional parameter to add to the system prompt of the AI</param>
    /// <returns>Returns a string with the returned value from the AI</returns>
    Task<string> CompleteAssignment(Dictionary<string, string> assignmentInformation, string systemPrompt = "");

    /// <summary>
    /// Summarize the submission content in relation to the assignment description.
    /// </summary>
    /// <param name="submissionContent">String that contains the content in the submission</param>
    /// <param name="assignmentDescription">String that is a description of the assignment</param>
    /// <returns>Returns a string of the AI's response</returns>
    Task<string> SummarizeSubmissionAsync(string submissionContent, string assignmentDescription);
}