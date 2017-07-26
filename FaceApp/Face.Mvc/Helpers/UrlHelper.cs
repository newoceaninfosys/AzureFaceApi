namespace Face.Mvc.Helpers
{
    public static class UrlHelper
    {
        public static string GetFaceUrl(string faceId)
        {
            return $"/Facing/{faceId}.jpg";
        }
    }
}
