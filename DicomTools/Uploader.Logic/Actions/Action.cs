namespace Uploader.Logic.Actions
{
    internal enum Action
    {
        None = 0,

        ListLocalDir = 1,

        ListRemoteDir = 2,

        VerifyLocalFiles = 4,

        UploadLocalFiles = 5,

        ApplyCorrections = 6,
    }
}