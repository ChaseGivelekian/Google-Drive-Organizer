﻿using Google.Apis.Auth.OAuth2;
using Google.Apis.Classroom.v1;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace Google_Drive_Organizer.Services;

public static class GoogleCredentialsManager
{
    private static readonly string[] Scopes =
    [
        DriveService.Scope.DriveReadonly,
        ClassroomService.Scope.ClassroomCoursesReadonly,
        ClassroomService.Scope.ClassroomCourseworkMeReadonly
    ];

    private const string CredentialsPath = "credentials.json";
    private const string TokenPath = "token.json";

    private static async Task<UserCredential> GetUserCredentialAsync()
    {
        await using var stream = new FileStream(CredentialsPath, FileMode.Open, FileAccess.Read);
        var secrets = await GoogleClientSecrets.FromStreamAsync(stream);

        return await GoogleWebAuthorizationBroker.AuthorizeAsync(
            secrets.Secrets,
            Scopes,
            "user",
            CancellationToken.None,
            new FileDataStore(TokenPath, true));
    }

    public static async Task<DriveService> CreateDriveServiceAsync()
    {
        var credential = await GetUserCredentialAsync();
        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }

    public static async Task<ClassroomService> CreateClassroomServiceAsync()
    {
        var credential = await GetUserCredentialAsync();
        return new ClassroomService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential
        });
    }
}